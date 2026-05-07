using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CessionarioCommittente;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class RappresentanteFiscaleCessionarioCommittenteValidator : DenominazioneNomeCognomeValidator<RappresentanteFiscaleCessionarioCommittente>
    {
        public RappresentanteFiscaleCessionarioCommittenteValidator()
        {
            RuleFor(x => x.IdFiscaleIVA)
                .SetValidator(new IdFiscaleIVAValidator());
        }
    }
}
