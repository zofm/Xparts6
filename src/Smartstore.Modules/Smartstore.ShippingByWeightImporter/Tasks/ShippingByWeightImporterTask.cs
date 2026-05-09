using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.DataExchange.Excel;
using Smartstore.Scheduling;
using Smartstore.ShippingByWeightImporter.Models;
using Smartstore.ShippingByWeightImporter.Services;
using Smartstore.ShippingByWeightImporter.Settings;

namespace Smartstore.ShippingByWeightImporter.Tasks
{
    public class ShippingByWeightImporterTask : ITask
    {
        private readonly IShippingByWeightImporterService _importerService;
        private readonly ShippingByWeightImporterSettings _settings;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ShippingByWeightImporterTask(
            IShippingByWeightImporterService importerService,
            ShippingByWeightImporterSettings settings)
        {
            _importerService = importerService;
            _settings = settings;
        }

        public async Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var file = new FileInfo(_settings.ExcelFilePath ?? string.Empty);

            if (!file.Exists)
            {
                Logger.LogDebug("No file exists at path: \"{Path}\"", file.FullName);
                return;
            }

            using var stream = file.OpenRead();
            using var reader = new ExcelReader(stream, hasHeaders: true, defaultColumnName: "Column");

            var countries = await _importerService.GetCountriesAsync();
            var methods = await _importerService.GetShippingMethodsAsync();

            Logger.LogInformation("START - Importing shipping costs from {File}", file.Name);

            var items = new List<ShippingCost>();
            int rowIndex = 0;

            while (reader.Read())
            {
                if (reader.FieldCount < 11)
                {
                    Logger.LogWarning("Invalid number of columns ({Count}), expected 11.", reader.FieldCount);
                    return;
                }

                var countryName = reader.GetValue(1).Convert<string>();
                var country = countries.FirstOrDefault(x => x.Name == countryName);
                if (country == null)
                {
                    Logger.LogWarning("Unable to find COUNTRY with name \"{Name}\", row {Row}.", countryName, rowIndex);
                    rowIndex++;
                    continue;
                }

                var methodName = reader.GetValue(3).Convert<string>();
                var method = methods.FirstOrDefault(x => x.Name == methodName);
                if (method == null)
                {
                    Logger.LogWarning("Unable to find SHIPPING METHOD with name \"{Name}\", row {Row}.", methodName, rowIndex);
                    rowIndex++;
                    continue;
                }

                items.Add(new ShippingCost
                {
                    StoreId = 0,
                    CountryId = country.Id,
                    Zip = reader.GetValue(2).Convert<string>(),
                    ShippingMethodId = method.Id,
                    WeightFrom = reader.GetValue(4).Convert<decimal>(),
                    WeightTo = reader.GetValue(5).Convert<decimal>(),
                    UsePercentage = reader.GetValue(6).Convert<bool>(),
                    ChargePercentage = reader.GetValue(7).Convert<decimal>(),
                    ChargeAmount = reader.GetValue(8).Convert<decimal>(),
                    SurchargeSmallQuantities = reader.GetValue(9).Convert<decimal>(),
                    ThresholdSmallQuantities = reader.GetValue(10).Convert<decimal>()
                });

                rowIndex++;
            }

            if (items.Count == 0)
            {
                Logger.LogWarning("No valid rows found in the file.");
                return;
            }

            try
            {
                await _importerService.UpdateShippingCostsAsync(items);
                Logger.LogInformation("END - Imported {Count} shipping cost records.", items.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while saving imported shipping costs.");
            }
        }
    }
}
