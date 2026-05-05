using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.Checkout.Cart;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Checkout.Payment;
using Smartstore.Core.Configuration;
using Smartstore.Core.Stores;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;
using Smartstore.Http;
using Smartstore.PagOnline.Components;
using Smartstore.PagOnline.Controllers;
using Smartstore.PagOnline.Services;
using Smartstore.PagOnline.Settings;

namespace Smartstore.PagOnline.Providers
{
    [SystemName("Payments.PagOnline")]
    [FriendlyName("PagOnline")]
    [Order(1)]
    [PaymentMethod(PaymentMethodType.Redirection)]
    public class PagOnlineProvider : PaymentMethodBase, IConfigurable
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingFactory _settingFactory;
        private readonly IPagOnlineService _pagOnlineService;

        public PagOnlineProvider(
            IStoreContext storeContext,
            ISettingFactory settingFactory,
            IPagOnlineService pagOnlineService)
        {
            _storeContext = storeContext;
            _settingFactory = settingFactory;
            _pagOnlineService = pagOnlineService;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public static string SystemName => "Payments.PagOnline";

        public override bool SupportCapture => true;
        public override bool SupportPartiallyRefund => true;
        public override bool SupportRefund => true;
        public override bool SupportVoid => true;
        public override bool RequiresInteraction => false;

        public RouteInfo GetConfigurationRoute()
            => new(nameof(PagOnlineController.Configure), "PagOnline", new { area = "Admin" });

        public override Widget GetPaymentInfoWidget()
            => new ComponentWidget(typeof(PagOnlinePaymentInfoViewComponent));

        public override async Task<(decimal FixedFeeOrPercentage, bool UsePercentage)> GetPaymentFeeInfoAsync(ShoppingCart cart)
        {
            var settings = await _settingFactory.LoadSettingsAsync<PagOnlineSettings>(_storeContext.CurrentStore.Id);
            return (settings.AdditionalFee, settings.AdditionalFeePercentage);
        }

        public override Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Pending
            });
        }

        public override async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return;

            var settings = await _settingFactory.LoadSettingsAsync<PagOnlineSettings>(order.StoreId);

            var language = order.CustomerLanguageId == 2 ? "IT" : "EN";

            var initRequest = new PagOnlinePaymentInitRequest
            {
                OrderId = order.Id,
                StoreId = order.StoreId,
                Amount = order.OrderTotal,
                CustomerEmail = order.BillingAddress?.Email ?? string.Empty,
                CustomerName = order.BillingAddress != null
                    ? $"{order.BillingAddress.FirstName} {order.BillingAddress.LastName}".Trim()
                    : string.Empty,
                Language = language
            };

            var response = await _pagOnlineService.InitAsync(initRequest);

            if (response.Success)
            {
                postProcessPaymentRequest.RedirectUrl = response.RedirectUrl;
            }
            else
            {
                Logger.Error(response.ErrorDescription);
            }
        }

        public override async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return order.PaymentStatus == PaymentStatus.Pending && (DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes >= 1.0;
        }

        public override async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var order = capturePaymentRequest.Order;
            var result = new CapturePaymentResult
            {
                NewPaymentStatus = order.PaymentStatus
            };

            if (long.TryParse(order.AuthorizationTransactionId, out long tranId))
            {
                var response = await _pagOnlineService.ConfirmAsync(new PagOnlinePaymentConfirmRequest
                {
                    OrderId = order.Id,
                    StoreId = order.StoreId,
                    Amount = order.OrderTotal,
                    TranID = tranId
                });

                if (response.Success)
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    result.CaptureTransactionId = response.TranID.ToString();
                    result.CaptureTransactionResult = response.Rc;
                }
                else
                {
                    Logger.Error(response.ErrorDescription);
                    throw new PaymentException(response.ErrorDescription, SystemName);
                }
            }

            return result;
        }

        public override async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;
            var result = new RefundPaymentResult
            {
                NewPaymentStatus = order.PaymentStatus
            };

            if (long.TryParse(order.CaptureTransactionId, out long tranId))
            {
                var response = await _pagOnlineService.RefundAsync(new PagOnlinePaymentRefundRequest
                {
                    OrderId = order.Id,
                    StoreId = order.StoreId,
                    Amount = (decimal)refundPaymentRequest.AmountToRefund,
                    TranID = tranId
                });

                if (response.Success)
                {
                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund
                        ? PaymentStatus.PartiallyRefunded
                        : PaymentStatus.Refunded;
                }
                else
                {
                    Logger.Error(response.ErrorDescription);
                    throw new PaymentException(response.ErrorDescription, SystemName);
                }
            }

            return result;
        }

        public override async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var order = voidPaymentRequest.Order;
            var result = new VoidPaymentResult
            {
                NewPaymentStatus = order.PaymentStatus
            };

            if (long.TryParse(order.AuthorizationTransactionId, out long tranId))
            {
                var response = await _pagOnlineService.CancelAsync(new PagOnlinePaymentCancelRequest
                {
                    OrderId = order.Id,
                    StoreId = order.StoreId,
                    Amount = order.OrderTotal,
                    TranID = tranId
                });

                if (response.Success)
                {
                    result.NewPaymentStatus = PaymentStatus.Voided;
                }
                else
                {
                    Logger.Error(response.ErrorDescription);
                    throw new PaymentException(response.ErrorDescription, SystemName);
                }
            }

            return result;
        }
    }
}
