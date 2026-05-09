using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.Common;
using Smartstore.Core.Data;
using Smartstore.Scheduling;
using Smartstore.StateOrProvinceImporter.Settings;

namespace Smartstore.StateOrProvinceImporter.Tasks
{
    public class StateOrProvinceImporterTask : ITask
    {
        private static readonly Dictionary<string, List<string>> AcceptedTypes = new()
        {
            { "AR", ["province", "city"] },
            { "AU", ["territory", "state"] },
            { "CA", ["territory", "province"] },
            { "CN", ["municipality", "province", "autonomous region", "special administrative region"] },
            { "CO", ["", "capital district", "department"] },
            { "CY", ["district"] },
            { "EC", ["province"] },
            { "EG", ["governorate"] },
            { "FR", ["metropolitan region", "metropolitan department", "overseas region"] },
            { "GH", ["region"] },
            { "KE", ["county"] },
            { "NI", ["department", "autonomous region"] },
            { "NG", ["state", "capital territory"] },
            { "CH", ["canton"] },
            { "TW", ["special municipality"] },
            { "TZ", ["Region"] },
            { "TR", ["province"] },
            { "VE", ["state", "capital district"] }
        };

        private static readonly Dictionary<string, string> ItalianProvinces = new()
        {
            { "AG", "Agrigento" }, { "AL", "Alessandria" }, { "AN", "Ancona" }, { "AO", "Aosta" },
            { "AR", "Arezzo" }, { "AP", "Ascoli Piceno" }, { "AT", "Asti" }, { "AV", "Avellino" },
            { "BA", "Bari" }, { "BT", "Barletta-Andria-Trani" }, { "BL", "Belluno" }, { "BN", "Benevento" },
            { "BG", "Bergamo" }, { "BI", "Biella" }, { "BO", "Bologna" }, { "BZ", "Bolzano" },
            { "BS", "Brescia" }, { "BR", "Brindisi" }, { "CA", "Cagliari" }, { "CL", "Caltanissetta" },
            { "CB", "Campobasso" }, { "CI", "Carbonia-Iglesias" }, { "CE", "Caserta" }, { "CT", "Catania" },
            { "CZ", "Catanzaro" }, { "CH", "Chieti" }, { "CO", "Como" }, { "CS", "Cosenza" },
            { "CR", "Cremona" }, { "KR", "Crotone" }, { "CN", "Cuneo" }, { "EN", "Enna" },
            { "FM", "Fermo" }, { "FE", "Ferrara" }, { "FI", "Firenze" }, { "FG", "Foggia" },
            { "FC", "Forlì-Cesena" }, { "FR", "Frosinone" }, { "GE", "Genova" }, { "GO", "Gorizia" },
            { "GR", "Grosseto" }, { "IM", "Imperia" }, { "IS", "Isernia" }, { "SP", "La Spezia" },
            { "AQ", "L'Aquila" }, { "LT", "Latina" }, { "LE", "Lecce" }, { "LC", "Lecco" },
            { "LI", "Livorno" }, { "LO", "Lodi" }, { "LU", "Lucca" }, { "MC", "Macerata" },
            { "MN", "Mantova" }, { "MS", "Massa-Carrara" }, { "MT", "Matera" }, { "ME", "Messina" },
            { "MI", "Milano" }, { "MO", "Modena" }, { "MB", "Monza e della Brianza" }, { "NA", "Napoli" },
            { "NO", "Novara" }, { "NU", "Nuoro" }, { "OT", "Olbia-Tempio" }, { "OR", "Oristano" },
            { "PD", "Padova" }, { "PA", "Palermo" }, { "PR", "Parma" }, { "PV", "Pavia" },
            { "PG", "Perugia" }, { "PU", "Pesaro e Urbino" }, { "PE", "Pescara" }, { "PC", "Piacenza" },
            { "PI", "Pisa" }, { "PT", "Pistoia" }, { "PN", "Pordenone" }, { "PZ", "Potenza" },
            { "PO", "Prato" }, { "RG", "Ragusa" }, { "RA", "Ravenna" }, { "RC", "Reggio Calabria" },
            { "RE", "Reggio Emilia" }, { "RI", "Rieti" }, { "RN", "Rimini" }, { "RM", "Roma" },
            { "RO", "Rovigo" }, { "SA", "Salerno" }, { "VS", "Medio Campidano" }, { "SS", "Sassari" },
            { "SV", "Savona" }, { "SI", "Siena" }, { "SR", "Siracusa" }, { "SO", "Sondrio" },
            { "TA", "Taranto" }, { "TE", "Teramo" }, { "TR", "Terni" }, { "TO", "Torino" },
            { "OG", "Ogliastra" }, { "TP", "Trapani" }, { "TN", "Trento" }, { "TV", "Treviso" },
            { "TS", "Trieste" }, { "UD", "Udine" }, { "VA", "Varese" }, { "VE", "Venezia" },
            { "VB", "Verbano-Cusio-Ossola" }, { "VC", "Vercelli" }, { "VR", "Verona" },
            { "VV", "Vibo Valentia" }, { "VI", "Vicenza" }, { "VT", "Viterbo" }
        };

