using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class NadacRepository(SearchToolDBContext _context)
    {
        public async Task<Stream> DownloadNadacCsvAsync(string csvUrl = "https://download.medicaid.gov/data/nadac-national-average-drug-acquisition-cost-06-11-2025.csv")
        {
            var client = new HttpClient();
            Console.WriteLine("Downloading NADAC CSV file...: " + csvUrl);
            var response = await client.GetAsync(csvUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        public async Task<List<NadacRecord>> GetMatchingPricesAsync(string csvUrl)
        {
            var yourNdcList = _context.DrugMedis
                .Select(d => d.DrugNDC.Trim())
                .Distinct()
                .ToList();

            if (yourNdcList == null || !yourNdcList.Any())
            {
                return new List<NadacRecord>();
            }

            var csvStream = await DownloadNadacCsvAsync(csvUrl);

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                MissingFieldFound = null
            });
            Console.WriteLine("Reading NADAC CSV file...");
            var allRecords = csv.GetRecords<NadacRecord>()
                .Where(r => yourNdcList.Contains(NormalizeNdcTo11Digits(r.NDC)))
                .ToList();
            Console.WriteLine($"Found {allRecords.Count} matching NADAC records.");
            var latestPerNdc =  allRecords
                .GroupBy(r => r.NDC)
                .Select(g => g
                    .OrderByDescending(r => r.AsOfDate)  // Use EffectiveDate if AsOfDate is missing
                    .ThenByDescending(r => r.EffectiveDate)
                    .First()
                )
                .ToList();
            Console.WriteLine($"Filtered to {latestPerNdc.Count} latest NADAC records per NDC.");
            return latestPerNdc;
        }

        public async Task AddNadacRecordsAsync(List<NadacRecord> nadacRecords)
        {
            if (nadacRecords == null || !nadacRecords.Any())
            {
                return;
            }
            var existingDrugInsurances = await _context.DrugInsurances.ToListAsync();
            var diDict = existingDrugInsurances
                .ToDictionary(di => (di.InsuranceId, di.DrugId, di.BranchId));
            var drugDict = _context.Drugs
                .ToDictionary(d => d.NDC);
            var insurance = await _context.InsuranceRxes.FirstOrDefaultAsync(x => x.RxGroup == "Medi-Cal");
            var branchId = 6;
            foreach (var record in nadacRecords)
            {
                record.NDC = NormalizeNdcTo11Digits(record.NDC);
                if (!drugDict.TryGetValue(record.NDC, out var drug))
                {
                    continue; // Skip if drug not found
                }
                var drugInsuranceKey = (insurance.Id, drug.Id, branchId);
                if (diDict.TryGetValue(drugInsuranceKey, out var existingDrugInsurance))
                {
                    // Update existing record
                    existingDrugInsurance.Date = DateTime.UtcNow;
                    existingDrugInsurance.InsurancePayment = record.NadacPerUnit != null
                               ? record.NadacPerUnit * 0.9m
                               : 0m;

                }
                else
                {
                    continue;
                }
            }

            await _context.SaveChangesAsync();
        }

        public static string NormalizeNdcTo11Digits(string ndcCode)
        {
            // Remove hyphens
            ndcCode = ndcCode.Replace("-", "");

            if (ndcCode.Length < 11)
            {
                ndcCode = ndcCode.PadLeft(11, '0');
            }

            // Return original if it matches the 11-digit format already
            return ndcCode;
        }
    }
}