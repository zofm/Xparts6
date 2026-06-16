using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Data;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.Catalog.Attributes;
using Smartstore.Core.Data;
using Smartstore.Core.DataExchange.Excel;
using Smartstore.MotoImporter.Settings;
using Smartstore.Scheduling;

namespace Smartstore.MotoImporter.Tasks
{
    /// <summary>
    /// Imports motorbike compatibility as product variant attribute values from Excel files.
    /// Each Excel row must have at least 3 columns: col[0]=Brand, col[1]=Model, col[2]=Year (or variant),
    /// and col with header "ID" containing the product identifier.
    /// </summary>
    public class ImportMotorbikeAttributesTask : ITask
    {
        private const string MotoAttributeAlias = "moto-only";
        private const string MotoAttributeName = "Moto";

        private readonly SmartDbContext _db;
        private readonly MotoImporterSettings _settings;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public ImportMotorbikeAttributesTask(SmartDbContext db, MotoImporterSettings settings)
        {
            _db = db;
            _settings = settings;
        }

        public async Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var folderPath = _settings.ExcelFolderPath;
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                Logger.LogDebug("No Excel folder path configured.");
                return;
            }

            var folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
            {
                folder.Create();
                Logger.LogDebug("Created Excel folder at \"{Path}\".", folderPath);
                return;
            }

            var file = folder.GetFiles("*.xlsx").FirstOrDefault();
            if (file == null)
            {
                Logger.LogDebug("No .xlsx file found in folder \"{Path}\".", folderPath);
                return;
            }

            Logger.LogInformation("START - Importing motorbike attributes from \"{File}\".", file.Name);

            // Load (or create) the global moto product attribute.
            var motoAttribute = await _db.ProductAttributes
                .FirstOrDefaultAsync(x => x.Alias == MotoAttributeAlias, cancelToken);

            if (motoAttribute == null)
            {
                Logger.LogDebug("Moto product attribute not found, creating it.");
                motoAttribute = new ProductAttribute
                {
                    Alias = MotoAttributeAlias,
                    Name = MotoAttributeName,
                    AllowFiltering = false,
                    DisplayOrder = 100
                };
                _db.ProductAttributes.Add(motoAttribute);
                await _db.SaveChangesAsync(cancelToken);
                Logger.LogDebug("Created moto product attribute with id {Id}.", motoAttribute.Id);
            }

