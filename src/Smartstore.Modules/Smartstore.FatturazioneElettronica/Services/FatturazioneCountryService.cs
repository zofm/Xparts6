using Smartstore.Core.Common;
using Smartstore.Core.Data;

namespace Smartstore.FatturazioneElettronica.Services
{
    public class FatturazioneCountryService : IFatturazioneCountryService
    {
        private readonly SmartDbContext _db;

        public FatturazioneCountryService(SmartDbContext db)
        {
            _db = db;
        }

        public List<string> GetEuropeanCountryTwoLetterIsoCodes()
        {
            return new List<string>
            {
                "AT", "BE", "BG", "CY", "HR", "DK", "EE", "FI", "FR", "DE",
                "GB", "EL", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL",
                "PT", "CZ", "SK", "RO", "SI", "ES", "SE", "HU"
            };
        }

        public List<int> GetEuropeanCountryIds()
        {
            var list = GetEuropeanCountryTwoLetterIsoCodes();
            return _db.Set<Country>()
                .Where(x => list.Contains(x.TwoLetterIsoCode))
                .Select(x => x.Id)
                .ToList();
        }
    }
}