        private readonly SmartDbContext _db;
        private readonly StateOrProvinceImporterSettings _settings;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public StateOrProvinceImporterTask(SmartDbContext db, StateOrProvinceImporterSettings settings)
        {
            _db = db;
            _settings = settings;
        }

        public async Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var filePath = _settings.CsvFilePath;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Logger.LogDebug("No CSV file found at path: \"{Path}\"", filePath);
                return;
            }

            var countries = await _db.Countries.AsNoTracking().ToListAsync(cancelToken);
            var countryIndex = countries.ToDictionary(c => c.TwoLetterIsoCode);

            var lines = await File.ReadAllLinesAsync(filePath, cancelToken);

            Logger.LogInformation("START - Importing states/provinces from {File}", Path.GetFileName(filePath));

            int inserted = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                var columns = lines[i].Split(',');
                if (columns.Length < 7)
                    continue;

                var countryCode = columns[3];
                var provinceName = columns[1].Replace("\"", string.Empty);
                var provinceCode = columns[5];
                var provinceType = columns[6].Replace("\"", string.Empty);

                if (countryCode == "IT")
                    continue;

                if (AcceptedTypes.TryGetValue(countryCode, out var allowed) && !allowed.Contains(provinceType))
                    continue;

                if (!countryIndex.TryGetValue(countryCode, out var country))
                    continue;

                var exists = await _db.StateProvinces
                    .AnyAsync(x => x.CountryId == country.Id && x.Abbreviation == provinceCode, cancelToken);

                if (!exists)
                {
                    _db.StateProvinces.Add(new StateProvince
                    {
                        Abbreviation = provinceCode,
                        Name = provinceName,
                        CountryId = country.Id,
                        Published = true,
                        DisplayOrder = 0
                    });
                    inserted++;
                }
            }

            // Import Italian provinces
            if (countryIndex.TryGetValue("IT", out var italy))
            {
                var existingItCodes = await _db.StateProvinces
                    .Where(x => x.CountryId == italy.Id)
                    .Select(x => x.Abbreviation)
                    .ToListAsync(cancelToken);

                var existingSet = new HashSet<string>(existingItCodes, StringComparer.OrdinalIgnoreCase);

                foreach (var (code, name) in ItalianProvinces)
                {
                    if (!existingSet.Contains(code))
                    {
                        _db.StateProvinces.Add(new StateProvince
                        {
                            Abbreviation = code,
                            Name = name,
                            CountryId = italy.Id,
                            Published = true,
                            DisplayOrder = 0
                        });
                        inserted++;
                    }
                }
            }

            await _db.SaveChangesAsync(cancelToken);

            Logger.LogInformation("END - Inserted {Count} states/provinces.", inserted);
        }
    }
}
