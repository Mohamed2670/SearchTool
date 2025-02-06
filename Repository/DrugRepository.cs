using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
               .Where(x => x.Name.ToLower().Contains(name.ToLower())).
               Distinct()
               .ToListAsync();
        }

        public async Task<ICollection<string>> GetAllNDCByDrugName(string name)
        {
            var items = await _context.Drugs
                  .Where(d => d.Name == name)
                .GroupBy(d => d.NDC)
                .Select(d => d.Key)
                .Distinct()
                .ToListAsync();
            return items;
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
        public async Task<decimal> GetBestNet(int insuranceId, int classId)
        {
            var item = await _context.ClassInsurances.FirstOrDefaultAsync(x => x.InsuranceId == insuranceId && x.ClassId == classId);
            return item.BestNet;
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
                    HeaderValidated = null,
                    MissingFieldFound = null,
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
                    await context.SaveChangesAsync();
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
                    ClassId = drugClass.Id,
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
                var classItem = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Name == record.DrugClass);
                if (classItem == null)
                {
                    classItem = new DrugClass { Name = record.DrugClass };
                    _context.DrugClasses.Add(classItem);
                    await _context.SaveChangesAsync();

                }

                if (drug == null)
                {

                    drug = new Drug
                    {
                        Name = record.DrugName,
                        NDC = record.NDCCode,
                        Form = null,
                        Strength = null,
                        ClassId = classItem.Id,
                        ACQ = record.AcquisitionCost,
                        AWP = 0,
                        Rxcui = record.RxCui
                    };
                    _context.Drugs.Add(drug);
                    await _context.SaveChangesAsync();
                }

                // Check if drug insurance entry exists
                var exists = await _context.DrugInsurances
                    .FirstOrDefaultAsync(di => di.InsuranceId == insurance.Id && di.DrugId == drug.Id);

                if (classItem == null)
                {
                    throw new InvalidOperationException($"Drug class with ID {drug.ClassId} not found.");
                }
                if (exists == null)
                {
                    var drugInsurance = new DrugInsurance
                    {
                        InsuranceId = insurance.Id,
                        DrugId = drug.Id,
                        NDCCode = record.NDCCode,
                        DrugName = record.DrugName,
                        Net = record.NetProfit,
                        ClassId = classItem.Id,
                        date = record.Date, // string type
                        Prescriber = record.Prescriber,
                        Quantity = record.Quantity,
                        AcquisitionCost = record.AcquisitionCost,
                        RxCui = record.RxCui ?? 0,
                        Discount = record.Discount,
                        insuranceName = record.Insurance,
                        InsurancePayment = record.InsurancePayment,
                        PatientPayment = record.PatientPayment,
                        DrugClass = record.DrugClass
                    };
                    // Console.WriteLine(drugInsurance.date + " : " + record.Date);
                    // Console.ReadKey();
                    _context.DrugInsurances.Add(drugInsurance);
                    await _context.SaveChangesAsync();

                }
                else
                {
                    DateTime existsDate, recordDate;
                    bool isExistsDateValid = DateTime.TryParseExact(exists.date, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out existsDate);
                    bool isRecordDateValid = DateTime.TryParseExact(record.Date, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out recordDate);
                    // Console.WriteLine(existsDate + " : " + recordDate);
                    // Console.ReadKey();
                    if (exists != null && isExistsDateValid && isRecordDateValid && existsDate < recordDate) // take month day year
                    {
                        exists.Net = record.NetProfit;
                        exists.AcquisitionCost = record.AcquisitionCost;
                        exists.Discount = record.Discount;
                        exists.InsurancePayment = record.InsurancePayment;
                        exists.PatientPayment = record.PatientPayment;
                        exists.date = record.Date;
                        _context.DrugInsurances.Update(exists);
                    }
                }
                var ClassInsuranceExists = await _context.ClassInsurances.FirstOrDefaultAsync(x => x.InsuranceId == insurance.Id && x.ClassId == classItem.Id);
                if (ClassInsuranceExists == null)
                {
                    var classInsurance = new ClassInsurance
                    {
                        InsuranceId = insurance.Id,
                        InsuranceName = insurance.Name,
                        ClassId = classItem.Id,
                        ClassName = classItem.Name,
                        BestNet = record.NetProfit
                    };
                    // Console.WriteLine(classItem.Id + " : " + classItem.Name);
                    // Console.WriteLine(drug.Id + " : " + drug.Name);
                    // Console.WriteLine(classInsurance.InsuranceName + " : " + classInsurance.InsuranceId + " : " + classInsurance.BestNet);
                    // Console.WriteLine(classInsurance.ClassId + " : " + classInsurance.ClassName );

                    // Console.ReadKey();
                    _context.ClassInsurances.Add(classInsurance);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    decimal HiestNet = Math.Max(record.NetProfit, ClassInsuranceExists.BestNet);

                    ClassInsuranceExists.BestNet = Math.Max(record.NetProfit, ClassInsuranceExists.BestNet);

                    _context.ClassInsurances.Update(ClassInsuranceExists);
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

        public async Task<DrugInsurance> GetBySelection(string name, string ndc, string insuranceName)
        {
            var insurance = await _context.Insurances.FirstOrDefaultAsync(x => x.Name == insuranceName);
            var item = await _context.DrugInsurances.FirstOrDefaultAsync(x => x.DrugName == name && x.NDCCode == ndc && x.InsuranceId == insurance.Id);
            return item;
        }

        public async Task<ICollection<DrugInsurance>> GetAltrantives(string className, int insuranceId)
        {
            var classItem = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Name == className);
            if (classItem == null)
            {
                throw new InvalidOperationException($"Drug class with name {className} not found.");
            }
            var items = await _context.DrugInsurances
                .Where(x => x.ClassId == classItem.Id && x.InsuranceId == insuranceId).
                GroupBy(x => x.DrugName).Select(g => g.First())
                .ToListAsync();

            return items;
        }

        internal async Task<Drug> SearchByIdNdc(int id, string ndc)
        {
            var item = await _context.Drugs.FirstOrDefaultAsync(x => x.Id == id && x.NDC == ndc);
            return item;
        }

        internal async Task<Drug> GetDrugByNdc(string ndc)
        {
            var item = await _context.Drugs.FirstOrDefaultAsync(x => x.NDC == ndc);
            return item;
        }

        internal async Task<DrugInsurance> GetDetails(string ndc, int insuranceId)
        {
            var item = await _context.DrugInsurances.FirstOrDefaultAsync(x => x.NDCCode == ndc && x.InsuranceId == insuranceId);
            return item;
        }

        internal async Task<ICollection<string>> getDrugNDCsByNameInsuance(string drugName, int insurnaceId)
        {
            var items = await _context.DrugInsurances.Where(x => x.InsuranceId == insurnaceId && x.DrugName == drugName).
            GroupBy(x => x.NDCCode).Select(d => d.Key).ToListAsync();
            return items;
        }

        internal async Task<DrugClass> getClassbyId(int id)
        {
            var item = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Id == id);
            return item;
        }

        internal async Task<ICollection<Drug>> GetDrugsByClass(int classId)
        {
            var items = await _context.Drugs.Where(x => x.ClassId == classId).GroupBy(x => x.Name).Select(g => g.First()).ToListAsync();
            return items;
        }

        internal async Task<ICollection<DrugInsurance>> GetAllLatest()
        {
            var items = await _context.DrugInsurances
                .AsNoTracking()
                .ToListAsync();
            return items;
        }
        internal async Task<ICollection<DrugInsurance>> GetAllDrugs(int classId)
        {
            var drugInsurances = await _context.DrugInsurances
                .Where(di => di.ClassId == classId)
                .ToListAsync();

            var drugs = await _context.Drugs
                .Where(d => d.ClassId == classId)
                .GroupBy(d => d.NDC)
                .Select(g => g.First())
                .ToListAsync();

            var result = drugs.Select(d =>
            {
                var drugInsurance = drugInsurances.FirstOrDefault(di => di.DrugId == d.Id);
                return drugInsurance ?? new DrugInsurance
                {
                    InsuranceId = 0, // or any default value
                    DrugId = d.Id,
                    NDCCode = d.NDC,
                    DrugName = d.Name,
                    ClassId = d.ClassId,
                    Net = 0, // or any default value
                    date = null, // or any default value
                    Prescriber = null, // or any default value
                    Quantity = 0, // or any default value
                    AcquisitionCost = 0, // or any default value
                    RxCui = 0, // or any default value
                    Discount = 0, // or any default value
                    InsurancePayment = 0, // or any default value
                    PatientPayment = 0, // or any default value
                    insuranceName = null, // or any default value
                };
            }).ToList();

            return result;
        }

        internal async Task<Drug> GetDrugById(int id)
        {
            var item = await _context.Drugs.FirstOrDefaultAsync(x => x.Id == id);
            return item;
        }

        internal async Task oneway()
        {
            var drugInsurances = await _context.DrugInsurances.ToListAsync();
            var insurances = await _context.Insurances.ToDictionaryAsync(i => i.Id, i => i.Name);

            foreach (var drugInsurance in drugInsurances)
            {
                if (insurances.TryGetValue(drugInsurance.InsuranceId, out var insuranceName))
                {
                    drugInsurance.insuranceName = insuranceName;
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
            public decimal? RxCui { get; set; }

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
