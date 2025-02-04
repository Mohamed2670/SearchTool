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

        public async Task<ICollection<string>> GetAllNDCByDrugName(string name)
        {
            return await _context.Drugs
                  .Where(d => d.Name == name)
                .GroupBy(d => d.NDC)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();
        }

        public async Task<ICollection<Insurance>> GetDrugInsurances(string name)
        {
            // get insurance name by drug name
            var items = await _context.DrugInsurances
                .Where(d => d.DrugName == name)
                .GroupBy(d => d.InsuranceId)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();

            // get insurance name by id     
            var ret = await _context.Insurances.Where(x => items.Contains(x.Id)).ToListAsync();
            return ret;       
        }

        public async Task<ICollection<string>> GetAllInsuranceByNDC(string ndc)
        {
            var items = await _context.DrugInsurances
                .Where(d => d.NDCCode == ndc)
                .GroupBy(d => d.InsuranceId)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();
            var ret = await _context.Insurances.Where(x => items.Contains(x.Id)).Select(i => i.Name).ToListAsync();
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
                string temp = record.NDC.Replace("-", "");
                temp = temp.TrimStart('0');


                if (!drugClasses.TryGetValue(record.DrugClass, out var drugClass))

                {
                    drugClass = new DrugClass { Name = record.DrugClass };
                    context.DrugClasses.Add(drugClass);
                    drugClasses[record.DrugClass] = drugClass;
                }
                var drug = new Drug
                {
                    Name = record.Name,
                    NDC = temp,
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

        public async Task ImportDrugInsuranceAsync(string filePath = "scripts.csv")
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null, // Ignore missing headers
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ScriptRecord>().ToList();
            int cnt = 0;
            foreach (var record in records)
            {
                // Check if insurance exists
                var insurance = await _context.Insurances
                    .FirstOrDefaultAsync(i => i.Name == record.Insurance);

                if (insurance == null)
                {
                    insurance = new Insurance { Name = record.Insurance };
                    _context.Insurances.Add(insurance);
                    await _context.SaveChangesAsync();
                }

                // Check if drug exists
                var drug = await _context.Drugs.FirstOrDefaultAsync(i => i.NDC == record.NDCCode);

                if (drug == null)
                {

                    drug = new Drug
                    {
                        Name = record.DrugName,
                        NDC = record.NDCCode,
                        Form = null, // Default values, adjust if needed
                        Strength = null,
                        ClassId = 0,
                        ACQ = 0,
                        AWP = 0,
                        Rxcui = 0
                    };
                    _context.Drugs.Add(drug);
                    await _context.SaveChangesAsync();
                }

                // Check if drug insurance entry exists
                var exists = await _context.DrugInsurances
                    .AnyAsync(di => di.InsuranceId == insurance.Id && di.DrugId == drug.Id);

                if (!exists)
                {
                    var drugInsurance = new DrugInsurance
                    {
                        InsuranceId = insurance.Id,
                        DrugId = drug.Id,
                        NDCCode = record.NDCCode,
                        DrugName = record.DrugName
                    };
                    _context.DrugInsurances.Add(drugInsurance);
                    await _context.SaveChangesAsync();

                }

                var script = new Script
                {
                    Date = record.Date,
                    ScriptCode = record.Script,
                    RxNumber = record.RxNumber,
                    DrugName = record.DrugName,
                    Insurance = record.Insurance,
                    Prescriber = record.Prescriber,
                    Quantity = record.Quantity,
                    AcquisitionCost = record.AcquisitionCost,
                    NDCCode = record.NDCCode,
                    RxCui = record.RxCui ?? 0,
                    Discount = record.Discount,
                    InsurancePayment = record.InsurancePayment,
                    PatientPayment = record.PatientPayment,
                    DrugClass = record.DrugClass,
                    NetProfit = record.NetProfit
                };
                _context.Scripts.Add(script);
                cnt++;
                if (cnt == 1000)
                {
                    cnt = 0;
                    await _context.SaveChangesAsync();

                }


            }
            await _context.SaveChangesAsync();

        }


        public class DrugInsuranceRecord
        {
            [Name("Ins")]
            public string InsuranceName { get; set; }

            [Name("Drug Name")]
            public string DrugName { get; set; }

            [Name("NDC")]
            public string NDCCode { get; set; }
        }
        public class ScriptRecord
        {
            [Name("Date")]
            public string Date { get; set; }

            [Name("Script")]
            public string Script { get; set; }

            [Name("R#")]
            public string RxNumber { get; set; }

            [Name("Drug Name")]
            public string DrugName { get; set; }

            [Name("Ins")]
            public string Insurance { get; set; }

            [Name("Prescriber")]
            public string Prescriber { get; set; }

            [Name("Qty")]
            public decimal Quantity { get; set; }

            [Name("ACQ")]
            public decimal AcquisitionCost { get; set; }

            [Name("NDC")]
            public string NDCCode { get; set; }

            [Name("RxCui")]
            public int? RxCui { get; set; }

            [Name("Discount")]
            public decimal Discount { get; set; }

            [Name("Ins Pay")]
            public decimal InsurancePayment { get; set; }

            [Name("Pat Pay")]
            public decimal PatientPayment { get; set; }

            [Name("class")]
            public string DrugClass { get; set; }

            [Name("Net_profit")]
            public decimal NetProfit { get; set; }
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
