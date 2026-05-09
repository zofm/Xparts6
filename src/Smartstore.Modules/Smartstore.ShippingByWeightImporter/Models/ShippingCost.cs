namespace Smartstore.ShippingByWeightImporter.Models
{
    public class ShippingCost
    {
        public int StoreId { get; set; }
        public int CountryId { get; set; }
        public string Zip { get; set; }
        public int ShippingMethodId { get; set; }
        public decimal WeightFrom { get; set; }
        public decimal WeightTo { get; set; }
        public bool UsePercentage { get; set; }
        public decimal ChargePercentage { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal SurchargeSmallQuantities { get; set; }
        public decimal ThresholdSmallQuantities { get; set; }
    }
}
