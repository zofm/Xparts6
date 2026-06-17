using it.netsw.apps.igfs.cg.coms.api.init;
using it.netsw.apps.igfs.cg.coms.api.tran;
using Smartstore.Core.Common.Services;
using Smartstore.Core.Web;
using Smartstore.PagOnline.Providers;
using Smartstore.PagOnline.Settings;

namespace Smartstore.PagOnline.Services
{
    public class PagOnlineService : IPagOnlineService
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly PagOnlineSettings _settings;

        public PagOnlineService(
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            PagOnlineSettings settings)
        {
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _settings = settings;
        }

        private string FormatOrderId(int orderId)
        {
            if (_settings.UseSandbox)
                return "TEST_" + orderId.ToString();

            return orderId.ToString();
        }

        private long RemoveDecimal(decimal amount)
            => Convert.ToInt64(amount * 100);

        private string CancelOrderUrl(int orderId)
            => _webHelper.GetStoreLocation() + $"PagOnline/CancelOrder?id={orderId}";

        private string VerifyOrderUrl(int orderId)
            => _webHelper.GetStoreLocation() + $"PagOnline/VerifyOrder?id={orderId}";

        public async Task<PagOnlineInitResponse> InitAsync(PagOnlinePaymentInitRequest request)
        {
            var uri = new Uri(_settings.WebServiceUrl);
            var svc = new IgfsCgInit(uri)
            {
                Tid = _settings.Tid,
                KSig = _settings.Ksig,
                ShopID = FormatOrderId(request.OrderId),
                ShopUserRef = request.CustomerEmail,
                ShopUserName = request.CustomerName,
                TrType = it.netsw.apps.igfs.cg.coms.api.TrType.AUTH,
                Amount = RemoveDecimal(request.Amount),
                CurrencyCode = it.netsw.apps.igfs.cg.coms.api.CurrencyCode.EUR,
                LangID = request.Language == "IT"
                    ? it.netsw.apps.igfs.cg.coms.api.LangID.IT
                    : it.netsw.apps.igfs.cg.coms.api.LangID.EN,
                ErrorURL = new Uri(CancelOrderUrl(request.OrderId)),
                NotifyURL = new Uri(VerifyOrderUrl(request.OrderId)),
                AddInfo1 = request.StoreId.ToString()
            };

            if (!svc.execute() || svc.Error)
            {
                return new PagOnlineInitResponse
                {
                    ErrorDescription = svc.ErrorDesc,
                    Rc = svc.Rc
                };
            }

            // storing ID for this payment
            var attrs = _genericAttributeService.GetAttributesForEntity("Order", request.OrderId);
            attrs.Set(PagOnlineProvider.SystemName + ".PaymentID", svc.PaymentID, request.StoreId);
            attrs.Set(PagOnlineProvider.SystemName + ".Status", "Init", request.StoreId);
            await attrs.SaveChangesAsync();

            return new PagOnlineInitResponse
            {
                Success = true,
                Rc = svc.Rc,
                RedirectUrl = svc.RedirectURL?.ToString()
            };
        }

        public async Task<PagOnlineVerifyResponse> VerifyAsync(PagOnlinePaymentVerifyRequest request)
        {
            var attrs = _genericAttributeService.GetAttributesForEntity("Order", request.OrderId);
            var paymentID = attrs.Get<string>(PagOnlineProvider.SystemName + ".PaymentID", request.StoreId);
            if (paymentID == null)
            {
                return new PagOnlineVerifyResponse
                {
                    Success = false,
                    ErrorDescription = "Payment not found!"
                };
            }

            var uri = new Uri(_settings.WebServiceUrl);
            var svc = new IgfsCgVerify(uri)
            {
                Tid = _settings.Tid,
                KSig = _settings.Ksig,
                ShopID = FormatOrderId(request.OrderId),
                PaymentID = paymentID
            };

            if (!svc.execute() || svc.Error)
            {
                return new PagOnlineVerifyResponse
                {
                    ErrorDescription = svc.ErrorDesc,
                    Rc = svc.Rc
                };
            }

            // saving status for this payment
            attrs.Set(PagOnlineProvider.SystemName + ".Status", "Verified");
            await attrs.SaveChangesAsync();

            return new PagOnlineVerifyResponse
            {
                Success = true,
                Rc = svc.Rc,
                TranID = svc.TranID.Value,
                EnrStatus = svc.EnrStatus,
                AuthStatus = svc.AuthStatus
            };
        }

        public Task<PagOnlineConfirmResponse> ConfirmAsync(PagOnlinePaymentConfirmRequest request)
        {
            var uri = new Uri(_settings.WebServiceUrl);
            var svc = new IgfsCgConfirm(uri)
            {
                Tid = _settings.Tid,
                KSig = _settings.Ksig,
                ShopID = FormatOrderId(request.OrderId),
                RefTranID = request.TranID,
                Amount = RemoveDecimal(request.Amount)
            };

            if (!svc.execute() || svc.Error)
            {
                return Task.FromResult(new PagOnlineConfirmResponse
                {
                    ErrorDescription = svc.ErrorDesc,
                    Rc = svc.Rc
                });
            }

            return Task.FromResult(new PagOnlineConfirmResponse
            {
                Success = true,
                Rc = svc.Rc,
                TranID = svc.TranID.Value
            });
        }

        public Task<PagOnlineCancelResponse> CancelAsync(PagOnlinePaymentCancelRequest request)
        {
            var uri = new Uri(_settings.WebServiceUrl);
            var svc = new IgfsCgVoidAuth(uri)
            {
                Tid = _settings.Tid,
                KSig = _settings.Ksig,
                ShopID = FormatOrderId(request.OrderId),
                RefTranID = request.TranID,
                Amount = RemoveDecimal(request.Amount)
            };

            if (!svc.execute())
            {
                return Task.FromResult(new PagOnlineCancelResponse
                {
                    ErrorDescription = svc.ErrorDesc,
                    Rc = svc.Rc
                });
            }

            return Task.FromResult(new PagOnlineCancelResponse
            {
                Success = true,
                Rc = svc.Rc
            });
        }

        public Task<PagOnlineRefundResponse> RefundAsync(PagOnlinePaymentRefundRequest request)
        {
            var uri = new Uri(_settings.WebServiceUrl);
            var svc = new IgfsCgCredit(uri)
            {
                Tid = _settings.Tid,
                KSig = _settings.Ksig,
                ShopID = FormatOrderId(request.OrderId),
                RefTranID = request.TranID,
                Amount = RemoveDecimal(request.Amount)
            };

            if (!svc.execute())
            {
                return Task.FromResult(new PagOnlineRefundResponse
                {
                    ErrorDescription = svc.ErrorDesc,
                    Rc = svc.Rc
                });
            }

            return Task.FromResult(new PagOnlineRefundResponse
            {
                Success = true,
                Rc = svc.Rc
            });
        }
    }
}
