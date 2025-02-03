using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Dtos.DrugDtos;
using SearchTool_ServerSide.Models;
using ServerSide.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SearchTool_ServerSide.Repository
{
    public class DrugRepository : GenericRepository<Drug>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public DrugRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ICollection<Drug>> GetDrugsByName(string name)
        {
            return await _context.Drugs
               .Where(x => x.Name.ToLower().Contains(name.ToLower()))
               .ToListAsync();
        }
        // public async Task<ICollection<DrugInsurance>> GetAllNDCByDrugName(string name)
        // {
        //     return await _context.DrugInsurances
        //         .Where(di => di.Drug != null && di.Drug.Name.ToLower() == name.ToLower())
        //         .Select(di => di.NDCCode)
        //         .Distinct()
        //         .ToListAsync();
        // }
        public async Task<ICollection<string>> GetAllNDCByDrugName(string name)
        {
            return await _context.Drugs
                  .Where(d => d.Name == name)
                .GroupBy(d => d.NDC)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();
        }
        public async Task<ICollection<string>> GetAllInsuranceByNDC(string ndc)
        {
            var items = await _context.DrugInsurances
                  .Where(d => d.NDCCode == ndc)
                .GroupBy(d => d.InsuranceId)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();
            var ret = await _context.Insurances.Where(x => items.Contains(x.Id)).Select(i => i.name).ToListAsync();
            return ret;
        }
        public async Task SaveData()
        {
            var filePath = "drugs.csv";

            var drugs = LoadDrugsFromCsv(filePath);

            static List<DrugCs> LoadDrugsFromCsv(string filePath)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null, // تجاهل الأخطاء الخاصة بالرؤوس
                    MissingFieldFound = null, // تجاهل الحقول المفقودة
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                return new List<DrugCs>(csv.GetRecords<DrugCs>());
            }


            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            var connectionString = configuration.GetConnectionString("SearchTool");

            var options = new DbContextOptionsBuilder<SearchToolDBContext>()
                .UseNpgsql(connectionString) // Change if using SQL Server
                .Options;

            using var context = new SearchToolDBContext(options);
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null, // Ignore missing header validation
                MissingFieldFound = null // Ignore missing fields
            });

            var records = csv.GetRecords<DrugCs>().ToList();
            var drugClasses = context.DrugClasses.ToDictionary(dc => dc.Name, dc => dc);

            foreach (var record in records)
            {
                if (!drugClasses.TryGetValue(record.DrugClass, out var drugClass))
                {
                    drugClass = new DrugClass { Name = record.DrugClass };
                    context.DrugClasses.Add(drugClass);
                    drugClasses[record.DrugClass] = drugClass;
                }
                var drug = new Drug
                {
                    Name = record.Name,
                    NDC = record.NDC,
                    Form = record.Form,
                    Strength = record.Strength,
                    ACQ = record.ACQ,
                    AWP = record.AWP,
                    Rxcui = record.Rxcui,
                    DrugClass = drugClass
                };
                var item = await context.Drugs.FirstOrDefaultAsync(x => x.NDC == drug.NDC);
                if (item != null)
                    continue;
                context.Drugs.Add(drug);
            }
            await context.SaveChangesAsync();

        }
        public async Task temp2()
        {
            string filePath = "scripts.csv";
            var scripts = LoadScriptsFromCsv(filePath);

            Console.WriteLine("Scripts loaded from CSV.");

            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .Build();

            var connectionString = configuration.GetConnectionString("SearchTool");

            var options = new DbContextOptionsBuilder<SearchToolDBContext>()
                .UseNpgsql(connectionString)
                .Options;

            using var context = new SearchToolDBContext(options);

            // Group scripts by ScriptCode and select the one with the latest Date
            var latestScripts = scripts
                .GroupBy(s => s.ScriptCode)
                .Select(g => g.OrderByDescending(s => s.Date).First())
                .ToList();

            foreach (var script in latestScripts)
            {
                var existingScript = await context.Scripts
                    .FirstOrDefaultAsync(s => s.ScriptCode == script.ScriptCode);
                DateTime parsedDate;
                string dateString = script.Date.Split(' ')[0];
                if (!DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    Console.WriteLine($"Warning: Invalid date format '{script.Date}', skipping entry.");
                    continue;
                }

                if (existingScript == null)
                {
                    var item = _mapper.Map<Script>(script);
                    item.Date = parsedDate;
                    await context.Scripts.AddAsync(item);
                    Console.WriteLine($"Added script: {script.ScriptCode}");
                }
                else if (existingScript.Date < parsedDate)
                {
                    // Update existing script with newer data
                    existingScript.Date =parsedDate;
                    existingScript.TotalPrice = script.TotalPrice;
                    existingScript.UserId = script.UserId;
                    existingScript.InsurancePay = script.InsurancePay;
                    existingScript.Net = script.Net;
                    existingScript.Discount = script.Discount;
                    existingScript.PatientPay = script.PatientPay;
                    existingScript.Quantity = script.Quantity;
                    existingScript.NDCCode = script.NDCCode;
                    existingScript.DrugInsuranceId = script.DrugInsuranceId;

                    context.Scripts.Update(existingScript);
                    Console.WriteLine($"Updated script: {script.ScriptCode}");
                }
                else
                {
                    Console.WriteLine($"Script {script.ScriptCode} is up-to-date, skipping.");
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Data has been successfully stored in PostgreSQL.");
        }

        public List<ScriptAddDto> LoadScriptsFromCsv(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ScriptAddDto>();
            return new List<ScriptAddDto>(records);
        }



        public class DrugCs
        {
            [Name("drug_name")]
            public string Name { get; set; }

            [Name("ndc")]
            public string NDC { get; set; }

            [Name("form")]
            public string? Form { get; set; }

            [Name("strength")]
            public string? Strength { get; set; }

            [Name("acq")]
            public decimal ACQ { get; set; }

            [Name("awp")]
            public decimal AWP { get; set; }

            [Name("rxcui")]
            [Default(0)] // تعيين القيمة الافتراضية إلى 0 عند وجود بيانات فارغة
            public int Rxcui { get; set; }

            [Name("drug_class")]
            public string? DrugClass { get; set; }
        }
    }
}
