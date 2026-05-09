using Microsoft.Extensions.DependencyInjection;
using Smartstore.Engine;
using Smartstore.Engine.Builders;
using Smartstore.ShippingByWeightImporter.Services;
using Smartstore.ShippingByWeightImporter.Settings;

namespace Smartstore.ShippingByWeightImporter
{
    internal class Startup : StarterBase
    {
        public override void ConfigureServices(IServiceCollection services, IApplicationContext appContext)
        {
            if (appContext.IsInstalled)
            {
                services.AddScoped<IShippingByWeightImporterService, ShippingByWeightImporterService>();
            }
        }
    }
}
