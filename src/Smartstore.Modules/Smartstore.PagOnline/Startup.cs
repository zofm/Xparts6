using Microsoft.Extensions.DependencyInjection;
using Smartstore.Engine;
using Smartstore.Engine.Builders;
using Smartstore.PagOnline.Services;

namespace Smartstore.PagOnline;

internal class Startup : StarterBase
{
    public override void ConfigureServices(IServiceCollection services, IApplicationContext appContext)
    {
        if (appContext.IsInstalled)
        {
            services.AddScoped<IPagOnlineService, PagOnlineService>();
        }
    }
}
