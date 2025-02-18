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
using SearchTool_ServerSide.Dtos;
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
        public async Task<ClassInsurance> GetBestNet(int insuranceId, int classId)
        {
            var item = await _context.ClassInsurances.FirstOrDefaultAsync(x => x.InsuranceId == insuranceId && x.ClassId == classId);
            return item;
        }
        public async Task<Insurance> GetInsurance(string name)
        {
            var item = await _context.Insurances.FirstOrDefaultAsync(x => x.Name == name);
            return item;

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

                // Console.WriteLine("rxcui " + record.Rxcui);
                // Console.ReadKey();
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
                    DrugClassId = drugClass.Id,
                    ACQ = record.ACQ,
                    AWP = record.AWP,
                    Rxcui = record.Rxcui

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
                    await _context.Insurances.AddAsync(insurance);
                    await _context.SaveChangesAsync();
                }

                // Normalize NDC Code
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);

                // Check if drug exists
                var drug = await _context.Drugs.FirstOrDefaultAsync(i => i.NDC == record.NDCCode);
                var tempDrug = await _context.Drugs.FirstOrDefaultAsync(x => x.Name == record.DrugName);
                if (drug == null)
                {
                    if (tempDrug != null)
                    {
                        drug = new Drug
                        {
                            Name = record.DrugName,
                            NDC = record.NDCCode,
                            Form = tempDrug.Form,
                            Strength = tempDrug.Strength,
                            DrugClassId = tempDrug.DrugClassId,
                            ACQ = record.AcquisitionCost,
                            AWP = 0,
                            Rxcui = tempDrug.Rxcui
                        };
                        await _context.Drugs.AddAsync(drug);
                        tempDrug.ACQ = record.AcquisitionCost;
                        _context.Drugs.Update(tempDrug);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Skipping record: Drug with NDC {record.NDCCode} not found.");
                        // Console.ReadLine();
                        continue; // Skip this record

                    }
                }

                var classItem = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Id == drug.DrugClassId);

                // Check if drug insurance entry exists
                var exists = await _context.DrugInsurances
                    .FirstOrDefaultAsync(di => di.InsuranceId == insurance.Id && di.DrugId == drug.Id);

                if (exists == null)
                {
                    var drugInsurance = new DrugInsurance
                    {
                        InsuranceId = insurance.Id,
                        DrugId = drug.Id,
                        NDCCode = record.NDCCode,
                        DrugName = record.DrugName,
                        Net = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost,
                        DrugClassId = classItem.Id,
                        date = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime(),
                        Prescriber = record.Prescriber,
                        Quantity = record.Quantity,
                        AcquisitionCost = record.AcquisitionCost,
                        Discount = record.Discount,
                        insuranceName = record.Insurance,
                        InsurancePayment = record.InsurancePayment,
                        PatientPayment = record.PatientPayment,
                        DrugClass = classItem.Name
                    };
                    await _context.DrugInsurances.AddAsync(drugInsurance);
                }
                else
                {
                    DateTime existsDate = exists.date, recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime();
                    if (existsDate < recordDate)
                    {
                        exists.Net = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost;
                        exists.AcquisitionCost = record.AcquisitionCost;
                        exists.Discount = record.Discount;
                        exists.InsurancePayment = record.InsurancePayment;
                        exists.PatientPayment = record.PatientPayment;
                        exists.date = recordDate;
                        _context.DrugInsurances.Update(exists);

                    }

                }
                await _context.SaveChangesAsync();

                var recordDat = DateTime.SpecifyKind(DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture), DateTimeKind.Utc);
                var yearMonth = new DateTime(recordDat.Year, recordDat.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                var ClassInsuranceExists = await _context.ClassInsurances.FirstOrDefaultAsync(x =>
                    x.InsuranceId == insurance.Id && x.ClassId == classItem.Id && x.Date.Year == recordDat.Year && x.Date.Month == recordDat.Month);
                // if (yearMonth.Month == 11)
                // {
                //     Console.WriteLine("how!!!" + record.Date + " : " + DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime());
                //     Console.ReadKey();
                // }
                if (ClassInsuranceExists == null)
                {
                    var classInsurance = new ClassInsurance
                    {
                        InsuranceId = insurance.Id,
                        InsuranceName = insurance.Name,
                        ClassId = classItem.Id,
                        DrugId = drug.Id,
                        Date = yearMonth,
                        ClassName = classItem.Name,
                        ScriptDateTime = yearMonth,
                        ScriptCode = record.Script,
                        BestNet = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost
                    };
                    _context.ClassInsurances.Add(classInsurance);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var bestNet = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost;
                    if (bestNet > ClassInsuranceExists.BestNet)
                    {
                        ClassInsuranceExists.BestNet = bestNet;
                        ClassInsuranceExists.DrugId = drug.Id;
                        ClassInsuranceExists.ScriptCode = record.Script;
                        ClassInsuranceExists.ScriptDateTime = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime();
                        _context.ClassInsurances.Update(ClassInsuranceExists);
                        await _context.SaveChangesAsync();
                    }
                }
                // Find or create the script entry
                var script = await _context.Scripts.FirstOrDefaultAsync(s => s.ScriptCode == record.Script);

                if (script == null)
                {
                    script = new Script
                    {
                        Date = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime(),
                        ScriptCode = record.Script,
                        RxNumber = record.RxNumber,
                        User = record.User
                    };

                    await _context.Scripts.AddAsync(script);
                    await _context.SaveChangesAsync(); // Ensure script ID is set
                }

                // Check if the script item already exists
                var scriptItem = await _context.ScriptItems
                    .FirstOrDefaultAsync(si => si.ScriptId == script.Id && si.NDCCode == record.NDCCode);

                if (scriptItem == null)
                {
                    scriptItem = new ScriptItem
                    {
                        ScriptId = script.Id,
                        DrugName = record.DrugName,
                        Insurance = record.Insurance,
                        PF = record.PF,
                        Prescriber = record.Prescriber,
                        Quantity = record.Quantity,
                        AcquisitionCost = record.AcquisitionCost,
                        Discount = record.Discount,
                        InsurancePayment = record.InsurancePayment,
                        PatientPayment = record.PatientPayment,
                        NetProfit = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost,
                        NDCCode = record.NDCCode,
                        DrugClass = classItem.Name
                    };

                    await _context.ScriptItems.AddAsync(scriptItem);
                }
                else
                {
                    // Update existing script item if it has a newer date
                    DateTime recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture).ToUniversalTime();
                    if (script.Date < recordDate)
                    {
                        scriptItem.NetProfit = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost;
                        scriptItem.AcquisitionCost = record.AcquisitionCost;
                        scriptItem.Discount = record.Discount;
                        scriptItem.InsurancePayment = record.InsurancePayment;
                        scriptItem.PatientPayment = record.PatientPayment;
                        _context.ScriptItems.Update(scriptItem);
                    }
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
                .Where(x => x.DrugClassId == classItem.Id && x.InsuranceId == insuranceId).
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
        internal async Task<DrugClass> getClassbyName(string name)
        {
            var item = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Name == name);
            return item;
        }

        internal async Task<ICollection<Drug>> GetDrugsByClass(int classId)
        {
            var items = await _context.Drugs.Where(x => x.DrugClassId == classId).GroupBy(x => x.Name).Select(g => g.First()).ToListAsync();
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
                .Where(di => di.DrugClassId == classId)
                .ToListAsync();

            var drugs = await _context.Drugs
                .Where(d => d.DrugClassId == classId)
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
                    DrugClassId = d.DrugClassId,
                    Net = 0, // or any default value
                    date = DateTime.UtcNow, // or any default value
                    Prescriber = null, // or any default value
                    Quantity = 0, // or any default value
                    AcquisitionCost = 0, // or any default value
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

            [Name("User")]
            public string? User { get; set; }

            [Name("Drug Name")]
            public string DrugName { get; set; }

            [Name("Ins")]
            public string Insurance { get; set; }

            [Name("PF")]
            public string PF { get; set; }

            [Name("Prescriber")]
            public string Prescriber { get; set; }

            [Name("Qty")]
            public decimal Quantity { get; set; }

            [Name("ACQ")]
            public decimal AcquisitionCost { get; set; }

            [Name("Discount")]
            public decimal Discount { get; set; }

            [Name("Ins Pay")]
            public decimal InsurancePayment { get; set; }

            [Name("Pat Pay")]
            public decimal PatientPayment { get; set; }

            [Name("NDC")]
            public string NDCCode { get; set; }
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

            [Name("rxCUI")]
            [Default(0)] // تعيين القيمة الافتراضية إلى 0 عند وجود بيانات فارغة
            public decimal Rxcui { get; set; }

            [Name("drug_class")]
            public string? DrugClass { get; set; }
        }
        internal async Task<ICollection<AuditReadDto>> GetAllLatestScripts()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.Insurances on scriptItem.Insurance equals insurance.Name
                join classItem in _context.DrugClasses on scriptItem.DrugClass equals classItem.Name
                join classInsurance in _context.ClassInsurances
                    on new { InsuranceId = insurance.Id, ClassId = classItem.Id, Year = script.Date.Year, Month = script.Date.Month }
                    equals new { classInsurance.InsuranceId, classInsurance.ClassId, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month }
                join drug in _context.Drugs on classInsurance.DrugId equals drug.Id

                let prevMonth = script.Date.AddMonths(-1)
                let bestNetEntryPrevMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == insurance.Id && ci.ClassId == classItem.Id &&
                                 ci.Date.Year == prevMonth.Year && ci.Date.Month == prevMonth.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()

                let bestNetEntryCurrentMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == insurance.Id && ci.ClassId == classItem.Id &&
                                 ci.Date.Year == script.Date.Year && ci.Date.Month == script.Date.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()

                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth  // Use previous month if available, otherwise fallback to current month

                select new AuditReadDto
                {
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = script.RxNumber,
                    User = script.User,
                    DrugName = scriptItem.DrugName, // From ScriptItem now
                    Insurance = scriptItem.Insurance,
                    PF = scriptItem.PF,
                    Prescriber = scriptItem.Prescriber,
                    Quantity = scriptItem.Quantity,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    NDCCode = scriptItem.NDCCode,
                    NetProfit = scriptItem.NetProfit,
                    DrugClass = scriptItem.DrugClass, // From ScriptItem now
                    HighstDrugNDC = drug.NDC,
                    HighstDrugName = drug.Name,
                    HighstDrugId = drug.Id,
                    HighstNet = bestNetEntry.BestNet,
                    HighstScriptCode = bestNetEntry.ScriptCode,
                    HighstScriptDate = bestNetEntry.ScriptDateTime
                }).ToListAsync();

            return auditData;
        }


        internal async Task<ICollection<DrugInsurance>> GetInsuranceByNdc(string ndc)
        {
            var items = await _context.DrugInsurances.Where(x => x.NDCCode == ndc).ToListAsync();
            return items;
        }
    }
}