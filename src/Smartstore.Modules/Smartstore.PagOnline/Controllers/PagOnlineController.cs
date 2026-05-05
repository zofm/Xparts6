using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smartstore.ComponentModel;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Checkout.Payment;
using Smartstore.Core.Data;
using Smartstore.Core.Security;
using Smartstore.Core.Stores;
using Smartstore.PagOnline.Models;
using Smartstore.PagOnline.Services;
using Smartstore.PagOnline.Settings;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;

namespace Smartstore.PagOnline.Controllers
{
    [Area("Admin")]
    public class PagOnlineController : ModuleController
    {
        private readonly SmartDbContext _db;
        private readonly IStoreContext _storeContext;
        private readonly IPagOnlineService _pagOnlineService;
        private readonly IOrderProcessingService _orderProcessingService;

        public PagOnlineController(
            SmartDbContext db,
            IStoreContext storeContext,
            IPagOnlineService pagOnlineService,
            IOrderProcessingService orderProcessingService)
        {
            _db = db;
            _storeContext = storeContext;
            _pagOnlineService = pagOnlineService;
            _orderProcessingService = orderProcessingService;
        }

        [LoadSetting, AuthorizeAdmin]
        public IActionResult Configure(PagOnlineSettings settings)
        {
            var model = MiniMapper.Map<PagOnlineSettings, ConfigurationModel>(settings);
            return View(model);
        }

        [HttpPost, SaveSetting, AuthorizeAdmin]
        public IActionResult Configure(PagOnlineSettings settings, ConfigurationModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return RedirectToAction(nameof(Configure));
        }

        [HttpGet]
        public async Task<IActionResult> VerifyOrder(int id)
        {
            var store = _storeContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;

            var order = await _db.Orders
                .Where(x => x.Id == id && x.StoreId == store.Id && x.CustomerId == customer.Id)
                .FirstOrDefaultAsync();

            if (order != null)
            {
                var verifyResponse = await _pagOnlineService.VerifyAsync(new PagOnlinePaymentVerifyRequest
                {
                    OrderId = id,
                    StoreId = store.Id
                });

                if (verifyResponse.Success)
                {
                    order.PaymentStatus = PaymentStatus.Authorized;
                    order.AuthorizationTransactionId = verifyResponse.TranID.ToString();
                    _db.OrderNotes.Add(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"Authorization ({verifyResponse.AuthStatus}) accepted from POS with transaction ID: {verifyResponse.TranID} (ENR status: {verifyResponse.EnrStatus})",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    await _db.SaveChangesAsync();
                    await _orderProcessingService.MarkAsAuthorizedAsync(order);

                    return RedirectToAction("Completed", "Checkout", new { area = "" });
                }
            }

            return RedirectToAction("Details", "Order", new { id, area = "" });
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var store = _storeContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;

            var order = await _db.Orders
                .Where(x => x.StoreId == store.Id && x.CustomerId == customer.Id)
                .OrderByDescending(x => x.CreatedOnUtc)
                .FirstOrDefaultAsync();

            if (order != null)
            {
                return RedirectToAction("Details", "Order", new { id = order.Id, area = "" });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
