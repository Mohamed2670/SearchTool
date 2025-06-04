using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Dtos;
using SearchTool_ServerSide.Dtos.DrugDtos;
using SearchTool_ServerSide.Dtos.InsuranceDtos.cs;
using SearchTool_ServerSide.Dtos.ScritpsDto;
using SearchTool_ServerSide.Models;
using ServerSide.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SearchTool_ServerSide.Repository
{
    public class DrugRepository : GenericRepository<Drug>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        public DrugRepository(SearchToolDBContext context, IMapper mapper, IMemoryCache cache) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<ICollection<Drug>> GetDrugsByName(string name, int pageNumber, int pageSize = 20)
        {
            var items = await _context.Drugs
                .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .GroupBy(x => x.Name.ToLower())
                .Select(g => g.First())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return items;
        }
        public async Task<ICollection<Drug>> GetClassesByName(string name, int pageNumber, int pageSize = 20)
        {
            var items = await _context.Drugs
                .Include(di => di.DrugClass)
                .Where(x => x.DrugClass.Name.ToLower().Contains(name.ToLower()))
                .GroupBy(x => x.DrugClass.Name.ToLower())
                .Select(g => g.First())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return items;
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
            var filePath = "drug_enriched_with_group.csv";

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
            var drugClassesV2 = await context.DrugClassV2s.ToDictionaryAsync(dc => dc.Name, dc => dc);
            var drugClassesV3 = await context.DrugClassV3s.ToDictionaryAsync(dc => dc.Name, dc => dc);
            // **Step 3: Load Existing Drugs by NDC & Name**
            var existingDrugsByNdc = await context.Drugs
                .GroupBy(d => d.NDC)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var existingDrugsByName = await context.Drugs
                .GroupBy(d => d.Name)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var newDrugs = new List<Drug>();
            var newDrugClasses = new List<DrugClass>();
            var newDrugClassesV2 = new List<DrugClassV2>();
            var newDrugClassesV3 = new List<DrugClassV3>();
            foreach (var record in records)
            {
                if (!drugClasses.TryGetValue(record.DrugClass, out var drugClass))
                {
                    drugClass = new DrugClass { Name = record.DrugClass };
                    newDrugClasses.Add(drugClass);
                    drugClasses[record.DrugClass] = drugClass; // Add to dictionary for future lookups
                }
                if (!drugClassesV2.TryGetValue(record.ClassV2, out var classV2))
                {
                    // Console.WriteLine($"Adding new DrugClassV2: {record.ClassV2}");

                    classV2 = new DrugClassV2 { Name = record.ClassV2 };
                    newDrugClassesV2.Add(classV2);
                    drugClassesV2[record.ClassV2] = classV2; // Add to dictionary for future lookups
                }
                if (!drugClassesV3.TryGetValue(record.ClassV3, out var classV3))
                {
                    // Console.WriteLine($"Adding new DrugClassV3: {record.ClassV3}");

                    classV3 = new DrugClassV3 { Name = record.ClassV3 };
                    newDrugClassesV3.Add(classV3);
                    drugClassesV3[record.ClassV3] = classV3; // Add to dictionary for future lookups
                }
            }

            // Batch insert new drug classes
            if (newDrugClasses.Any() || newDrugClassesV2.Any() || newDrugClassesV3.Any())
            {
                await context.DrugClasses.AddRangeAsync(newDrugClasses);
                await context.DrugClassV2s.AddRangeAsync(newDrugClassesV2);
                await context.DrugClassV3s.AddRangeAsync(newDrugClassesV3);

                await context.SaveChangesAsync();
                Console.WriteLine($"Added {newDrugClasses.Count} new drug classes at {DateTime.Now}");
            }

            foreach (var record in records)
            {
                record.Name = record.Name.ToUpper();
                if (record.DrugClass == null)
                {
                    record.DrugClass = "Unknown";
                }
                string tempNdc = NormalizeNdcTo11Digits(record.NDC);

                // **Check if Drug Exists by NDC**
                if (existingDrugsByNdc.ContainsKey(tempNdc))
                {
                    Console.WriteLine($"Drug with NDC {tempNdc} already exists.");
                    continue; // Skip existing drug
                }

                // **Check if Drug Exists by Name**
                // if (existingDrugsByName.TryGetValue(record.Name, out var existingDrug))
                // {
                //     var newDrug = new Drug
                //     {
                //         Name = record.Name,
                //         NDC = tempNdc,
                //         Form = existingDrug.Form,
                //         Strength = existingDrug.Strength,
                //         DrugClassId = existingDrug.DrugClassId,
                //         ACQ = record.ACQ ?? 0,
                //         AWP = record.AWP ?? 0,
                //         Rxcui = existingDrug.Rxcui,
                //         Route = record.Route,
                //         Ingrdient = record.Ingrdient,
                //         TECode = record.TECode,
                //         ApplicationNumber = record.ApplicationNumber,
                //         ApplicationType = record.ApplicationType
                //     };

                //     newDrugs.Add(newDrug);
                //     existingDrugsByNdc[tempNdc] = newDrug; // Add to dictionary
                // }
                // else
                {
                    // **Create Drug Class if Missing**
                    var newDrug = new Drug
                    {
                        Name = record.Name,
                        NDC = tempNdc,
                        Form = record.Form,
                        Strength = record.Strength,
                        DrugClassId = drugClasses[record.DrugClass].Id,
                        DrugClassV2Id = drugClassesV2[record.ClassV2].Id,
                        DrugClassV3Id = drugClassesV3[record.ClassV3].Id,
                        ACQ = record.ACQ ?? 0,
                        AWP = record.AWP ?? 0,
                        Rxcui = record.Rxcui,
                        Route = record.Route,
                        Ingrdient = record.Ingrdient,
                        TECode = record.TECode,
                        ApplicationNumber = record.ApplicationNumber,
                        ApplicationType = record.ApplicationType,
                        StrengthUnit = record.Unit,
                        Type = record.Type
                    };

                    newDrugs.Add(newDrug);
                    existingDrugsByNdc[tempNdc] = newDrug; // Add to dictionary
                }

                // **Batch Processing**
                if (newDrugs.Count >= 10000)
                {
                    await context.Drugs.AddRangeAsync(newDrugs);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Processed batch of 10,000 drugs at {DateTime.Now}");
                    newDrugs.Clear();
                }
            }

            // Save remaining drugs
            if (newDrugs.Any())
            {
                await context.Drugs.AddRangeAsync(newDrugs);
                await context.SaveChangesAsync();
                Console.WriteLine($"Processed final batch of {newDrugs.Count} drugs at {DateTime.Now}");
            }
        }
        public async Task AddMediCare()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string filePath = @"Medical_with_NADAC_Data.xlsx";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Opening the file...");

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            using var reader1 = ExcelReaderFactory.CreateReader(stream);
            using var reader2 = ExcelReaderFactory.CreateReader(stream);

            int rowIndex = 0;
            var existingDrugsByNdc = await _context.Drugs
               .GroupBy(d => d.NDC)
               .ToDictionaryAsync(g => g.Key, g => g.First());
            var existingClass = await _context.DrugClasses
               .GroupBy(d => d.Name)
               .ToDictionaryAsync(g => g.Key, g => g.First());
            var existingClassV2 = await _context.DrugClassV2s.GroupBy(d => d.Name)
               .ToDictionaryAsync(g => g.Key, g => g.First());
            var existingClassV3 = await _context.DrugClassV3s.GroupBy(d => d.Name)
                .ToDictionaryAsync(g => g.Key, g => g.First());
            var existingDrugMediByNDC = await _context.DrugMedis
               .GroupBy(d => d.DrugNDC)
               .ToDictionaryAsync(g => g.Key, g => g.First());
            var drugBranchDict = await _context.DrugBranches.ToDictionaryAsync(g => (g.BranchId, g.DrugId));
            var diDict = await _context.DrugInsurances.ToDictionaryAsync(di => (di.InsuranceId, di.DrugId, di.BranchId));
            var drugMedis = new List<DrugMedi>();
            var unmatched = new List<string>();
            var drugBranches = new List<DrugBranch>();
            var newDrugInsurances = new List<DrugInsurance>();
            var newDrug = new List<Drug>();
            var newClass = new List<DrugClass>();
            var newClassV2 = new List<DrugClassV2>();
            var newClassV3 = new List<DrugClassV3>();
            var insuranceBin = await _context.Insurances.FirstOrDefaultAsync(x => x.Bin == "022659");
            if (insuranceBin == null)
            {
                _context.Insurances.Add(new Insurance
                {
                    Bin = "022659",
                    Name = "Medi-Cal"
                });
                await _context.SaveChangesAsync();
                insuranceBin = await _context.Insurances.FirstOrDefaultAsync(x => x.Bin == "022659");
                var insurancePcn = await _context.InsurancePCNs.FirstOrDefaultAsync(x => x.InsuranceId == insuranceBin.Id);
                if (insurancePcn == null)
                {
                    _context.InsurancePCNs.Add(new InsurancePCN
                    {
                        PCN = "Medi-Cal",
                        InsuranceId = insuranceBin.Id
                    });
                    await _context.SaveChangesAsync();
                }
                insurancePcn = await _context.InsurancePCNs.FirstOrDefaultAsync(x => x.InsuranceId == insuranceBin.Id);
                var insuranceRx = await _context.InsuranceRxes.FirstOrDefaultAsync(x => x.InsurancePCNId == insurancePcn.Id);
                if (insuranceRx == null)
                {
                    _context.InsuranceRxes.Add(new InsuranceRx
                    {
                        RxGroup = "Medi-Cal",
                        InsurancePCNId = insurancePcn.Id
                    });
                    await _context.SaveChangesAsync();
                }
            }
            var insurance = await _context.InsuranceRxes.FirstOrDefaultAsync(x => x.RxGroup == "Medi-Cal");
            while (reader1.Read())
            {
                var productId = NormalizeNdcTo11Digits(reader1.GetValue(0)?.ToString());
                var labelName = reader1.GetValue(1)?.ToString();
                if (!existingClass.TryGetValue(labelName, out var drugClass))
                {
                    drugClass = new DrugClass
                    {
                        Name = labelName
                    };
                    newClass.Add(drugClass);
                    existingClass[labelName] = drugClass;
                }
                if (!existingClassV2.TryGetValue(labelName, out var drugClassV2))
                {
                    drugClassV2 = new DrugClassV2
                    {
                        Name = labelName
                    };
                    newClassV2.Add(drugClassV2);
                    existingClassV2[labelName] = drugClassV2;
                }
                if (!existingClassV3.TryGetValue(labelName, out var drugClassV3))
                {
                    drugClassV3 = new DrugClassV3
                    {
                        Name = labelName
                    };
                    newClassV3.Add(drugClassV3);
                    existingClassV3[labelName] = drugClassV3;
                }
            }
            if (newClass.Any())
            {
                await _context.DrugClasses.AddRangeAsync(newClass);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newClass.Count} drugs at {DateTime.Now}");
            }
            if (newClassV2.Any())
            {
                await _context.DrugClassV2s.AddRangeAsync(newClassV2);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newClassV2.Count} drugs at {DateTime.Now}");
            }
            if (newClassV3.Any())
            {
                await _context.DrugClassV3s.AddRangeAsync(newClassV3);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newClassV3.Count} drugs at {DateTime.Now}");
            }
            rowIndex = 0;
            // Reset the row index for the main processing loop
            while (reader2.Read())
            {
                rowIndex++;
                if (rowIndex == 1)
                    continue;
                var productId = NormalizeNdcTo11Digits(reader2.GetValue(0)?.ToString());
                var labelName = reader2.GetValue(1)?.ToString();
                var priorAuth = reader2.GetValue(3)?.ToString();
                var extended = reader2.GetValue(4)?.ToString();
                var CostringTier = reader2.GetValue(5)?.ToString();
                var NotComp = reader2.GetValue(6)?.ToString();
                var ccp = reader2.GetValue(7)?.ToString();
                var insurancePay = reader2.GetValue(8)?.ToString();
                var unit = reader2.GetValue(9)?.ToString();
                var dateTime = reader2.GetValue(10)?.ToString();

                if (!existingDrugsByNdc.TryGetValue(productId, out var x))
                {
                    var drugClass = existingClass[labelName];
                    var drugClassV2 = existingClassV2[labelName];
                    var drugClassV3 = existingClassV3[labelName];
                    var newDrugItem = new Drug
                    {
                        NDC = productId,
                        Name = labelName,
                        DrugClassId = drugClass.Id,
                        DrugClassV2Id = drugClassV2.Id,
                        DrugClassV3Id = drugClassV3.Id,
                        Form = "NA",
                        Strength = "NA",
                        ACQ = 0,
                        AWP = 0,
                        Rxcui = 0,
                        Route = "NA",
                        Ingrdient = "NA",
                        TECode = "NA",
                        ApplicationNumber = "NA",
                        ApplicationType = "NA",
                        StrengthUnit = unit,
                        Type = "NA"
                    };
                    newDrug.Add(newDrugItem);
                    existingDrugsByNdc[productId] = newDrugItem;
                }

            }
            if (newDrug.Any())
            {
                await _context.Drugs.AddRangeAsync(newDrug);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newDrug.Count} drugs at {DateTime.Now}");
            }
            rowIndex = 0;
            while (reader.Read())
            {
                rowIndex++;
                if (rowIndex == 1)
                    continue;
                var productId = NormalizeNdcTo11Digits(reader.GetValue(0)?.ToString());
                var labelName = reader.GetValue(1)?.ToString();
                var priorAuth = reader.GetValue(3)?.ToString();
                var extended = reader.GetValue(4)?.ToString();
                var CostringTier = reader.GetValue(5)?.ToString();
                var NotComp = reader.GetValue(6)?.ToString();
                var ccp = reader.GetValue(7)?.ToString();
                var insurancePay = reader.GetValue(8)?.ToString();
                var unit = reader.GetValue(9)?.ToString();
                var dateTime = reader.GetValue(10)?.ToString();

                if (existingDrugsByNdc.TryGetValue(productId, out var drug))
                {

                    var diKey = (insurance.Id, drug.Id, 1);
                    if (!diDict.TryGetValue(diKey, out var existingDI))
                    {
                        var newDI = new DrugInsurance
                        {
                            InsuranceId = insurance.Id,
                            DrugId = drug.Id,
                            BranchId = 6,
                            NDCCode = drug.NDC,
                            Net = 0,
                            DrugClassId = drug.DrugClassId,
                            DrugClassV2Id = drug.DrugClassV2Id,
                            DrugClassV3Id = drug.DrugClassV3Id,
                            date = dateTime != null
                                ? (DateTime.TryParseExact(dateTime,
                                                        new[] { "MM-dd-yy", "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd" },
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out var parsedDate)
                                    ? parsedDate.ToUniversalTime()
                                    : DateTime.UtcNow)
                                : DateTime.UtcNow,
                            Prescriber = "",
                            Quantity = "",
                            AcquisitionCost = 0,
                            Discount = 0,
                            InsurancePayment = insurancePay != null
                                ? (decimal.Parse(insurancePay) * 0.9m )
                                : 0m,
                            PatientPayment = 0,
                        };
                        newDrugInsurances.Add(newDI);
                        diDict.Add(diKey, newDI);
                    }




                    if (!drugBranchDict.ContainsKey((1, drug.Id)))
                    {
                        var newDrugBranch = new DrugBranch
                        {
                            BranchId = 6,
                            DrugId = drug.Id
                        };
                        drugBranches.Add(newDrugBranch);
                    }
                    if (!existingDrugMediByNDC.ContainsKey(productId))
                    {
                        // Console.WriteLine($"Adding new DrugMedi for NDC: {drug.Id}");
                        // Console.ReadKey();
                        var newDrugMedei = new DrugMedi
                        {
                            DrugId = drug.Id,
                            DrugNDC = drug.NDC,
                            PriorAuthorization = priorAuth,
                            ExtendedDuration = extended,
                            CostCeilingTier = CostringTier,
                            NonCapitatedDrugIndicator = NotComp,
                            CCSPanelAuthority = ccp ?? "NA"
                        };
                        drugMedis.Add(newDrugMedei);
                    }
                    // Console.WriteLine($"Product ID: {productId}, Prior Auth: {priorAuth} " +
                    //     $", Extended: {extended}, Costring Tier: {CostringTier}, Not Comp: {NotComp}");
                }
                else
                {
                    //store this in list after that at file.txt
                    unmatched.Add(productId);
                }
            }
            // Save unmatched NDCs to a file
            if (unmatched.Any())
            {
                File.WriteAllLines("unmatched.txt", unmatched);
                Console.WriteLine($"Unmatched NDCs saved to unmatched.txt");
            }


            if (drugBranches.Any())
            {
                await _context.DrugBranches.AddRangeAsync(drugBranches);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {drugBranches.Count} drugs at {DateTime.Now}");
            }
            if (drugMedis.Any())
            {
                await _context.DrugMedis.AddRangeAsync(drugMedis);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {drugMedis.Count} drugs at {DateTime.Now}");
            }
            if (newDrugInsurances.Any())
            {
                await _context.DrugInsurances.AddRangeAsync(newDrugInsurances);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newDrugInsurances.Count} drugs at {DateTime.Now}");
            }

            stopwatch.Stop();
            Console.WriteLine($"Finished in {stopwatch.ElapsedMilliseconds} ms");
        }



        public async Task AddScripts(ICollection<ScriptAddDto> scriptAddDtos)
        {


            // Keep only records that yield a valid Drug.
            var processedRecords = new List<ScriptAddDto>();

            // ========================================================
            // PHASE 1: Process Principal Entities – Insurances & Drugs
            // ========================================================
            // Preload existing Insurances, Drugs, and DrugClasses.
            var insuranceDict = await _context.Insurances.ToDictionaryAsync(i => i.Bin);
            var insurancePCNDict = await _context.InsurancePCNs.ToDictionaryAsync(i => i.PCN);
            var insuranceRxDict = await _context.InsuranceRxes.ToDictionaryAsync(i => i.RxGroup);
            var drugsFromDb = await _context.Drugs.ToListAsync();
            // Key by normalized NDC.
            var drugDict = drugsFromDb.ToDictionary(d => d.NDC);
            // Key by drug name (using grouping to avoid duplicate key issues)
            var drugByNameDict = drugsFromDb
                                    .GroupBy(d => d.Name)
                                    .ToDictionary(g => g.Key, g => g.First());
            var drugClassDict = await _context.DrugClasses.ToDictionaryAsync(dc => dc.Id);
            var drugClassV2Dict = await _context.DrugClassV2s.ToDictionaryAsync(dc => dc.Id);
            var drugClassV3Dict = await _context.DrugClassV3s.ToDictionaryAsync(dc => dc.Id);

            var newInsurances = new List<Insurance>();
            var newInsurancePCNs = new List<InsurancePCN>();
            var newInsuranceRxes = new List<InsuranceRx>();

            var newDrugs = new List<Drug>();
            int batchSize = 1000, countPhase1 = 0;

            foreach (var record in scriptAddDtos)
            {
                record.Bin = record.Bin.ToUpper();
                record.PCN = record.PCN.ToUpper();
                record.RxGroup = record.RxGroup.ToUpper();
                record.DrugName = record.DrugName.ToUpper();
                if (record.Bin.Length < 6)
                {
                    record.Bin = record.Bin.PadLeft(6, '0');
                }
                if (record.PCN.Length < 1)
                {
                    record.PCN = record.Bin + "(Other)";
                }
                if (record.RxGroup.Length < 1)
                {

                    record.RxGroup = record.PCN + "(Other)";

                }
                record.RxGroup = record.RxGroup.Trim();
                // Normalize NDC.
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);
                // ---- Process Insurance
                if (!insuranceDict.ContainsKey(record.Bin))
                {
                    var ins = new Insurance { Bin = record.Bin };
                    newInsurances.Add(ins);
                    insuranceDict[record.Bin] = ins; // will have generated Id after saving
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
                if (!insuranceDict.TryGetValue(record.Bin, out var insurance))
                    continue;

                if (!insurancePCNDict.ContainsKey(record.PCN))
                {
                    var ins = new InsurancePCN { PCN = record.PCN, InsuranceId = insurance.Id };
                    newInsurancePCNs.Add(ins);
                    insurancePCNDict[record.PCN] = ins;
                }
            }
            if (newInsurancePCNs.Any())
            {
                _context.InsurancePCNs.AddRange(newInsurancePCNs);
                await _context.SaveChangesAsync();
            }
            foreach (var record in processedRecords)
            {
                if (!insurancePCNDict.TryGetValue(record.PCN, out var insurancePCN))
                    continue;

                if (!insuranceRxDict.ContainsKey(record.RxGroup))
                {
                    var ins = new InsuranceRx { RxGroup = record.RxGroup, InsurancePCNId = insurancePCN.Id };
                    newInsuranceRxes.Add(ins);
                    insuranceRxDict[record.RxGroup] = ins;
                }
            }
            if (newInsuranceRxes.Any())
            {
                _context.InsuranceRxes.AddRange(newInsuranceRxes);
                await _context.SaveChangesAsync();
            }
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
                if (!insuranceRxDict.TryGetValue(record.RxGroup, out var insuranceItem))
                {
                    Console.WriteLine("hoooooooooooooo");
                    continue;
                }
                if (!drugDict.TryGetValue(record.NDCCode, out var drug))
                    continue;
                if (!drugClassDict.TryGetValue(drug.DrugClassId, out var classItem))
                    continue;

                // -----------------------
                // Merge DrugInsurance
                // ------------------
                if (!branchDict.TryGetValue(record.Branch, out var branch))
                    continue;

                var diKey = (insuranceItem.Id, drug.Id, branch.Id);
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
                        InsuranceId = insuranceItem.Id,
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

                var ciKey = (insuranceItem.Id, classItem.Id, recordDate.Year, recordDate.Month, branch.Id);
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
                        InsuranceId = insuranceItem.Id,
                        InsuranceName = insuranceItem.RxGroup,
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

                if (!insuranceRxDict.TryGetValue(record.RxGroup, out var insurance2))
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
            var insuranceDict = await _context.Insurances.ToDictionaryAsync(i => i.Bin);
            var insurancePCNDict = await _context.InsurancePCNs.ToDictionaryAsync(i => i.PCN);
            var insuranceRxDict = await _context.InsuranceRxes.ToDictionaryAsync(i => i.RxGroup);
            var drugsFromDb = await _context.Drugs.ToListAsync();
            // Key by normalized NDC.
            var drugDict = drugsFromDb
                                    .GroupBy(d => d.NDC)
                                    .ToDictionary(g => g.Key, g => g.First());
            // Key by drug name (using grouping to avoid duplicate key issues)
            var drugByNameDict = drugsFromDb
                                    .GroupBy(d => d.Name)
                                    .ToDictionary(g => g.Key, g => g.First());
            var drugClassDict = await _context.DrugClasses.ToDictionaryAsync(dc => dc.Id);
            var newInsurances = new List<Insurance>();
            var newInsurancePCNs = new List<InsurancePCN>();
            var newInsuranceRxes = new List<InsuranceRx>();

            var newDrugs = new List<Drug>();
            int batchSize = 1000, countPhase1 = 0;

            foreach (var record in records)
            {
                record.Bin = record.Bin.ToUpper();
                record.PCN = record.PCN.ToUpper();
                record.RxGroup = record.RxGroup.ToUpper();
                record.DrugName = record.DrugName.ToUpper();
                if (record.Bin.Length < 6)
                {
                    record.Bin = record.Bin.PadLeft(6, '0');
                }
                if (record.PCN.Length < 1)
                {
                    record.PCN = record.Bin + "(Other)";
                }
                if (record.RxGroup.Length < 1)
                {

                    record.RxGroup = record.PCN + "(Other)";

                }
                record.RxGroup = record.RxGroup.Trim();
                // Normalize NDC.
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);
                if (string.IsNullOrWhiteSpace(record.NDCCode) || record.NDCCode == "00000000000")
                {
                    Console.WriteLine($"Skipping record with invalid NDC: {record.NDCCode}");
                    continue;
                }
                // ---- Process Insurance
                if (!insuranceDict.ContainsKey(record.Bin))
                {
                    var ins = new Insurance { Bin = record.Bin };
                    newInsurances.Add(ins);
                    insuranceDict[record.Bin] = ins; // will have generated Id after saving
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
                            DrugClassV2Id = tempDrug.DrugClassV2Id,
                            DrugClassV3Id = tempDrug.DrugClassV3Id,
                            ACQ = record.AcquisitionCost,
                            AWP = 0,
                            Rxcui = tempDrug.Rxcui,
                            Route = tempDrug.Route,
                            Ingrdient = tempDrug.Ingrdient,
                            TECode = tempDrug.TECode,
                            ApplicationNumber = tempDrug.ApplicationNumber,
                            ApplicationType = tempDrug.ApplicationType
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
            var existingClassInsuranceV2s = await _context.ClassInsuranceV2s.ToListAsync();
            var existingClassInsuranceV3s = await _context.ClassInsuranceV3s.ToListAsync();

            var ciDict = existingClassInsurances
                .ToDictionary(ci => (ci.InsuranceId, ci.ClassId, ci.Date.Year, ci.Date.Month, ci.BranchId));
            var ci2Dict = existingClassInsuranceV2s
            .ToDictionary(ci => (ci.InsuranceId, ci.DrugClassV2Id, ci.Date.Year, ci.Date.Month, ci.BranchId));
            var ci3Dict = existingClassInsuranceV3s
            .ToDictionary(ci => (ci.InsuranceId, ci.DrugClassV3Id, ci.Date.Year, ci.Date.Month, ci.BranchId));
            var drugBranchDict = await _context.DrugBranches.ToDictionaryAsync(g => (g.BranchId, g.DrugId));
            // Lists for new inserts.
            var newDrugInsurances = new List<DrugInsurance>();
            var newClassInsurances = new List<ClassInsurance>();
            var newClassInsuranceV2s = new List<ClassInsuranceV2>();
            var newClassInsuranceV3s = new List<ClassInsuranceV3>();


            foreach (var record in processedRecords)
            {
                if (!insuranceDict.TryGetValue(record.Bin, out var insurance))
                    continue;

                if (!insurancePCNDict.ContainsKey(record.PCN))
                {
                    var ins = new InsurancePCN { PCN = record.PCN, InsuranceId = insurance.Id };
                    newInsurancePCNs.Add(ins);
                    insurancePCNDict[record.PCN] = ins;
                }
            }
            if (newInsurancePCNs.Any())
            {
                _context.InsurancePCNs.AddRange(newInsurancePCNs);
                await _context.SaveChangesAsync();
            }
            foreach (var record in processedRecords)
            {
                if (!insurancePCNDict.TryGetValue(record.PCN, out var insurancePCN))
                    continue;

                if (!insuranceRxDict.ContainsKey(record.RxGroup))
                {
                    var ins = new InsuranceRx { RxGroup = record.RxGroup, InsurancePCNId = insurancePCN.Id };
                    newInsuranceRxes.Add(ins);
                    insuranceRxDict[record.RxGroup] = ins;
                }
            }
            if (newInsuranceRxes.Any())
            {
                _context.InsuranceRxes.AddRange(newInsuranceRxes);
                await _context.SaveChangesAsync();
            }
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
                if (!insuranceRxDict.TryGetValue(record.RxGroup, out var insuranceItem))
                {
                    Console.WriteLine("hoooooooooooooo");
                    continue;
                }
                if (!drugDict.TryGetValue(record.NDCCode, out var drug))
                    continue;
                if (!drugClassDict.TryGetValue(drug.DrugClassId, out var classItem))
                    continue;

                // -----------------------
                // Merge DrugInsurance
                // ------------------
                if (!branchDict.TryGetValue(record.Branch, out var branch))
                    continue;

                var diKey = (insuranceItem.Id, drug.Id, branch.Id);
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
                        InsuranceId = insuranceItem.Id,
                        DrugId = drug.Id,
                        BranchId = branch.Id,
                        NDCCode = record.NDCCode,
                        Net = netValue,
                        DrugClassId = classItem.Id,
                        DrugClassV2Id = drug.DrugClassV2Id,
                        DrugClassV3Id = drug.DrugClassV3Id,
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

                var ciKey = (insuranceItem.Id, classItem.Id, recordDate.Year, recordDate.Month, branch.Id);
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
                        InsuranceId = insuranceItem.Id,
                        InsuranceName = insuranceItem.RxGroup,
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
                //////////ClassInsurace V2//////////////////////
                if (ci2Dict.TryGetValue(ciKey, out var existingCIV2))
                {
                    // Update if this record has a higher net value.
                    if (netValue > existingCIV2.BestNet)
                    {
                        existingCIV2.BestNet = netValue;
                        existingCIV2.DrugId = drug.Id;
                        existingCIV2.ScriptCode = record.Script;
                        existingCIV2.ScriptDateTime = recordDate;
                    }
                }
                else
                {

                    var newCI = new ClassInsuranceV2
                    {
                        InsuranceId = insuranceItem.Id,
                        InsuranceName = insuranceItem.RxGroup,
                        DrugClassV2Id = drug.DrugClassV2Id,
                        DrugId = drug.Id,
                        BranchId = branch.Id,
                        Date = yearMonth,
                        ClassName = classItem.Name,
                        ScriptDateTime = yearMonth,
                        ScriptCode = record.Script,
                        BestNet = netValue
                    };
                    newClassInsuranceV2s.Add(newCI);
                    ci2Dict.Add(ciKey, newCI);
                }
                //////////ClassInsurace V3//////////////////////
                if (ci3Dict.TryGetValue(ciKey, out var existingCIV3))
                {
                    // Update if this record has a higher net value.
                    if (netValue > existingCIV3.BestNet)
                    {
                        existingCIV3.BestNet = netValue;
                        existingCIV3.DrugId = drug.Id;
                        existingCIV3.ScriptCode = record.Script;
                        existingCIV3.ScriptDateTime = recordDate;
                    }
                }
                else
                {
                    var newCI = new ClassInsuranceV3
                    {
                        InsuranceId = insuranceItem.Id,
                        InsuranceName = insuranceItem.RxGroup,
                        DrugClassV3Id = drug.DrugClassV3Id,
                        DrugId = drug.Id,
                        BranchId = branch.Id,
                        Date = yearMonth,
                        ClassName = classItem.Name,
                        ScriptDateTime = yearMonth,
                        ScriptCode = record.Script,
                        BestNet = netValue
                    };
                    newClassInsuranceV3s.Add(newCI);
                    ci3Dict.Add(ciKey, newCI);
                }
            }

            // Now add only the new DrugInsurance and ClassInsurance records.
            _context.DrugInsurances.AddRange(newDrugInsurances);
            await _context.SaveChangesAsync();

            _context.ClassInsurances.AddRange(newClassInsurances);
            await _context.SaveChangesAsync();
            _context.ClassInsuranceV2s.AddRange(newClassInsuranceV2s);
            await _context.SaveChangesAsync();
            _context.ClassInsuranceV3s.AddRange(newClassInsuranceV3s);
            await _context.SaveChangesAsync();


            // ========================================================
            // PHASE 3: Process Users and Scripts
            // ========================================================
            // Preload Users, Branches, and Scripts.
            var userDict = await _context.Users
                .GroupBy(u => u.ShortName)
                .Select(g => g.First())
                .ToDictionaryAsync(u => u.ShortName);
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

                if (!insuranceRxDict.TryGetValue(record.RxGroup, out var insurance2))
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
                    if (script.Date <= recordDate)
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
        internal async Task<DrugBestAlternativeReadDto> GetBestAlternativeByNDCRxGroupId(int classId, int insuranceId)
        {
            var query = from di in _context.ClassInsurances
                        join irx in _context.InsuranceRxes on di.InsuranceId equals irx.Id
                        join ipcn in _context.InsurancePCNs on irx.InsurancePCNId equals ipcn.Id
                        join ins in _context.Insurances on ipcn.InsuranceId equals ins.Id
                        join dr in _context.Drugs on di.DrugId equals dr.Id
                        where di.ClassId == classId
                              && di.InsuranceId == insuranceId
                              && di.Date == _context.ClassInsurances
                                                .Where(x => x.ClassId == classId && x.InsuranceId == insuranceId)
                                                .Max(x => x.Date) // Get the newest date
                        select new
                        {
                            ClassId = dr.DrugClassId,
                            Date = di.Date,
                            BranchId = di.BranchId,
                            ClassName = di.ClassName,
                            DrugId = di.DrugId,
                            ScriptCode = di.ScriptCode,
                            ScriptDateTime = di.ScriptDateTime,
                            DrugName = dr.Name,
                            DrugClass = dr.DrugClass.Name,
                            BranchName = di.Branch.Name,
                            NDC = dr.NDC,
                            BinId = ipcn.InsuranceId,
                            PcnId = ipcn.Id,
                            RxGroupId = irx.Id,
                            DrugInsurance = di,
                            Bin = ins.Bin,
                            BinFullName = ins.Name,
                            RxGroup = irx.RxGroup,
                            PCN = ipcn.PCN,
                            Net = di.BestNet
                        };

            var result = await query.FirstOrDefaultAsync();
            if (result == null)
                return null;
            Console.WriteLine("result : ", result.Net);

            DrugBestAlternativeReadDto dto = new DrugBestAlternativeReadDto();
            dto.ClassId = result.ClassId;
            dto.Date = result.Date;
            dto.BranchId = result.BranchId;
            dto.ClassName = result.ClassName;
            dto.BestNet = result.Net;
            dto.DrugId = result.DrugId;
            dto.ScriptCode = result.ScriptCode;
            dto.ScriptDateTime = result.ScriptDateTime;
            dto.DrugName = result.DrugName;
            dto.DrugClass = result.DrugClass;
            dto.BranchName = result.BranchName;
            dto.NDC = result.NDC;
            dto.BinId = result.BinId;
            dto.PcnId = result.PcnId;
            dto.RxGroupId = result.RxGroupId;
            dto.BinFullName = result.BinFullName;
            dto.Bin = result.Bin;
            dto.Pcn = result.PCN;
            dto.Rxgroup = result.RxGroup;
            return dto;
        }

        internal async Task<DrugsAlternativesReadDto> GetDetails(string ndc, int insuranceId)
        {
            var query = from di in _context.DrugInsurances
                        join irx in _context.InsuranceRxes on di.InsuranceId equals irx.Id
                        join ipcn in _context.InsurancePCNs on irx.InsurancePCNId equals ipcn.Id
                        join ins in _context.Insurances on ipcn.InsuranceId equals ins.Id
                        where di.NDCCode == ndc
                              && di.InsuranceId == insuranceId
                              && di.date == _context.DrugInsurances
                                                .Where(x => x.NDCCode == ndc && x.InsuranceId == insuranceId)
                                                .Max(x => x.date) // Get the newest date
                        select new
                        {
                            DrugInsurance = di,
                            Bin = ins.Bin,
                            BinFullName = ins.Name,
                            RxGroup = irx.RxGroup,
                            PCN = ipcn.PCN,
                            pcnId = ipcn.Id,
                            rxgroupId = irx.Id,
                            binId = ins.Id,
                            DrugClassV2Id = di.Drug.DrugClassV2Id,
                            DrugClassV3Id = di.Drug.DrugClassV3Id,

                        };

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
                return null;

            var dto = _mapper.Map<DrugsAlternativesReadDto>(result.DrugInsurance);
            dto.bin = result.Bin;
            dto.BinFullName = result.BinFullName;
            dto.rxgroup = result.RxGroup;
            dto.pcn = result.PCN;
            dto.pcnId = result.pcnId;
            dto.rxgroupId = result.rxgroupId;
            dto.binId = result.binId;
            dto.DrugClassV2Id = result.DrugClassV2Id;
            dto.DrugClassV3Id = result.DrugClassV3Id;
            return dto;
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

        // internal async Task<ICollection<Drug>> GetAllDrugsV2(int classId)
        // {
        //     var items = await _context.Drugs
        //          .Where(x => x.DrugClassV2Id == classId)
        //          .GroupBy(x => x.NDC)
        //          .Select(g => g.First())
        //          .ToListAsync();


        //     return items;
        // }


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

            // Load InsuranceRx records including their related InsurancePCN and Insurance (for bin)
            var insuranceRxDict = await _context.InsuranceRxes
                                                .Include(ir => ir.InsurancePCN)
                                                    .ThenInclude(ipcn => ipcn.Insurance)
                                                .ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override properties from joined tables.
                    dto.DrugClass = item.DrugClass.Name;
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;

                    // Use the InsuranceRx dictionary to set insuranceName, pcn, bin, and rxgroup.
                    if (insuranceRxDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insuranceRx))
                    {
                        dto.insuranceName = insuranceRx.RxGroup;
                        dto.pcn = insuranceRx.InsurancePCN?.PCN;
                        dto.bin = insuranceRx.InsurancePCN?.Insurance?.Bin;
                        dto.rxgroup = insuranceRx.RxGroup; // Adjust if rxgroup should be different.
                        dto.BinFullName = insuranceRx.InsurancePCN?.Insurance?.Name;
                        dto.binId = insuranceRx.InsurancePCN.Insurance.Id;
                        dto.pcnId = insuranceRx.InsurancePCN.Id;
                        dto.rxgroupId = insuranceRx.Id;
                        dto.ApplicationNumber = item.Drug.ApplicationNumber;
                        dto.ApplicationType = item.Drug.ApplicationType;
                        dto.Route = item.Drug.Route;
                        dto.Strength = item.Drug.Strength;
                        dto.Form = item.Drug.Form;
                        dto.Ingrdient = item.Drug.Ingrdient;
                        dto.StrengthUnit = item.Drug.StrengthUnit;
                        dto.Type = item.Drug.Type;
                        dto.TECode = item.Drug.TECode;
                    }
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
                        Quantity = "",
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
                    dto.bin = null;
                    dto.pcn = null;
                    dto.rxgroup = null;
                    dto.branchName = null;
                    dto.ApplicationNumber = item.Drug.ApplicationNumber;
                    dto.ApplicationType = item.Drug.ApplicationType;
                    dto.Route = item.Drug.Route;
                    dto.Strength = item.Drug.Strength;
                    dto.Form = item.Drug.Form;
                    dto.Ingrdient = item.Drug.Ingrdient;
                    dto.StrengthUnit = item.Drug.StrengthUnit;
                    dto.Type = item.Drug.Type;
                    dto.TECode = item.Drug.TECode;
                    return dto;
                }
            }).ToList();

            return result;
        }


        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV3(int classId)
        {
            var query = from d in _context.Drugs.Where(d => d.DrugClassV3Id == classId)
                        join dc in _context.DrugClassV3s on d.DrugClassV3Id equals dc.Id
                        join di in _context.DrugInsurances.Where(di => di.DrugClassV3Id == classId)
                            on d.Id equals di.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty()
                        select new { Drug = d, DrugClass = dc, DrugInsurance = di };

            var list = await query.ToListAsync();
            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);

            // Load InsuranceRx records including their related InsurancePCN and Insurance (for bin)
            var insuranceRxDict = await _context.InsuranceRxes
                                                .Include(ir => ir.InsurancePCN)
                                                    .ThenInclude(ipcn => ipcn.Insurance)
                                                .ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override properties from joined tables.
                    dto.DrugClass = item.DrugClass.Name;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClassV2Id = item.Drug.DrugClassV2Id;
                    dto.DrugClassV3Id = item.Drug.DrugClassV3Id;
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;

                    // Use the InsuranceRx dictionary to set insuranceName, pcn, bin, and rxgroup.
                    if (insuranceRxDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insuranceRx))
                    {
                        dto.insuranceName = insuranceRx.RxGroup;
                        dto.pcn = insuranceRx.InsurancePCN?.PCN;
                        dto.bin = insuranceRx.InsurancePCN?.Insurance?.Bin;
                        dto.rxgroup = insuranceRx.RxGroup; // Adjust if rxgroup should be different.
                        dto.BinFullName = insuranceRx.InsurancePCN?.Insurance?.Name;
                        dto.binId = insuranceRx.InsurancePCN.Insurance.Id;
                        dto.pcnId = insuranceRx.InsurancePCN.Id;
                        dto.rxgroupId = insuranceRx.Id;
                        dto.ApplicationNumber = item.Drug.ApplicationNumber;
                        dto.ApplicationType = item.Drug.ApplicationType;
                        dto.Route = item.Drug.Route;
                        dto.Strength = item.Drug.Strength;
                        dto.Form = item.Drug.Form;
                        dto.Ingrdient = item.Drug.Ingrdient;
                        dto.StrengthUnit = item.Drug.StrengthUnit;
                        dto.Type = item.Drug.Type;
                        dto.TECode = item.Drug.TECode;
                    }
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
                        DrugClassV2Id = item.Drug.DrugClassV2Id,
                        DrugClassV3Id = item.Drug.DrugClassV3Id,
                        Net = 0,
                        date = DateTime.UtcNow,
                        Prescriber = null,
                        Quantity = "",
                        AcquisitionCost = 0,
                        Discount = 0,
                        InsurancePayment = 0,
                        PatientPayment = 0,
                        BranchId = 1,
                        InsuranceId = 0,
                        Drug = item.Drug,

                    };

                    var dto = _mapper.Map<DrugsAlternativesReadDto>(defaultInsurance);
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClassV2Id = item.Drug.DrugClassV2Id;
                    dto.DrugClassV3Id = item.Drug.DrugClassV3Id;
                    dto.DrugClass = item.DrugClass.Name;//for v3 class 
                    dto.insuranceName = null;
                    dto.bin = null;
                    dto.pcn = null;
                    dto.rxgroup = null;
                    dto.branchName = null;
                    dto.ApplicationNumber = item.Drug.ApplicationNumber;
                    dto.ApplicationType = item.Drug.ApplicationType;
                    dto.Route = item.Drug.Route;
                    dto.Strength = item.Drug.Strength;
                    dto.Form = item.Drug.Form;
                    dto.Ingrdient = item.Drug.Ingrdient;
                    dto.StrengthUnit = item.Drug.StrengthUnit;
                    dto.Type = item.Drug.Type;
                    dto.TECode = item.Drug.TECode;
                    return dto;
                }
            }).ToList();

            return result;
        }

        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV2(int classId)
        {
            var query = from d in _context.Drugs.Where(d => d.DrugClassV2Id == classId)
                        join dc in _context.DrugClassV2s on d.DrugClassV2Id equals dc.Id
                        join di in _context.DrugInsurances.Where(di => di.DrugClassV2Id == classId)
                            on d.Id equals di.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty()
                        select new { Drug = d, DrugClass = dc, DrugInsurance = di };

            var list = await query.ToListAsync();
            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);

            // Load InsuranceRx records including their related InsurancePCN and Insurance (for bin)
            var insuranceRxDict = await _context.InsuranceRxes
                                                .Include(ir => ir.InsurancePCN)
                                                    .ThenInclude(ipcn => ipcn.Insurance)
                                                .ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override properties from joined tables.
                    dto.DrugClass = item.DrugClass.Name;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClassV2Id = item.Drug.DrugClassV2Id;
                    dto.DrugClassV3Id = item.Drug.DrugClassV3Id;
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;

                    // Use the InsuranceRx dictionary to set insuranceName, pcn, bin, and rxgroup.
                    if (insuranceRxDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insuranceRx))
                    {
                        dto.insuranceName = insuranceRx.RxGroup;
                        dto.pcn = insuranceRx.InsurancePCN?.PCN;
                        dto.bin = insuranceRx.InsurancePCN?.Insurance?.Bin;
                        dto.rxgroup = insuranceRx.RxGroup; // Adjust if rxgroup should be different.
                        dto.BinFullName = insuranceRx.InsurancePCN?.Insurance?.Name;
                        dto.binId = insuranceRx.InsurancePCN.Insurance.Id;
                        dto.pcnId = insuranceRx.InsurancePCN.Id;
                        dto.rxgroupId = insuranceRx.Id;
                        dto.ApplicationNumber = item.Drug.ApplicationNumber;
                        dto.ApplicationType = item.Drug.ApplicationType;
                        dto.Route = item.Drug.Route;
                        dto.Strength = item.Drug.Strength;
                        dto.Form = item.Drug.Form;
                        dto.Ingrdient = item.Drug.Ingrdient;
                        dto.StrengthUnit = item.Drug.StrengthUnit;
                        dto.Type = item.Drug.Type;
                        dto.TECode = item.Drug.TECode;
                    }
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
                        DrugClassV2Id = item.Drug.DrugClassV2Id,
                        DrugClassV3Id = item.Drug.DrugClassV3Id,
                        Net = 0,
                        date = DateTime.UtcNow,
                        Prescriber = null,
                        Quantity = "",
                        AcquisitionCost = 0,
                        Discount = 0,
                        InsurancePayment = 0,
                        PatientPayment = 0,
                        BranchId = 1,
                        InsuranceId = 0,
                        Drug = item.Drug,

                    };

                    var dto = _mapper.Map<DrugsAlternativesReadDto>(defaultInsurance);
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;
                    dto.DrugClassId = item.Drug.DrugClassId;
                    dto.DrugClassV2Id = item.Drug.DrugClassV2Id;
                    dto.DrugClassV3Id = item.Drug.DrugClassV3Id;
                    dto.DrugClass = item.DrugClass.Name;//for v3 class 
                    dto.insuranceName = null;
                    dto.bin = null;
                    dto.pcn = null;
                    dto.rxgroup = null;
                    dto.branchName = null;
                    dto.ApplicationNumber = item.Drug.ApplicationNumber;
                    dto.ApplicationType = item.Drug.ApplicationType;
                    dto.Route = item.Drug.Route;
                    dto.Strength = item.Drug.Strength;
                    dto.Form = item.Drug.Form;
                    dto.Ingrdient = item.Drug.Ingrdient;
                    dto.StrengthUnit = item.Drug.StrengthUnit;
                    dto.Type = item.Drug.Type;
                    dto.TECode = item.Drug.TECode;
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
            [Name("RxGroup")]
            public string RxGroup { get; set; }
            [Name("Bin")]
            public string Bin { get; set; }
            [Name("PCN")]
            public string PCN { get; set; }

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
            public string Quantity { get; set; }

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
            public string? Name { get; set; }

            [Name("ndc")]
            public string? NDC { get; set; }

            [Name("form")]
            public string? Form { get; set; }

            [Name("strength")]
            public string? Strength { get; set; }

            [Name("acq")]
            public decimal? ACQ { get; set; }

            [Name("awp")]
            public decimal? AWP { get; set; }

            [Name("rxCUI")]
            [Default(0)]
            public decimal? Rxcui { get; set; }

            [Name("drug_class")]
            public string? DrugClass { get; set; }
            [Name("route")]
            public string? Route { get; set; }
            [Name("TE_Code")]
            public string? TECode { get; set; }
            [Name("ingredient")]
            public string? Ingrdient { get; set; }
            [Name("Appl_No")]
            public string? ApplicationNumber { get; set; }
            [Name("Appl_Type")]
            public string? ApplicationType { get; set; }
            [Name("Adjusted_Group_ID")]
            public string? ClassV2 { get; set; }
            [Name("Unit")]
            public string? Unit { get; set; }
            [Name("Type")]
            public string? Type { get; set; }
            [Name("Group_By_Class_Standardized")]
            public string? ClassV3 { get; set; }
        }
        internal async Task<ICollection<AuditReadDto>> GetAllLatestScripts()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id
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
                    InsuranceId = insurance.Id,
                    // Best Drug
                    HighstDrugName = bestDrug.Name,
                    HighstDrugNDC = bestDrug.NDC,
                    HighstDrugId = bestDrug.Id,
                    BranchCode = branch.Name,
                    Insurance = insurance.RxGroup,
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





        public async Task<ICollection<AuditReadDto>> GetAllLatestScriptsPaginated(int pageNumber, int pageSize, int classVersion = 1)
        {
            if (classVersion == 1)
            {
                // Use a constant cache key for the entire dataset.
                const string cacheKey = "AllLatestScripts";
                List<AuditReadDto> allData;

                // Attempt to get the dataset from cache.
                if (!_cache.TryGetValue(cacheKey, out allData))
                {
                    // Cache miss: load the entire dataset from the database.
                    allData = await LoadAllLatestScriptsFromDatabaseAsync();

                    // Set up cache options. Adjust the expiration as necessary.
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(120));

                    // Store the full dataset in cache.
                    _cache.Set(cacheKey, allData, cacheOptions);
                }
                // Paginate the data from the cache.
                var pagedData = allData
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return pagedData;
            }
            else if (classVersion == 2)
            {
                const string cacheKey = "AllLatestScriptsV2";
                List<AuditReadDto> allData;

                // Attempt to get the dataset from cache.
                if (!_cache.TryGetValue(cacheKey, out allData))
                {
                    // Cache miss: load the entire dataset from the database.
                    allData = await LoadAllLatestScriptsV2FromDatabaseAsync();

                    // Set up cache options. Adjust the expiration as necessary.
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(120));

                    // Store the full dataset in cache.
                    _cache.Set(cacheKey, allData, cacheOptions);
                }
                // Paginate the data from the cache.
                var pagedData = allData
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return pagedData;
            }
            else
            {
                const string cacheKey = "AllLatestScriptsV3";
                List<AuditReadDto> allData;

                // Attempt to get the dataset from cache.
                if (!_cache.TryGetValue(cacheKey, out allData))
                {
                    // Cache miss: load the entire dataset from the database.
                    allData = await LoadAllLatestScriptsV3FromDatabaseAsync();

                    // Set up cache options. Adjust the expiration as necessary.
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(120));

                    // Store the full dataset in cache.
                    _cache.Set(cacheKey, allData, cacheOptions);
                }
                // Paginate the data from the cache.
                var pagedData = allData
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return pagedData;
            }
        }
        public async Task<ICollection<AuditReadDto>> GetLatestScriptsByMonthYear(int month, int year)
        {
            const string cacheKey = "AllLatestScripts";
            List<AuditReadDto> allData;

            // Try to retrieve the full dataset from cache
            if (!_cache.TryGetValue(cacheKey, out allData))
            {
                // Cache miss: Load the full dataset from the database
                allData = await LoadAllLatestScriptsFromDatabaseAsync();

                // Configure cache options (adjust expiration as necessary)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(120));

                // Store the full dataset in cache
                _cache.Set(cacheKey, allData, cacheOptions);
            }

            // Filter the cached data for the specified month and year
            var filteredData = allData
                .Where(x => x.Date.Month == month && x.Date.Year == year)
                .ToList();

            return filteredData;
        }

        private async Task<List<AuditReadDto>> LoadAllLatestScriptsFromDatabaseAsync()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id into insuranceGroup
                from insurance in insuranceGroup.DefaultIfEmpty()
                join classItem in _context.DrugClasses on scriptItem.DrugClassId equals classItem.Id into classGroup
                from classItem in classGroup.DefaultIfEmpty()
                join branch in _context.Branches on script.BranchId equals branch.Id into branchGroup
                from branch in branchGroup.DefaultIfEmpty()
                join classInsurance in _context.ClassInsurances
                    on new { InsuranceId = insurance != null ? (int?)insurance.Id : null, ClassId = classItem != null ? (int?)classItem.Id : null, Year = script.Date.Year, Month = script.Date.Month, BranchId = branch != null ? (int?)branch.Id : null }
                    equals new { InsuranceId = (int?)classInsurance.InsuranceId, ClassId = (int?)classInsurance.ClassId, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month, BranchId = (int?)classInsurance.BranchId }
                    into classInsuranceGroup
                from classInsurance in classInsuranceGroup.DefaultIfEmpty()
                join scriptDrug in _context.Drugs on scriptItem.DrugId equals scriptDrug.Id into scriptDrugGroup
                from scriptDrug in scriptDrugGroup.DefaultIfEmpty()
                join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id into bestDrugGroup
                from bestDrug in bestDrugGroup.DefaultIfEmpty()
                join user in _context.Users on script.UserId equals user.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
                from prescriber in prescriberGroup.DefaultIfEmpty()
                let prevMonth = script.Date.AddMonths(-1)
                let bestNetEntryPrevMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.ClassId == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == prevMonth.Year && ci.Date.Month == prevMonth.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntryCurrentMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.ClassId == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == script.Date.Year && ci.Date.Month == script.Date.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth
                select new AuditReadDto
                {
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = scriptItem.RxNumber,
                    User = user != null ? user.Name : "Unknown",
                    DrugName = scriptDrug != null ? scriptDrug.Name : null,
                    NDCCode = scriptDrug != null ? scriptDrug.NDC : null,
                    DrugId = scriptDrug != null ? scriptDrug.Id : 0,
                    HighstDrugName = bestDrug != null ? bestDrug.Name : null,
                    HighstDrugNDC = bestDrug != null ? bestDrug.NDC : null,
                    HighstDrugId = bestDrug != null ? bestDrug.Id : 0,
                    BranchCode = branch != null ? branch.Name : null,
                    Insurance = insurance != null ? insurance.RxGroup : null,
                    InsuranceId = insurance != null ? insurance.Id : 0,
                    PF = scriptItem.PF,
                    Prescriber = prescriber != null ? prescriber.Name : "Unknown",
                    Quantity = scriptItem.Quantity,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    NetProfit = scriptItem.NetProfit,
                    DrugClass = classItem != null ? classItem.Name : null,
                    HighstNet = bestNetEntry != null ? bestNetEntry.BestNet : 0,
                    HighstScriptCode = bestNetEntry != null ? bestNetEntry.ScriptCode : null,
                    HighstScriptDate = bestNetEntry != null ? bestNetEntry.ScriptDateTime : DateTime.MinValue
                }).ToListAsync();

            return auditData;
        }

        private async Task<List<AuditReadDto>> LoadAllLatestScriptsV2FromDatabaseAsync()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id into insuranceGroup
                from insurance in insuranceGroup.DefaultIfEmpty()
                join classItem in _context.DrugClassV2s on scriptItem.Drug.DrugClassV2Id equals classItem.Id into classGroup
                from classItem in classGroup.DefaultIfEmpty()
                join branch in _context.Branches on script.BranchId equals branch.Id into branchGroup
                from branch in branchGroup.DefaultIfEmpty()
                join classInsurance in _context.ClassInsuranceV2s
                    on new { InsuranceId = insurance != null ? (int?)insurance.Id : null, DrugClassV2Id = classItem != null ? (int?)classItem.Id : null, Year = script.Date.Year, Month = script.Date.Month, BranchId = branch != null ? (int?)branch.Id : null }
                    equals new { InsuranceId = (int?)classInsurance.InsuranceId, DrugClassV2Id = (int?)classInsurance.DrugClassV2Id, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month, BranchId = (int?)classInsurance.BranchId }
                    into classInsuranceGroup
                from classInsurance in classInsuranceGroup.DefaultIfEmpty()
                join scriptDrug in _context.Drugs on scriptItem.DrugId equals scriptDrug.Id into scriptDrugGroup
                from scriptDrug in scriptDrugGroup.DefaultIfEmpty()
                join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id into bestDrugGroup
                from bestDrug in bestDrugGroup.DefaultIfEmpty()
                join user in _context.Users on script.UserId equals user.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
                from prescriber in prescriberGroup.DefaultIfEmpty()
                let prevMonth = script.Date.AddMonths(-1)
                let bestNetEntryPrevMonth = _context.ClassInsuranceV2s
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.DrugClassV2Id == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == prevMonth.Year && ci.Date.Month == prevMonth.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntryCurrentMonth = _context.ClassInsuranceV2s
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.DrugClassV2Id == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == script.Date.Year && ci.Date.Month == script.Date.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth
                select new AuditReadDto
                {
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = scriptItem.RxNumber,
                    User = user != null ? user.Name : "Unknown",
                    DrugName = scriptDrug != null ? scriptDrug.Name : null,
                    NDCCode = scriptDrug != null ? scriptDrug.NDC : null,
                    DrugId = scriptDrug != null ? scriptDrug.Id : 0,
                    HighstDrugName = bestDrug != null ? bestDrug.Name : null,
                    HighstDrugNDC = bestDrug != null ? bestDrug.NDC : null,
                    HighstDrugId = bestDrug != null ? bestDrug.Id : 0,
                    BranchCode = branch != null ? branch.Name : null,
                    Insurance = insurance != null ? insurance.RxGroup : null,
                    InsuranceId = insurance != null ? insurance.Id : 0,
                    PF = scriptItem.PF,
                    Prescriber = prescriber != null ? prescriber.Name : "Unknown",
                    Quantity = scriptItem.Quantity,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    NetProfit = scriptItem.NetProfit,
                    DrugClass = classItem != null ? classItem.Name : null,
                    HighstNet = bestNetEntry != null ? bestNetEntry.BestNet : 0,
                    HighstScriptCode = bestNetEntry != null ? bestNetEntry.ScriptCode : null,
                    HighstScriptDate = bestNetEntry != null ? bestNetEntry.ScriptDateTime : DateTime.MinValue
                }).ToListAsync();

            return auditData;
        }
        private async Task<List<AuditReadDto>> LoadAllLatestScriptsV3FromDatabaseAsync()
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id into insuranceGroup
                from insurance in insuranceGroup.DefaultIfEmpty()
                join classItem in _context.DrugClassV3s on scriptItem.Drug.DrugClassV3Id equals classItem.Id into classGroup
                from classItem in classGroup.DefaultIfEmpty()
                join branch in _context.Branches on script.BranchId equals branch.Id into branchGroup
                from branch in branchGroup.DefaultIfEmpty()
                join classInsurance in _context.ClassInsuranceV3s
                    on new { InsuranceId = insurance != null ? (int?)insurance.Id : null, DrugClassV3Id = classItem != null ? (int?)classItem.Id : null, Year = script.Date.Year, Month = script.Date.Month, BranchId = branch != null ? (int?)branch.Id : null }
                    equals new { InsuranceId = (int?)classInsurance.InsuranceId, DrugClassV3Id = (int?)classInsurance.DrugClassV3Id, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month, BranchId = (int?)classInsurance.BranchId }
                    into classInsuranceGroup
                from classInsurance in classInsuranceGroup.DefaultIfEmpty()
                join scriptDrug in _context.Drugs on scriptItem.DrugId equals scriptDrug.Id into scriptDrugGroup
                from scriptDrug in scriptDrugGroup.DefaultIfEmpty()
                join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id into bestDrugGroup
                from bestDrug in bestDrugGroup.DefaultIfEmpty()
                join user in _context.Users on script.UserId equals user.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
                from prescriber in prescriberGroup.DefaultIfEmpty()
                let prevMonth = script.Date.AddMonths(-1)
                let bestNetEntryPrevMonth = _context.ClassInsuranceV3s
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.DrugClassV3Id == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == prevMonth.Year && ci.Date.Month == prevMonth.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntryCurrentMonth = _context.ClassInsuranceV3s
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) && ci.DrugClassV3Id == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == script.Date.Year && ci.Date.Month == script.Date.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth
                select new AuditReadDto
                {
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = scriptItem.RxNumber,
                    User = user != null ? user.Name : "Unknown",
                    DrugName = scriptDrug != null ? scriptDrug.Name : null,
                    NDCCode = scriptDrug != null ? scriptDrug.NDC : null,
                    DrugId = scriptDrug != null ? scriptDrug.Id : 0,
                    HighstDrugName = bestDrug != null ? bestDrug.Name : null,
                    HighstDrugNDC = bestDrug != null ? bestDrug.NDC : null,
                    HighstDrugId = bestDrug != null ? bestDrug.Id : 0,
                    BranchCode = branch != null ? branch.Name : null,
                    Insurance = insurance != null ? insurance.RxGroup : null,
                    InsuranceId = insurance != null ? insurance.Id : 0,
                    PF = scriptItem.PF,
                    Prescriber = prescriber != null ? prescriber.Name : "Unknown",
                    Quantity = scriptItem.Quantity,
                    AcquisitionCost = scriptItem.AcquisitionCost,
                    Discount = scriptItem.Discount,
                    InsurancePayment = scriptItem.InsurancePayment,
                    PatientPayment = scriptItem.PatientPayment,
                    NetProfit = scriptItem.NetProfit,
                    DrugClass = classItem != null ? classItem.Name : null,
                    HighstNet = bestNetEntry != null ? bestNetEntry.BestNet : 0,
                    HighstScriptCode = bestNetEntry != null ? bestNetEntry.ScriptCode : null,
                    HighstScriptDate = bestNetEntry != null ? bestNetEntry.ScriptDateTime : DateTime.MinValue
                }).ToListAsync();

            return auditData;
        }









        // internal async Task<ICollection<AuditReadDto>> GetAllLatestScriptsPaginated(int page = 1, int pageSize = 1000)
        // {
        //     int skipCount = (page - 1) * pageSize;

        //     var auditData = await (
        //         from script in _context.Scripts
        //         join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
        //         join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id
        //         join classItem in _context.DrugClasses on scriptItem.DrugClassId equals classItem.Id
        //         join branch in _context.Branches on script.BranchId equals branch.Id
        //         join classInsurance in _context.ClassInsurances
        //             on new { InsuranceId = insurance.Id, ClassId = classItem.Id, Year = script.Date.Year, Month = script.Date.Month, BranchId = branch.Id }
        //             equals new { classInsurance.InsuranceId, classInsurance.ClassId, Year = classInsurance.Date.Year, Month = classInsurance.Date.Month, classInsurance.BranchId }
        //         join scriptDrug in _context.Drugs on scriptItem.DrugId equals scriptDrug.Id // Drug from script
        //         join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id // Best drug
        //         join user in _context.Users on script.UserId equals user.Id into userGroup
        //         from user in userGroup.DefaultIfEmpty() // Allow nullable users
        //         join prescriber in _context.Users on scriptItem.PrescriberId equals prescriber.Id into prescriberGroup
        //         from prescriber in prescriberGroup.DefaultIfEmpty() // Allow nullable prescribers
        //         let prevMonth = script.Date.AddMonths(-1)
        //         let bestNetEntryPrevMonth = _context.ClassInsurances
        //             .Where(ci => ci.InsuranceId == insurance.Id && ci.ClassId == classItem.Id &&
        //                          ci.Date.Year == prevMonth.Year && ci.Date.Month == prevMonth.Month)
        //             .OrderByDescending(ci => ci.BestNet)
        //             .FirstOrDefault()
        //         let bestNetEntryCurrentMonth = _context.ClassInsurances
        //             .Where(ci => ci.InsuranceId == insurance.Id && ci.ClassId == classItem.Id &&
        //                          ci.Date.Year == script.Date.Year && ci.Date.Month == script.Date.Month)
        //             .OrderByDescending(ci => ci.BestNet)
        //             .FirstOrDefault()
        //         let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth // Use previous month if available, otherwise fallback to current month
        //         select new AuditReadDto
        //         {
        //             Date = script.Date,
        //             ScriptCode = script.ScriptCode,
        //             RxNumber = scriptItem.RxNumber,
        //             User = user != null ? user.Name : "Unknown",

        //             // Drug from script
        //             DrugName = scriptDrug.Name,
        //             NDCCode = scriptDrug.NDC,
        //             DrugId = scriptDrug.Id,

        //             // Best Drug
        //             HighstDrugName = bestDrug.Name,
        //             HighstDrugNDC = bestDrug.NDC,
        //             HighstDrugId = bestDrug.Id,
        //             BranchCode = branch.Name,
        //             Insurance = insurance.RxGroup,
        //             PF = scriptItem.PF,
        //             Prescriber = prescriber != null ? prescriber.Name : "Unknown",
        //             Quantity = scriptItem.Quantity,
        //             AcquisitionCost = scriptItem.AcquisitionCost,
        //             Discount = scriptItem.Discount,
        //             InsurancePayment = scriptItem.InsurancePayment,
        //             PatientPayment = scriptItem.PatientPayment,
        //             NetProfit = scriptItem.NetProfit,
        //             DrugClass = classItem.Name,
        //             HighstNet = bestNetEntry.BestNet,
        //             HighstScriptCode = bestNetEntry.ScriptCode,
        //             HighstScriptDate = bestNetEntry.ScriptDateTime
        //         }
        //     ).Skip(skipCount).Take(pageSize).ToListAsync();

        //     return auditData;
        // }



        internal async Task<ICollection<DrugInsuranceReadDto>> GetInsuranceByNdc(string ndc)
        {
            var result = await (
                from di in _context.DrugInsurances
                join ins in _context.InsuranceRxes on di.InsuranceId equals ins.Id into insuranceJoin
                from ins in insuranceJoin.DefaultIfEmpty() // Left Join to handle nulls
                join drg in _context.Drugs on di.DrugId equals drg.Id into drugJoin
                from drg in drugJoin.DefaultIfEmpty()
                join br in _context.Branches on di.BranchId equals br.Id into branchJoin
                from br in branchJoin.DefaultIfEmpty()
                where di.NDCCode == ndc
                select new DrugInsuranceReadDto
                {
                    InsuranceId = ins.Id,
                    DrugId = drg.Id,
                    BranchId = br.Id,
                    NDCCode = di.NDCCode,
                    DrugClass = di.DrugClassId.ToString(), // Assuming DrugClassId should be kept as ID
                    Net = di.Net,
                    date = di.date,
                    Prescriber = di.Prescriber,
                    Quantity = di.Quantity,
                    AcquisitionCost = di.AcquisitionCost,
                    Discount = di.Discount,
                    InsurancePayment = di.InsurancePayment,
                    PatientPayment = di.PatientPayment,
                    Insurance = ins != null ? ins.RxGroup : null,
                    Drug = drg != null ? drg.Name : null,
                    Branch = br != null ? br.Name : null,
                    Id = di.Id
                }
            ).ToListAsync();

            return result;
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
            List<InsuranceCsvRecord> csvRecords;

            // Read the CSV file
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<InsuranceCsvMap>(); // Register the corrected mapping
                csvRecords = csv.GetRecords<InsuranceCsvRecord>().ToList();
            }

            // Fetch all existing insurances from DB and index them by BIN
            var insuranceDic = (await _context.Insurances.ToListAsync())
                .ToDictionary(x => x.Bin, StringComparer.OrdinalIgnoreCase);

            foreach (var csvRecord in csvRecords)
            {
                // Ensure Full Name is at least 6 characters long by padding with leading zeros
                if (csvRecord.Bin.Length < 6)
                {
                    csvRecord.Bin = csvRecord.Bin.PadLeft(6, '0');
                }

                // Skip if BIN is empty or invalid
                if (string.IsNullOrWhiteSpace(csvRecord.Bin))
                {
                    continue;
                }

                // Search in the database by BIN and update Full Name
                if (insuranceDic.TryGetValue(csvRecord.Bin, out var existingInsurance))
                {
                    existingInsurance.Name = csvRecord.FullName;
                }
            }

            await _context.SaveChangesAsync();
        }

        // CSV model with correct header mapping
        public class InsuranceCsvRecord
        {
            public string Bin { get; set; }
            public string FullName { get; set; }
        }

        // Mapping class for CsvHelper
        public sealed class InsuranceCsvMap : ClassMap<InsuranceCsvRecord>
        {
            public InsuranceCsvMap()
            {
                Map(m => m.Bin).Name("BIN");
                Map(m => m.FullName).Name("Full Name");
            }
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
            var insruaceDict = await _context.InsuranceRxes.ToDictionaryAsync(x => x.Id);
            var insruacePCN = await _context.InsurancePCNs.FirstOrDefaultAsync(x => x.Id == insruaceDict.First().Value.InsurancePCNId);
            var insruaceBin = await _context.Insurances.FirstOrDefaultAsync(x => x.Id == insruacePCN.InsuranceId);
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
                    {
                        dto.insuranceName = insruace.RxGroup;

                        dto.rxgroup = insruace.RxGroup;
                        dto.InsuranceId = insruace.Id;
                        dto.pcn = insruacePCN.PCN;
                        dto.bin = insruaceBin.Bin;
                        dto.BinFullName = insruaceBin.Name;

                    }
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
                        Quantity = "",
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

        internal async Task<ICollection<Drug>> GetDrugsByInsurance(int insuranceId, string drug)
        {
            var drugs = await (from d in _context.Drugs
                               join di in _context.DrugInsurances on d.Id equals di.DrugId
                               where di.InsuranceId == insuranceId &&
                                     d.Name.ToLower().Contains(drug.ToLower())
                               select d)
                              .Distinct()
                              .ToListAsync();

            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugsByInsurance(string insurance)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug)
                .Include(di => di.Insurance)
                .Where(di => di.Insurance != null && di.Insurance.RxGroup.ToLower() == insurance.ToLower())
                .Select(di => di.Drug)
                .Distinct()
                .ToListAsync();

            return drugs;
        }
        // ...existing code...
        internal async Task<ICollection<Drug>> GetDrugsByInsuranceNameDrugName(string insurance, string drugName, int pageSize = 1000, int pageNumber = 1)
        {
            Console.WriteLine($"Searching for drugs with insurance: {insurance} and drug name: {drugName} on page {pageNumber} with page size {pageSize}");
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug)
                .Include(di => di.Insurance)
                .Where(di => di.Insurance != null && di.Insurance.RxGroup.ToLower() == insurance.ToLower() &&
                             di.Drug.Name.ToLower().Contains(drugName.ToLower()))
                .Select(di => di.Drug)
                .Distinct()
                .OrderBy(d => d.Id) // Ensure deterministic ordering for paging
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugs;
        }
        // ...existing code...

        internal async Task<ICollection<Drug>> GetDrugsByPCN(string pcn)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug) // Load Drug
                .Include(di => di.Insurance) // Load InsuranceRx
                    .ThenInclude(ir => ir.InsurancePCN) // Load InsurancePCN for access to PCN
                .Where(di => di.Insurance != null
                             && di.Insurance.InsurancePCN != null
                             && di.Insurance.InsurancePCN.PCN.ToLower() == pcn.ToLower())
                .Select(di => di.Drug)
                .Distinct() // Avoid duplicate drugs if multiple records exist
                .ToListAsync();

            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugsByBIN(string bin)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug) // Load Drug
                .Include(di => di.Insurance) // Load InsuranceRx
                    .ThenInclude(ir => ir.InsurancePCN) // Load InsurancePCN for access to PCN
                .Where(di => di.Insurance != null
                             && di.Insurance.InsurancePCN.Insurance.Bin != null
                             && di.Insurance.InsurancePCN.Insurance.Bin.ToLower() == bin.ToLower())
                .Select(di => di.Drug)
                .Distinct() // Avoid duplicate drugs if multiple records exist
                .ToListAsync();

            return drugs;
        }

        internal async Task<ICollection<Insurance>> GetInsurances(string insurance)
        {
            var items = await _context.Insurances.Where(x => x.Name.ToLower().Contains(insurance.ToLower())).ToListAsync();
            return items;
        }

        internal async Task<ICollection<Insurance>> GetInsurancesBinsByName(string bin)
        {
            var items = await _context.Insurances
                .Where(x => x.Bin.ToLower().Contains(bin.ToLower()) || x.Name.ToLower().Contains(bin.ToLower()))
                .ToListAsync();
            return items;
        }

        internal async Task<ICollection<InsurancePCN>> GetInsurancesPcnByBinId(int binId)
        {
            var items = await _context.InsurancePCNs.Where(x => x.InsuranceId == binId).ToListAsync();
            return items;
        }
        internal async Task<ICollection<InsuranceRx>> GetInsurancesRxByPcnId(int pcnId)
        {
            var items = await _context.InsuranceRxes.Where(x => x.InsurancePCNId == pcnId).ToListAsync();
            return items;
        }

        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV2Insu(int classId)
        {
            var query = from d in _context.Drugs.Where(d => d.DrugClassV2Id == classId)
                        join dc in _context.DrugClasses on d.DrugClassV2Id equals dc.Id
                        join di in _context.DrugInsurances.Where(di => di.DrugClassId == classId)
                            on d.Id equals di.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty()
                        select new { Drug = d, DrugClass = dc, DrugInsurance = di };

            var list = await query.ToListAsync();
            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);

            // Load InsuranceRx records including their related InsurancePCN and Insurance (for bin)
            var insuranceRxDict = await _context.InsuranceRxes
                                                .Include(ir => ir.InsurancePCN)
                                                    .ThenInclude(ipcn => ipcn.Insurance)
                                                .ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                if (item.DrugInsurance != null)
                {
                    var dto = _mapper.Map<DrugsAlternativesReadDto>(item.DrugInsurance);
                    // Override properties from joined tables.
                    dto.DrugClass = item.DrugClass.Name;
                    dto.DrugName = item.Drug.Name;
                    dto.NDCCode = item.Drug.NDC;

                    // Use the InsuranceRx dictionary to set insuranceName, pcn, bin, and rxgroup.
                    if (insuranceRxDict.TryGetValue(item.DrugInsurance.InsuranceId, out var insuranceRx))
                    {
                        dto.insuranceName = insuranceRx.RxGroup;
                        dto.pcn = insuranceRx.InsurancePCN?.PCN;
                        dto.bin = insuranceRx.InsurancePCN?.Insurance?.Bin;
                        dto.rxgroup = insuranceRx.RxGroup; // Adjust if rxgroup should be different.
                        dto.BinFullName = insuranceRx.InsurancePCN?.Insurance?.Name;
                        dto.binId = insuranceRx.InsurancePCN.Insurance.Id;
                        dto.pcnId = insuranceRx.InsurancePCN.Id;
                        dto.rxgroupId = insuranceRx.Id;
                    }
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
                        Quantity = "",
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
                    dto.bin = null;
                    dto.pcn = null;
                    dto.rxgroup = null;
                    dto.branchName = null;
                    dto.ApplicationNumber = item.Drug.ApplicationNumber;
                    dto.ApplicationType = item.Drug.ApplicationType;
                    dto.Route = item.Drug.Route;
                    dto.Strength = item.Drug.Strength;
                    dto.Form = item.Drug.Form;
                    dto.Ingrdient = item.Drug.Ingrdient;
                    dto.StrengthUnit = item.Drug.StrengthUnit;
                    dto.Type = item.Drug.Type;
                    dto.TECode = item.Drug.TECode;
                    return dto;
                }
            }).ToList();

            return result;
        }

        internal async Task<ICollection<DrugMediReadDto>> GetAllMediDrugs(int classId)
        {
            //make it pagintions 
            var items = await (
                from drugmedi in _context.DrugMedis
                join drug in _context.Drugs on drugmedi.DrugId equals drug.Id
                where drug.DrugClassId == classId
                select new { drugmedi, drug }
            ).ToListAsync();
            var result = items.Select(item =>
            {
                var dto = _mapper.Map<DrugMediReadDto>(item.drugmedi);
                dto.DrugName = item.drug.Name;
                dto.DrugNDC = item.drug.NDC;
                return dto;
            }).ToList();
            return result;

        }

        internal async Task<ICollection<Drug>> GetDrugsByInsuranceNamePagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug)
                .Include(di => di.Insurance)
                .Where(di => di.Insurance != null && di.Insurance.RxGroup.ToLower() == insurance.ToLower() &&
                             di.Drug.Name.ToLower().Contains(drugName.ToLower()))
                .Select(di => di.Drug)
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugsByPCNPagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug)
                .Include(di => di.Insurance.InsurancePCN)
                .Where(di => di.Insurance != null && di.Insurance.InsurancePCN.PCN.ToLower() == insurance.ToLower() &&
                             di.Drug.Name.ToLower().Contains(drugName.ToLower()))
                .Select(di => di.Drug)
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugsByBINPagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug)
                .Include(di => di.Insurance.InsurancePCN.Insurance)
                .Where(di => di.Insurance != null &&
                             di.Insurance.InsurancePCN.Insurance.Bin.ToLower() == insurance.ToLower() &&
                             di.Drug.Name.ToLower().Contains(drugName.ToLower()))
                .OrderByDescending(di => di.Net)
                .ThenByDescending(di => di.InsurancePayment)
                .Select(di => di.Drug)
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugClassesByInsuranceNamePagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var drugClasses = await _context.DrugInsurances
                .Include(di => di.Drug.DrugClass)
                .Include(di => di.Insurance)
                .Where(di => di.Insurance != null && di.Insurance.RxGroup.ToLower() == insurance.ToLower() &&
                             di.Drug.DrugClass.Name.ToLower().Contains(drugClassName.ToLower()))
                .OrderByDescending(di => di.Net)
                .ThenByDescending(di => di.InsurancePayment)
                .Select(di => di.Drug)
                .GroupBy(d => d.DrugClassId)
                .Select(g => g.First())
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugClasses;
        }
        internal async Task<ICollection<Drug>> GetDrugClassesByPCNPagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug.DrugClass)
                .Include(di => di.Insurance.InsurancePCN)
                .Where(di => di.Insurance != null && di.Insurance.InsurancePCN.PCN.ToLower() == insurance.ToLower() &&
                             di.Drug.DrugClass.Name.ToLower().Contains(drugClassName.ToLower()))
                .OrderByDescending(di => di.Net)
                .ThenByDescending(di => di.InsurancePayment)
                .Select(di => di.Drug)
                .GroupBy(d => d.DrugClassId)
                .Select(g => g.First())
                .Distinct()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugs;
        }
        internal async Task<ICollection<Drug>> GetDrugClassesByBINPagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var drugs = await _context.DrugInsurances
                .Include(di => di.Drug.DrugClass)
                .Include(di => di.Insurance.InsurancePCN.Insurance)
                .Where(di => di.Insurance != null &&
                             di.Insurance.InsurancePCN.Insurance.Bin.ToLower() == insurance.ToLower() &&
                             di.Drug.DrugClass.Name.ToLower().Contains(drugClassName.ToLower()))
                .Select(di => di.Drug)
                .GroupBy(d => d.DrugClassId)
                .Select(g => g.First())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return drugs;
        }
    }
    // public sealed class InsuranceMap : ClassMap<Insurance>
    // {
    //     public InsuranceMap()
    //     {
    //         Map(m => m.Bin).Name("PCN");
    //         Map(m => m.Pcn).Name("Bin");
    //         Map(m => m.Name).Name("InsuranceShortName");
    //         Map(m => m.RxGroup).Name("RxGroup");
    //     }
    // }

}