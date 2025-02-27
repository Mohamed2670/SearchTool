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

        // public async Task<ICollection<Insurance>> GetDrugInsurances(string name)
        // {
        //     // get insurance name by drug name
        //     var items = await _context.DrugInsurances
        //         .Where(d => d.DrugName == name)
        //         .GroupBy(d => d.InsuranceId)
        //         .Select(d => d.Key)
        //         .Distinct()
        //         .ToListAsync();

        //     // get insurance name by id     
        //     var ret = await _context.Insurances.Where(x => items.Contains(x.Id)).ToListAsync();
        //     return ret;
        // }
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

            // **Step 1: Load Data from CSV**
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<DrugCs>().ToList();

            // **Step 2: Load Existing Drug Classes**
            var drugClasses = await context.DrugClasses.ToDictionaryAsync(dc => dc.Name, dc => dc);

            // **Step 3: Load Existing Drugs by NDC & Name**
            var existingDrugsByNdc = await context.Drugs
                .GroupBy(d => d.NDC)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var existingDrugsByName = await context.Drugs
                .GroupBy(d => d.Name)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var newDrugs = new List<Drug>();

            foreach (var record in records)
            {
                string tempNdc = record.NDC.Replace("-", ""); // Normalize NDC

                // **Check if Drug Exists by NDC**
                if (existingDrugsByNdc.ContainsKey(tempNdc))
                {
                    continue; // Skip existing drug
                }

                // **Check if Drug Exists by Name**
                if (existingDrugsByName.TryGetValue(record.Name, out var existingDrug))
                {
                    var newDrug = new Drug
                    {
                        Name = record.Name,
                        NDC = tempNdc,
                        Form = existingDrug.Form,
                        Strength = existingDrug.Strength,
                        DrugClassId = existingDrug.DrugClassId,
                        ACQ = record.ACQ,
                        AWP = record.AWP,
                        Rxcui = existingDrug.Rxcui
                    };

                    newDrugs.Add(newDrug);
                    existingDrugsByNdc[tempNdc] = newDrug; // Add to dictionary
                }
                else
                {
                    // **Create Drug Class if Missing**
                    if (!drugClasses.TryGetValue(record.DrugClass, out var drugClass))
                    {
                        drugClass = new DrugClass { Name = record.DrugClass };
                        await context.DrugClasses.AddAsync(drugClass);
                        await context.SaveChangesAsync();
                        drugClasses[record.DrugClass] = drugClass;
                    }

                    var newDrug = new Drug
                    {
                        Name = record.Name,
                        NDC = tempNdc,
                        Form = record.Form,
                        Strength = record.Strength,
                        DrugClassId = drugClass.Id,
                        ACQ = record.ACQ,
                        AWP = record.AWP,
                        Rxcui = record.Rxcui
                    };

                    newDrugs.Add(newDrug);
                    existingDrugsByNdc[tempNdc] = newDrug; // Add to dictionary
                }
            }

            if (newDrugs.Any())
            {
                await context.Drugs.AddRangeAsync(newDrugs);
                await context.SaveChangesAsync();
            }
        }


        public async Task ImportDrugInsuranceAsync(string filePath = "scripts.csv")
        {
            // ========================================================
            // PHASE 0: Read CSV Records
            // ========================================================
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ScriptRecord>().ToList();

            // Keep only records that yield a valid Drug.
            var processedRecords = new List<ScriptRecord>();

            // ========================================================
            // PHASE 1: Process Principal Entities – Insurances & Drugs
            // ========================================================
            // Preload existing Insurances, Drugs, and DrugClasses.
            var insuranceDict = await _context.Insurances.ToDictionaryAsync(i => i.Name);

            var drugsFromDb = await _context.Drugs.ToListAsync();
            // Key by normalized NDC.
            var drugDict = drugsFromDb.ToDictionary(d => d.NDC);
            // Key by drug name (using grouping to avoid duplicate key issues)
            var drugByNameDict = drugsFromDb
                                    .GroupBy(d => d.Name)
                                    .ToDictionary(g => g.Key, g => g.First());
            var drugClassDict = await _context.DrugClasses.ToDictionaryAsync(dc => dc.Id);
            var newInsurances = new List<Insurance>();
            var newDrugs = new List<Drug>();
            int batchSize = 1000, countPhase1 = 0;

            foreach (var record in records)
            {
                // Normalize NDC.
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);

                // ---- Process Insurance
                if (!insuranceDict.ContainsKey(record.Insurance))
                {
                    var ins = new Insurance { Name = record.Insurance };
                    newInsurances.Add(ins);
                    insuranceDict[record.Insurance] = ins; // will have generated Id after saving
                }

                // ---- Process Drug
                Drug drug = null;
                if (!drugDict.TryGetValue(record.NDCCode, out drug))
                {
                    if (drugByNameDict.TryGetValue(record.DrugName, out var tempDrug))
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
                        newDrugs.Add(drug);
                        drugDict[record.NDCCode] = drug;
                    }
                    else
                    {
                        Console.WriteLine($"Skipping record: Drug with NDC {record.NDCCode} not found.");
                        continue;
                    }
                }
                processedRecords.Add(record);
                countPhase1++;
                if (countPhase1 % batchSize == 0)
                {
                    if (newInsurances.Any())
                    {
                        _context.Insurances.AddRange(newInsurances);
                        await _context.SaveChangesAsync();
                        newInsurances.Clear();
                    }
                    if (newDrugs.Any())
                    {
                        _context.Drugs.AddRange(newDrugs);
                        await _context.SaveChangesAsync();
                        newDrugs.Clear();
                    }
                }
            }
            if (newInsurances.Any())
            {
                _context.Insurances.AddRange(newInsurances);
                await _context.SaveChangesAsync();
            }
            if (newDrugs.Any())
            {
                _context.Drugs.AddRange(newDrugs);
                await _context.SaveChangesAsync();
            }

            // ========================================================
            // PHASE 2: Process Intermediate Dependents – DrugInsurance & ClassInsurance
            // ========================================================
            // ========================================================
            // PHASE 2: Process Intermediate Dependents – DrugInsurance & ClassInsurance
            // ========================================================

            // --- Load existing DrugInsurance records and build a dictionary keyed by (InsuranceId, DrugId)
            var existingDrugInsurances = await _context.DrugInsurances.ToListAsync();
            var diDict = existingDrugInsurances
                .ToDictionary(di => (di.InsuranceId, di.DrugId, di.BranchId));
            var branchDict = await _context.Branches.ToDictionaryAsync(b => b.Code);

            var existingClassInsurances = await _context.ClassInsurances.ToListAsync();
            var ciDict = existingClassInsurances
                .ToDictionary(ci => (ci.InsuranceId, ci.ClassId, ci.Date.Year, ci.Date.Month, ci.BranchId));
            var drugBranchDict = await _context.DrugBranches.ToDictionaryAsync(g => (g.BranchId, g.DrugId));

            // Lists for new inserts.
            var newDrugInsurances = new List<DrugInsurance>();
            var newClassInsurances = new List<ClassInsurance>();

            foreach (var record in processedRecords)
            {
                // Normalize NDC and parse the date.
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);
                DateTime recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture)
                                                    .ToUniversalTime();
                decimal netValue = record.PatientPayment + record.InsurancePayment - record.AcquisitionCost;
                // Use the first day of the month for ClassInsurance.
                DateTime yearMonth = new DateTime(recordDate.Year, recordDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                // Look up principal entities (guaranteed from Phase 1).
                if (!insuranceDict.TryGetValue(record.Insurance, out var insurance))
                    continue;
                if (!drugDict.TryGetValue(record.NDCCode, out var drug))
                    continue;
                if (!drugClassDict.TryGetValue(drug.DrugClassId, out var classItem))
                    continue;

                // -----------------------
                // Merge DrugInsurance
                // ------------------
                if (!branchDict.TryGetValue(record.Branch, out var branch))
                    continue;

                var diKey = (insurance.Id, drug.Id, branch.Id);
                if (diDict.TryGetValue(diKey, out var existingDI))
                {
                    // If the existing record is older, update it.
                    if (existingDI.date < recordDate)
                    {
                        existingDI.Net = netValue;
                        existingDI.AcquisitionCost = record.AcquisitionCost;
                        existingDI.Discount = record.Discount;
                        existingDI.InsurancePayment = record.InsurancePayment;
                        existingDI.PatientPayment = record.PatientPayment;
                        existingDI.date = recordDate;
                        // No need to add to context as it's already tracked.
                    }
                }
                else
                {
                    var newDI = new DrugInsurance
                    {
                        InsuranceId = insurance.Id,
                        DrugId = drug.Id,
                        BranchId = branch.Id,
                        NDCCode = record.NDCCode,
                        Net = netValue,
                        DrugClassId = classItem.Id,
                        date = recordDate,
                        Prescriber = record.Prescriber,
                        Quantity = record.Quantity,
                        AcquisitionCost = record.AcquisitionCost,
                        Discount = record.Discount,
                        InsurancePayment = record.InsurancePayment,
                        PatientPayment = record.PatientPayment,
                    };
                    newDrugInsurances.Add(newDI);
                    diDict.Add(diKey, newDI);
                }

                // -----------------------
                // Merge ClassInsurance
                // -----------------------

                var ciKey = (insurance.Id, classItem.Id, recordDate.Year, recordDate.Month, branch.Id);
                if (ciDict.TryGetValue(ciKey, out var existingCI))
                {
                    // Update if this record has a higher net value.
                    if (netValue > existingCI.BestNet)
                    {
                        existingCI.BestNet = netValue;
                        existingCI.DrugId = drug.Id;
                        existingCI.ScriptCode = record.Script;
                        existingCI.ScriptDateTime = recordDate;
                    }
                }
                else
                {
                    var newCI = new ClassInsurance
                    {
                        InsuranceId = insurance.Id,
                        InsuranceName = insurance.Name,
                        ClassId = classItem.Id,
                        DrugId = drug.Id,
                        BranchId = branch.Id,
                        Date = yearMonth,
                        ClassName = classItem.Name,
                        ScriptDateTime = yearMonth,
                        ScriptCode = record.Script,
                        BestNet = netValue
                    };
                    newClassInsurances.Add(newCI);
                    ciDict.Add(ciKey, newCI);
                }
            }

            // Now add only the new DrugInsurance and ClassInsurance records.
            _context.DrugInsurances.AddRange(newDrugInsurances);
            await _context.SaveChangesAsync();

            _context.ClassInsurances.AddRange(newClassInsurances);
            await _context.SaveChangesAsync();


            // ========================================================
            // PHASE 3: Process Users and Scripts
            // ========================================================
            // Preload Users, Branches, and Scripts.
            var userDict = await _context.Users.ToDictionaryAsync(u => u.ShortName);
            var scriptDict = await _context.Scripts.ToDictionaryAsync(s => s.ScriptCode);

            var newUsers = new List<User>();
            var newScripts = new List<Script>();
            var newDrugBranches = new List<DrugBranch>();
            // Process missing Users (record owner and prescriber).
            foreach (var record in processedRecords)
            {
                if (!branchDict.TryGetValue(record.Branch, out var branch))
                    continue;
                if (!drugDict.TryGetValue(record.NDCCode, out var drug))
                    continue;
                var tempkey = (branch.Id, drug.Id);
                if (!drugBranchDict.TryGetValue(tempkey, out var drugBranch))
                {
                    var newDrugBranch = new DrugBranch
                    {
                        BranchId = branch.Id,
                        DrugId = drug.Id
                    };
                    newDrugBranches.Add(newDrugBranch);
                    drugBranchDict.Add(tempkey, newDrugBranch);
                }
                if (!userDict.ContainsKey(record.User))
                {
                    var newUser = new User { ShortName = record.User, Name = record.User, Email = $"{record.User}@pharmacy.com", Password = "DefaultPass123", BranchId = branch.Id };
                    newUsers.Add(newUser);
                    userDict[record.User] = newUser;
                }
                if (!userDict.ContainsKey(record.Prescriber))
                {
                    var newPrescriber = new User { ShortName = record.Prescriber, Name = record.Prescriber, Email = $"{record.Prescriber}@pharmacy.com", Password = "DefaultPass123", BranchId = branch.Id };
                    newUsers.Add(newPrescriber);
                    userDict[record.Prescriber] = newPrescriber;
                }
            }
            if (newUsers.Any())
            {
                _context.Users.AddRange(newUsers);
                await _context.SaveChangesAsync();
            }
            if (newDrugBranches.Any())
            {
                _context.DrugBranches.AddRange(newDrugBranches);
                await _context.SaveChangesAsync();
            }

            // Process Scripts.
            foreach (var record in processedRecords)
            {
                DateTime recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture)
                                                .ToUniversalTime();
                if (!scriptDict.ContainsKey(record.Script))
                {
                    if (!branchDict.TryGetValue(record.Branch, out var branch))
                        continue;
                    // Use the record owner from userDict.
                    var owner = userDict[record.User];
                    var newScript = new Script
                    {
                        Date = recordDate,
                        ScriptCode = record.Script,
                        BranchId = branch.Id,
                        UserId = owner.Id
                    };
                    newScripts.Add(newScript);
                    scriptDict[record.Script] = newScript;
                }
            }
            if (newScripts.Any())
            {
                _context.Scripts.AddRange(newScripts);
                await _context.SaveChangesAsync();
            }

            // ========================================================
            // PHASE 4: Process ScriptItems
            // ========================================================
            // Build a temporary dictionary keyed by (ScriptId, DrugId)
            var tempScriptItems = new Dictionary<(int scriptId, int drugId), ScriptItem>();
            foreach (var record in processedRecords)
            {
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);
                DateTime recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture)
                                                .ToUniversalTime();

                if (!insuranceDict.TryGetValue(record.Insurance, out var insurance2))
                    continue;
                if (!drugDict.TryGetValue(record.NDCCode, out var drug2))
                    continue;
                if (!drugClassDict.TryGetValue(drug2.DrugClassId, out var classItem2))
                    continue;
                if (!scriptDict.TryGetValue(record.Script, out var script))
                    continue;

                var siKey = (script.Id, drug2.Id);
                if (tempScriptItems.TryGetValue(siKey, out var existingSI))
                {
                    if (script.Date < recordDate)
                    {
                        existingSI.AcquisitionCost = record.AcquisitionCost;
                        existingSI.Discount = record.Discount;
                        existingSI.InsurancePayment = record.InsurancePayment;
                        existingSI.PatientPayment = record.PatientPayment;
                    }
                }
                else
                {
                    if (!userDict.TryGetValue(record.Prescriber, out var prescriber))
                        continue;
                    var newSI = new ScriptItem
                    {
                        ScriptId = script.Id,
                        DrugId = drug2.Id,
                        InsuranceId = insurance2.Id,
                        DrugClassId = classItem2.Id,
                        RxNumber = record.RxNumber,
                        PrescriberId = prescriber.Id,
                        PF = record.PF,
                        Quantity = record.Quantity,
                        AcquisitionCost = record.AcquisitionCost,
                        Discount = record.Discount,
                        InsurancePayment = record.InsurancePayment,
                        PatientPayment = record.PatientPayment,
                        NDCCode = record.NDCCode
                    };
                    tempScriptItems.Add(siKey, newSI);
                }
            }
            _context.ScriptItems.AddRange(tempScriptItems.Values);
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
            var item = await _context.DrugInsurances.FirstOrDefaultAsync(x => x.NDCCode == ndc && x.InsuranceId == insurance.Id);
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
                .Where(x => x.DrugClassId == classItem.Id && x.InsuranceId == insuranceId).ToListAsync();

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

        // internal async Task<ICollection<string>> getDrugNDCsByNameInsuance(string drugName, int insurnaceId)
        // {
        //     var items = await _context.DrugInsurances.Where(x => x.InsuranceId == insurnaceId && x.DrugName == drugName).
        //     GroupBy(x => x.NDCCode).Select(d => d.Key).ToListAsync();
        //     return items;
        // }

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
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugs(int classId)
        {
            var query = from d in _context.Drugs.Where(d => d.DrugClassId == classId)
                        join dc in _context.DrugClasses on d.DrugClassId equals dc.Id
                        join di in _context.DrugInsurances.Where(di => di.DrugClassId == classId)
                            on d.Id equals di.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty()
                        select new { Drug = d, DrugClass = dc, DrugInsurance = di };

            var list = await query.ToListAsync();
            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);
            var insruaceDict = await _context.Insurances.ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                // When a matching DrugInsurance exists, map it using AutoMapper.
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override DrugClass property with the value from the DrugClasses table.
                    dto.DrugClass = item.DrugClass.Name;
                    // Optionally, set DrugName and NDCCode if needed.
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    if (insruaceDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insruace))
                        dto.insuranceName = insruace.Name;
                    if (branchDict.TryGetValue(item.DrugInsurance.BranchId, out var branch))
                        dto.branchName = branch.Name;
                    return dto;
                }
                else
                {
                    // No matching DrugInsurance exists, so create a default instance.
                    var defaultInsurance = new DrugInsurance
                    {
                        DrugId = item.Drug.Id,
                        NDCCode = item.Drug.NDC,
                        DrugClassId = item.Drug.DrugClassId,
                        Net = 0,
                        date = DateTime.UtcNow,
                        Prescriber = null,
                        Quantity = 0,
                        AcquisitionCost = 0,
                        Discount = 0,
                        InsurancePayment = 0,
                        PatientPayment = 0,
                        BranchId = 1,
                        InsuranceId = 0,
                        Drug = item.Drug
                    };

                    var dto = _mapper.Map<DrugsAlternativesReadDto>(defaultInsurance);
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClass = item.DrugClass.Name; // Set from DrugClasses table.
                    dto.insuranceName = null;
                    dto.branchName = null;
                    return dto;
                }
            }).ToList();

            return result;
        }
        internal async Task<Drug> GetDrugById(int id)
        {
            var item = await _context.Drugs.FirstOrDefaultAsync(x => x.Id == id);
            return item;
        }

        // internal async Task oneway()
        // {
        //     var drugInsurances = await _context.DrugInsurances.ToListAsync();
        //     var insurances = await _context.Insurances.ToDictionaryAsync(i => i.Id, i => i.Name);

        //     foreach (var drugInsurance in drugInsurances)
        //     {
        //         if (insurances.TryGetValue(drugInsurance.InsuranceId, out var insuranceName))
        //         {
        //             drugInsurance.insuranceName = insuranceName;
        //         }
        //     }

        //     await _context.SaveChangesAsync();
        // }

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
            [Name("Branch")]
            public string Branch { get; set; }
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
            [Default(0)]
            public decimal Rxcui { get; set; }

            [Name("drug_class")]
            public string? DrugClass { get; set; }
        }
        internal async Task<ICollection<AuditReadDto>> GetAllLatestScripts()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.Insurances on scriptItem.InsuranceId equals insurance.Id
                join classItem in _context.DrugClasses on scriptItem.DrugClassId equals classItem.Id
                join branch in _context.Branches on script.BranchId equals branch.Id
                join classInsurance in _context.ClassInsurances
                    on new { InsuranceId = insurance.Id, ClassId = classItem.Id, Year = script.Date.Year, Month = script.Date.Month, BranchId = branch.Id }
                    equals new { classInsurance.InsuranceId, classInsurance.ClassId, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month, classInsurance.BranchId }
                join scriptDrug in _context.Drugs on scriptItem.DrugId equals scriptDrug.Id // Drug from script
                join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id // Best drug
                join user in _context.Users on script.UserId equals user.Id into userGroup
                from user in userGroup.DefaultIfEmpty() // Allow nullable users

                join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
                from prescriber in prescriberGroup.DefaultIfEmpty() // Allow nullable prescribers

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

                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth // Use previous month if available, otherwise fallback to current month

                select new AuditReadDto
                {
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = scriptItem.RxNumber,
                    User = user != null ? user.Name : "Unknown",

                    // Drug from script
                    DrugName = scriptDrug.Name,
                    NDCCode = scriptDrug.NDC,
                    DrugId = scriptDrug.Id,

                    // Best Drug
                    HighstDrugName = bestDrug.Name,
                    HighstDrugNDC = bestDrug.NDC,
                    HighstDrugId = bestDrug.Id,
                    BranchCode = branch.Name,
                    Insurance = insurance.Name,
                    PF = scriptItem.PF,
                    Prescriber = prescriber != null ? prescriber.Name : "Unknown",
                    Quantity = scriptItem.Quantity,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    NetProfit = scriptItem.NetProfit,
                    DrugClass = classItem.Name,
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
        internal async Task<Script> GetScriptAsync(string scriptCode)
        {
            var item = await _context.Scripts.FirstOrDefaultAsync(x => x.ScriptCode == scriptCode);
            return item;
        }
        internal async Task<ICollection<ScriptItemDto>> GetScriptByScriptCode(string scriptCode)
        {
            var items = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join branch in _context.Branches on script.BranchId equals branch.Id
                join drug in _context.Drugs on scriptItem.DrugId equals drug.Id
                join insurance in _context.Insurances on scriptItem.InsuranceId equals insurance.Id
                join drugClass in _context.DrugClasses on scriptItem.DrugClassId equals drugClass.Id
                join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
                join user in _context.Users on script.UserId equals user.Id
                from prescriber in prescriberGroup.DefaultIfEmpty() // Allow null prescriber

                where script.ScriptCode == scriptCode

                select new ScriptItemDto
                {
                    Id = scriptItem.Id,
                    DrugName = drug.Name,
                    NDCCode = scriptItem.NDCCode,
                    Quantity = scriptItem.Quantity,
                    PF = scriptItem.PF,
                    InsuranceName = insurance.Name,
                    DrugClassName = drugClass.Name,
                    PrescriberName = prescriber != null ? prescriber.Name : "Unknown",
                    UserName = user.Name,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    BranchName = branch.Name
                }
            ).ToListAsync();

            return items;
        }

        public async Task ImportInsurancesFromCsvAsync(string filePath = "insurance.csv")
        {
            List<Insurance> csvRecords;
            // Parse the CSV file.
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<InsuranceMap>();
                csvRecords = csv.GetRecords<Insurance>().ToList();
            }
            var insuranceDic = (await _context.Insurances.ToListAsync())
                               .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var newInsurances = new List<Insurance>();
            foreach (var csvRecord in csvRecords)
            {
                // If no short name is provided, skip this record.
                if (string.IsNullOrWhiteSpace(csvRecord.Name))
                    continue;

                // Convert to lower-case for a case-insensitive comparison.
                if (insuranceDic.TryGetValue(csvRecord.Name, out var existingInsurance))
                {  // Update the existing record.
                    existingInsurance.Bin = csvRecord.Bin;
                    existingInsurance.Pcn = csvRecord.Pcn;
                    existingInsurance.Description = csvRecord.Description;
                    existingInsurance.HelpDeskNumber = csvRecord.HelpDeskNumber;
                }
                else
                {
                    newInsurances.Add(csvRecord);
                }
            }
            _context.Insurances.AddRange(newInsurances);

            await _context.SaveChangesAsync();
        }

        internal async Task<Insurance> GetInsuranceDetails(string shortName)
        {
            var item = await _context.Insurances.FirstOrDefaultAsync(x => x.Name == shortName);
            return item;
        }

        internal async Task<ICollection<Drug>> GetDrugsByClassBranch(int classId, int branchId)
        {
            var items = await (
                from drug in _context.Drugs
                join db in _context.DrugBranches on drug.Id equals db.DrugId
                where drug.DrugClassId == classId && db.BranchId == branchId
                select drug
            ).ToListAsync();

            return items;
        }

        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAlternativesByClassIdBranchId(int classId, int branchId)
        {
            var query = from d in _context.Drugs.Where(d => d.DrugClassId == classId)
                        join dc in _context.DrugClasses on d.DrugClassId equals dc.Id
                        join di in _context.DrugInsurances.Where(di => di.DrugClassId == classId && branchId == di.BranchId)
                            on d.Id equals di.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty()
                        select new { Drug = d, DrugClass = dc, DrugInsurance = di };

            var list = await query.ToListAsync();
            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);
            var insruaceDict = await _context.Insurances.ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                // When a matching DrugInsurance exists, map it using AutoMapper.
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override DrugClass property with the value from the DrugClasses table.
                    dto.DrugClass = item.DrugClass.Name;
                    // Optionally, set DrugName and NDCCode if needed.
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    if (insruaceDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insruace))
                        dto.insuranceName = insruace.Name;
                    if (branchDict.TryGetValue(item.DrugInsurance.BranchId, out var branch))
                        dto.branchName = branch.Name;
                    return dto;
                }
                else
                {
                    // No matching DrugInsurance exists, so create a default instance.
                    var defaultInsurance = new DrugInsurance
                    {
                        DrugId = item.Drug.Id,
                        NDCCode = item.Drug.NDC,
                        DrugClassId = item.Drug.DrugClassId,
                        Net = 0,
                        date = DateTime.UtcNow,
                        Prescriber = null,
                        Quantity = 0,
                        AcquisitionCost = 0,
                        Discount = 0,
                        InsurancePayment = 0,
                        PatientPayment = 0,
                        BranchId = 1,
                        InsuranceId = 0,
                        Drug = item.Drug
                    };

                    var dto = _mapper.Map<DrugsAlternativesReadDto>(defaultInsurance);
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClass = item.DrugClass.Name; // Set from DrugClasses table.
                    dto.insuranceName = null;
                    dto.branchName = null;
                    return dto;
                }
            }).ToList();

            return result;
        }
    }
    public sealed class InsuranceMap : ClassMap<Insurance>
    {
        public InsuranceMap()
        {
            // Map CSV columns to Insurance properties.
            Map(m => m.Bin).Name("bin");
            Map(m => m.Pcn).Name("pcn");
            Map(m => m.Description).Name("description");
            Map(m => m.HelpDeskNumber).Name("help_desk_number");
            // Map "shortnames (Ins)" header to the Name property.
            Map(m => m.Name).Name("shortnames (Ins)");
        }
    }

}