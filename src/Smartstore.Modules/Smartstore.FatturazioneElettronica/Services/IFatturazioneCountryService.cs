namespace Smartstore.FatturazioneElettronica.Services
{
    public interface IFatturazioneCountryService
    {
        List<string> GetEuropeanCountryTwoLetterIsoCodes();
        List<int> GetEuropeanCountryIds();
    }
}