            // Read the Excel file and group rows by product ID.
            Dictionary<int, List<string>> rowsByProductId;
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                rowsByProductId = ReadMotorbikeRows(reader);
            }

            Logger.LogInformation("Found {Count} product(s) to process.", rowsByProductId.Count);

            foreach (var (productId, motoNames) in rowsByProductId)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                await ProcessProductAsync(productId, motoNames, motoAttribute, cancelToken);
            }

            Logger.LogInformation("END - Finished importing from \"{File}\". Deleting file.", file.Name);
            file.Delete();
        }

        private Dictionary<int, List<string>> ReadMotorbikeRows(IExcelDataReader reader)
        {
            var result = new Dictionary<int, List<string>>();
            var rowCount = 0;
            var skippedCount = 0;

            // Process all sheets
            do
            {
                var sheetName = reader.Name;
                Logger.LogInformation("Processing sheet: {SheetName}", sheetName);

                var isFirstRow = true;

                while (reader.Read())
                {
                    // Skip header row (first row of each sheet)
                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        continue;
                    }

                    rowCount++;

                    if (reader.FieldCount < 5)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Read all 5 columns
                    var rawCol0 = reader.GetValue(0);
                    var rawCol1 = reader.GetValue(1);
                    var rawCol2 = reader.GetValue(2);
                    var rawCol3 = reader.GetValue(3);
                    var rawCol4 = reader.GetValue(4);

                    // Check if ALL values are null (indicates empty row)
                    if (rawCol0 == null && rawCol1 == null && rawCol2 == null && rawCol3 == null && rawCol4 == null)
                    {
                        skippedCount++;
                        continue;
                    }

                    int productId = rawCol4.Convert<int>();

                    if (productId <= 0)
                    {
                        skippedCount++;
                        continue;
                    }

                    var brand = rawCol0.Convert<string>()?.Trim();
                    var model = rawCol1.Convert<string>()?.Trim();
                    var year = rawCol2.Convert<string>()?.Trim();

                    var fullName = $"{brand} {model} {year}".Trim();
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        skippedCount++;
                        continue;
                    }

                    if (!result.TryGetValue(productId, out var names))
                    {
                        names = new List<string>();
                        result[productId] = names;
                    }

                    if (!names.Contains(fullName, StringComparer.OrdinalIgnoreCase))
                    {
                        names.Add(fullName);
                    }
                }
            } while (reader.NextResult());

            Logger.LogInformation("Imported {Products} unique products with {Total} variants ({Skipped} rows skipped)",
                result.Count, rowCount, skippedCount);

            return result;
        }

        private async Task ProcessProductAsync(
            int productId,
            List<string> motoNames,
            ProductAttribute motoAttribute,
            CancellationToken cancelToken)
        {
            var product = await _db.Products.FindAsync([productId], cancelToken);
            if (product == null)
            {
                Logger.LogWarning("Product with ID {Id} not found, skipping.", productId);
                return;
            }

            Logger.LogInformation("START - Importing attributes for product \"{Name}\" ({Sku}).", product.Name, product.Sku);

            // Get or create the ProductVariantAttribute mapping for this product.
            var pva = await _db.ProductVariantAttributes
                .Include(x => x.ProductAttribute)
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.ProductAttributeId == motoAttribute.Id, cancelToken);

            if (pva == null)
            {
                Logger.LogDebug("Creating ProductVariantAttribute for product {Id}.", productId);
                pva = new ProductVariantAttribute
                {
                    ProductId = productId,
                    ProductAttributeId = motoAttribute.Id,
                    AttributeControlType = AttributeControlType.DropdownList,
                    IsRequired = true,
                    DisplayOrder = 10
                };
                _db.ProductVariantAttributes.Add(pva);
                await _db.SaveChangesAsync(cancelToken);
            }

            // Delete all existing combinations for this product (moto-related).
            var existingCombinations = await _db.ProductVariantAttributeCombinations
                .Where(x => x.ProductId == productId)
                .ToListAsync(cancelToken);
            _db.ProductVariantAttributeCombinations.RemoveRange(existingCombinations);

            // Delete all existing attribute values for the moto attribute on this product.
            var existingValues = await _db.ProductVariantAttributeValues
                .Where(x => x.ProductVariantAttributeId == pva.Id)
                .ToListAsync(cancelToken);
            _db.ProductVariantAttributeValues.RemoveRange(existingValues);

            await _db.SaveChangesAsync(cancelToken);

            // Insert new attribute values and collect them for combination creation.
            var newValues = new List<ProductVariantAttributeValue>();

            foreach (var motoName in motoNames)
            {
                var alias = GetSha256Hash(motoName);
                var value = new ProductVariantAttributeValue
                {
                    ProductVariantAttributeId = pva.Id,
                    Name = motoName,
                    Alias = alias,
                    IsPreSelected = false,
                    PriceAdjustment = 0M,
                    WeightAdjustment = 0M,
                    DisplayOrder = 0,
                    ValueType = ProductVariantAttributeValueType.Simple,
                    LinkedProductId = productId,
                    Quantity = 0
                };
                _db.ProductVariantAttributeValues.Add(value);
                newValues.Add(value);

                Logger.LogDebug("Queued value: {Name}.", motoName);
            }

            await _db.SaveChangesAsync(cancelToken);

            // Create one combination per moto value.
            foreach (var value in newValues)
            {
                var selection = new ProductVariantAttributeSelection(null);
                selection.AddAttributeValue(pva.Id, value.Id);

                var combination = new ProductVariantAttributeCombination
                {
                    ProductId = productId,
                    RawAttributes = selection.AsJson(),
                    StockQuantity = 10000,
                    AllowOutOfStockOrders = false,
                    IsActive = true
                };
                _db.ProductVariantAttributeCombinations.Add(combination);
            }

            await _db.SaveChangesAsync(cancelToken);

            Logger.LogInformation("END - Imported {Count} motorbike value(s) for product \"{Name}\" ({Sku}).",
                newValues.Count, product.Name, product.Sku);
        }

        private static string GetSha256Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
