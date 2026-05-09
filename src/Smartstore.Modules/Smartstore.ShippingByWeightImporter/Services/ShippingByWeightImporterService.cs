using Smartstore.Core.Common;
using Smartstore.Core.Checkout.Shipping;
using Smartstore.Core.Data;
using Smartstore.ShippingByWeight;
using Smartstore.ShippingByWeight.Domain;
using Smartstore.ShippingByWeightImporter.Models;

namespace Smartstore.ShippingByWeightImporter.Services
{
    public interface IShippingByWeightImporterService
    {
        Task<List<Country>> GetCountriesAsync();
        Task<List<ShippingMethod>> GetShippingMethodsAsync();
        Task UpdateShippingCostsAsync(IEnumerable<ShippingCost> items);
    }

    public class ShippingByWeightImporterService : IShippingByWeightImporterService
    {
        private readonly SmartDbContext _db;

        public ShippingByWeightImporterService(SmartDbContext db)
        {
            _db = db;
        }

        public Task<List<Country>> GetCountriesAsync()
            => _db.Countries.ToListAsync();

        public Task<List<ShippingMethod>> GetShippingMethodsAsync()
            => _db.ShippingMethods.ToListAsync();

        public async Task UpdateShippingCostsAsync(IEnumerable<ShippingCost> items)
        {
            await _db.ShippingRatesByWeight().ExecuteDeleteAsync();

            foreach (var item in items)
            {
                _db.ShippingRatesByWeight().Add(new ShippingRateByWeight
                {
                    StoreId = item.StoreId,
                    CountryId = item.CountryId,
                    Zip = item.Zip,
                    ShippingMethodId = item.ShippingMethodId,
                    From = item.WeightFrom,
                    To = item.WeightTo,
                    UsePercentage = item.UsePercentage,
                    ShippingChargePercentage = item.ChargePercentage,
                    ShippingChargeAmount = item.ChargeAmount,
                    SmallQuantitySurcharge = item.SurchargeSmallQuantities,
                    SmallQuantityThreshold = item.ThresholdSmallQuantities
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
