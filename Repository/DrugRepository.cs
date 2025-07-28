using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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
using SearchTool_ServerSide.Dtos.ClassDtos;
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
        public async Task<ICollection<Drug>> GetDrugsByName(string query, int page = 1, int pageSize = 20)
        {
            int offset = (page - 1) * pageSize;

            // Set pg_trgm similarity threshold
            await _context.Database.ExecuteSqlRawAsync("SET pg_trgm.similarity_threshold = 0.3;");

            var sql = @"
                    WITH ranked AS (
                        SELECT *,
                            ROW_NUMBER() OVER (
                                PARTITION BY name_unaccent
                                ORDER BY 
                                    (
                                        similarity(name_unaccent, unaccent({0})) * 0.5 +
                                        ts_rank(name_tsv, plainto_tsquery(unaccent({0}))) * 0.3 +
                                        CASE WHEN name_soundex = soundex(unaccent({0})) THEN 0.1 ELSE 0 END +
                                        CASE WHEN name_unaccent ILIKE '%' || unaccent({0}) || '%' THEN 0.1 ELSE 0 END
                                    ) DESC
                            ) AS rn,
                            similarity(name_unaccent, unaccent({0})) AS sim,
                            ts_rank(name_tsv, plainto_tsquery(unaccent({0}))) AS ts_rank,
                            soundex(name_unaccent) AS sndx
                        FROM ""Drugs""
                        WHERE name_unaccent % unaccent({0})
                        OR name_tsv @@ plainto_tsquery(unaccent({0}))
                        OR name_soundex = soundex(unaccent({0}))
                        OR name_unaccent ILIKE '%' || unaccent({0}) || '%'
                    )
                    SELECT *
                    FROM ranked
                    WHERE rn = 1
                    ORDER BY sim DESC, ts_rank DESC
                    LIMIT {1} OFFSET {2};
                    ";

            var results = await _context.Drugs
                .FromSqlRaw(sql, query, pageSize, offset)
                .AsNoTracking()
                .ToListAsync();

            return results;
        }


        public async Task<ICollection<DrugModal>> GetClassesByName(
            string name,
            string classVersion,
            int pageNumber,
            int pageSize = 20)
        {
            // Step 1: SQL query — filter & order
            var query =
                from dc in _context.DrugClasses
                join drug in _context.Drugs on dc.DrugId equals drug.Id
                join ci in _context.ClassInfos on dc.ClassId equals ci.Id
                join ct in _context.ClassTypes on ci.ClassTypeId equals ct.Id
                where EF.Functions.ILike(ci.Name, $"%{name}%")
                   && EF.Functions.ILike(ct.Name, classVersion)
                orderby ci.Id, drug.Id
                select new
                {
                    Drug = drug,
                    ClassInfo = ci,
                    ClassType = ct
                };

            var rawResults = await query.ToListAsync();

            // Step 2: Distinct by ClassInfo.Id — in memory
            var distinctResults = rawResults
                .GroupBy(x => x.ClassInfo.Id)
                .Select(g => g.First()) // take the first (best) drug per class
                .OrderBy(x => x.ClassInfo.Id) // stable order
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugModal
                {
                    Id = x.Drug.Id,
                    Name = x.Drug.Name,
                    Ndc = x.Drug.NDC,
                    Form = x.Drug.Form,
                    Strength = x.Drug.Strength,
                    ClassId = x.ClassInfo.Id,
                    ClassType = x.ClassType.Name,
                    ClassName = x.ClassInfo.Name,
                    Acq = x.Drug.ACQ,
                    Awp = x.Drug.AWP,
                    Rxcui = x.Drug.Rxcui ?? 0,
                    Route = x.Drug.Route,
                    TeCode = x.Drug.TECode,
                    Ingrdient = x.Drug.Ingrdient,
                    ApplicationNumber = x.Drug.ApplicationNumber,
                    ApplicationType = x.Drug.ApplicationType,
                    StrengthUnit = x.Drug.StrengthUnit,
                    Type = x.Drug.Type
                })
                .ToList();

            return distinctResults;
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
        public async Task SaveData(string filePath = "drug_enriched_with_group.csv")
        {
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
            var classTypes = await context.ClassTypes.ToDictionaryAsync(di => di.Name, di => di);
            var classInfos = await context.ClassInfos.ToDictionaryAsync(di => (di.Name, di.ClassTypeId), di => di);
            var drugClasses = await context.DrugClasses.ToDictionaryAsync(dc => (dc.ClassId, dc.DrugId), dc => dc);

            // **Step 3: Load Existing Drugs by NDC & Name**
            var existingDrugsByNdc = await context.Drugs
                .GroupBy(d => d.NDC)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var existingDrugsByName = await context.Drugs
                .GroupBy(d => d.Name)
                .ToDictionaryAsync(g => g.Key, g => g.First());
            var newClassTypes = new List<ClassType>();
            var newDrugs = new List<Drug>();
            var newDrugClasses = new List<DrugClass>();
            var newClassInfos = new List<ClassInfo>();
            //Name  : Description
            var addedclassTypes = new List<(string, string)>
                {
                    ("ClassV1","Exact Match"),
                    ("ClassV2","est 25% different in strength"),
                    ("ClassV3","Group_By_Class_Standardized"),
                    ("ClassV4","Cleaned with epc Names"),
                    ("ClassV5","EPC + MOA + Route")
                };
            foreach (var classType in addedclassTypes)
            {
                if (!classTypes.TryGetValue(classType.Item1, out var tempClassType))
                {
                    tempClassType = new ClassType
                    {
                        Name = classType.Item1,
                        Description = classType.Item2
                    };
                    newClassTypes.Add(tempClassType);
                }
            }
            if (newClassTypes.Any())
            {
                await context.ClassTypes.AddRangeAsync(newClassTypes);
                await context.SaveChangesAsync();

                newClassTypes.Clear();
            }
            var classTypeList = await _context.ClassTypes.ToListAsync();
            foreach (var record in records)
            {
                var tempClassType = new List<(string, string)>
                {
                    (record.DrugClass,"ClassV1"),
                    (record.ClassV2,"ClassV2"),
                    (record.ClassV3,"ClassV3"),
                    (record.ClassV4,"ClassV4"),
                    (record.ClassV5,"ClassV5")
                };


                for (int i = 0; i < tempClassType.Count; i++)
                {
                    var type = tempClassType[i].Item2;
                    if (classTypes.TryGetValue(type, out var classType))
                    {
                        var className = tempClassType[i].Item1;
                        if (string.IsNullOrWhiteSpace(className))
                        {
                            // Skip if className is null or empty
                            continue;
                        }
                        var classInfoKey = (className, classType.Id);
                        if (!classInfos.TryGetValue(classInfoKey, out var classInfo))
                        {
                            classInfo = new ClassInfo { Name = className, ClassTypeId = classType.Id };
                            newClassInfos.Add(classInfo);
                            classInfos[classInfoKey] = classInfo; // Add to dictionary for future lookups
                        }
                    }
                }



            }

            // Batch insert new drug classes
            if (newClassInfos.Any())
            {
                await context.ClassInfos.AddRangeAsync(newClassInfos);
                await context.SaveChangesAsync();
                Console.WriteLine($"Added {newClassInfos.Count} new drug classes at {DateTime.Now}");
            }
            const int batchSize = 50000;
            foreach (var record in records)
            {
                record.Name = record.Name.ToUpper();
                string tempNdc = NormalizeNdcTo11Digits(record.NDC);

                // **Check if Drug Exists by NDC**
                if (existingDrugsByNdc.ContainsKey(tempNdc))
                {
                    continue; // Skip existing drug
                }

                {
                    // **Create Drug Class if Missing**
                    var newDrug = new Drug
                    {
                        Name = record.Name,
                        NDC = tempNdc,
                        Form = record.Form,
                        Strength = record.Strength,
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
                if (newDrugs.Count >= batchSize)
                {
                    await context.Drugs.AddRangeAsync(newDrugs);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Processed batch of {batchSize} drugs at {DateTime.Now}");
                    newDrugs.Clear();
                }
            }

            if (newDrugs.Any())
            {
                await context.Drugs.AddRangeAsync(newDrugs);
                await context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newDrugs.Count} drugs at {DateTime.Now}");

                // Refresh the dictionary to include new drugs with their generated IDs
                existingDrugsByNdc = await context.Drugs
                    .GroupBy(d => d.NDC)
                    .ToDictionaryAsync(g => g.Key, g => g.First());
                newDrugs.Clear();
            }
            foreach (var record in records)
            {
                var tempClassType = new List<(string, string)>
                {
                    (record.DrugClass,"ClassV1"),
                    (record.ClassV2,"ClassV2"),
                    (record.ClassV3,"ClassV3"),
                    (record.ClassV4,"ClassV4"),
                    (record.ClassV5,"ClassV5")
                };

                string tempNdc = NormalizeNdcTo11Digits(record.NDC);
                if (existingDrugsByNdc.TryGetValue(tempNdc, out var drug))
                {

                    for (int i = 0; i < tempClassType.Count(); i++)
                    {
                        var type = tempClassType[i].Item2;
                        if (classTypes.TryGetValue(type, out var classType))
                        {
                            var className = tempClassType[i].Item1;
                            var classInfoKey = (className, classType.Id);

                            if (classInfos.TryGetValue(classInfoKey, out var classInfo))
                            {
                                if (!drugClasses.ContainsKey((classInfo.Id, drug.Id)))
                                {
                                    var newDrugClass = new DrugClass
                                    {
                                        ClassId = classInfo.Id,
                                        DrugId = drug.Id
                                    };
                                    drugClasses[(classInfo.Id, drug.Id)] = newDrugClass;
                                    newDrugClasses.Add(newDrugClass);
                                }
                            }

                        }
                    }
                }
                if (newDrugClasses.Count >= batchSize)
                {
                    await context.DrugClasses.AddRangeAsync(newDrugClasses);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Processed batch of {batchSize} drugCLasses at {DateTime.Now}");
                    newDrugClasses.Clear();
                }
            }
            if (newDrugClasses.Any())
            {
                await context.DrugClasses.AddRangeAsync(newDrugClasses);
                await context.SaveChangesAsync();
                Console.WriteLine($"Processed final batch of {newDrugClasses.Count} newDrugClasses at {DateTime.Now}");
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
            var existingDrugClass = await _context.DrugClasses.ToDictionaryAsync(dc => (dc.ClassId, dc.DrugId));
            var classTypes = await _context.ClassTypes.ToDictionaryAsync(dc => dc.Name);
            var drugBranchDict = await _context.DrugBranches.ToDictionaryAsync(g => (g.BranchId, g.DrugNDC));
            var diDict = await _context.DrugInsurances.ToDictionaryAsync(di => (di.InsuranceId, di.DrugId, di.BranchId));
            var classInfoDict = await _context.ClassInfos.ToDictionaryAsync(ci => (ci.Name, ci.ClassTypeId));
            var existingDrugMediByNDC = await _context.DrugMedis
                                                .GroupBy(d => d.DrugNDC)
                                                .ToDictionaryAsync(g => g.Key, g => g.First());
            var drugMedis = new List<DrugMedi>();
            var unmatched = new List<string>();
            var drugBranches = new List<DrugBranch>();
            var newDrugInsurances = new List<DrugInsurance>();
            var newDrug = new List<Drug>();
            var newDrugClass = new List<DrugClass>();
            var newClassInfo = new List<ClassInfo>();
            var insuranceBin = await _context.Insurances.FirstOrDefaultAsync(x => x.Bin == "022659");
            var tempclassTypes = new List<string>
                {
                    "ClassV1",
                    "ClassV2",
                    "ClassV3",
                    "ClassV4",
                    "classV5"
                };
            var tempDrugNdcs = new List<string>();

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
                for (int i = 0; i < tempclassTypes.Count(); i++)
                {
                    if (classTypes.TryGetValue(tempclassTypes[i], out var classType))
                    {
                        var classInfoKey = (labelName, classType.Id);
                        if (!classInfoDict.TryGetValue(classInfoKey, out var classInfo))
                        {
                            classInfo = new ClassInfo
                            {
                                Name = labelName,
                                ClassTypeId = classType.Id
                            };

                            newClassInfo.Add(classInfo);
                            classInfoDict[classInfoKey] = classInfo;
                        }
                    }
                }


            }
            if (newClassInfo.Any())
            {
                await _context.ClassInfos.AddRangeAsync(newClassInfo);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newClassInfo.Count} drugs at {DateTime.Now}");
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

                    var newDrugItem = new Drug
                    {
                        NDC = productId,
                        Name = labelName,

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
                    tempDrugNdcs.Add(productId);
                }

            }
            if (newDrug.Any())
            {
                await _context.Drugs.AddRangeAsync(newDrug);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Processed batch of {newDrug.Count} drugs at {DateTime.Now}");
            }
            foreach (var tempNDC in tempDrugNdcs)
            {
                if (existingDrugsByNdc.TryGetValue(tempNDC, out var drug))
                {
                    for (int i = 0; i < tempclassTypes.Count(); i++)
                    {
                        if (classTypes.TryGetValue(tempclassTypes[i], out var classType))
                        {
                            var classInfoKey = (drug.Name, classType.Id);
                            if (classInfoDict.TryGetValue(classInfoKey, out var classInfo))
                            {
                                var drugClassKey = (classInfo.Id, drug.Id);
                                if (!existingDrugClass.TryGetValue(drugClassKey, out var existingClass))
                                {
                                    existingClass = new DrugClass
                                    {
                                        DrugId = drugClassKey.Item2,
                                        ClassId = drugClassKey.Item1
                                    };
                                    existingDrugClass[drugClassKey] = existingClass;
                                }
                            }
                        }
                    }
                }
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
                            Date = dateTime != null
                                ? (DateTime.TryParseExact(dateTime,
                                                        new[] { "MM-dd-yy", "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd" },
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out var parsedDate)
                                    ? parsedDate.ToUniversalTime()
                                    : DateTime.UtcNow)
                                : DateTime.UtcNow,
                            Prescriber = "",
                            Quantity = 0,
                            AcquisitionCost = 0,
                            Discount = 0,
                            InsurancePayment = insurancePay != null
                                ? (decimal.Parse(insurancePay) * 0.9m)
                                : 0m,
                            PatientPayment = 0,
                        };
                        newDrugInsurances.Add(newDI);
                        diDict.Add(diKey, newDI);
                    }




                    if (!drugBranchDict.ContainsKey((1, drug.NDC)))
                    {
                        var newDrugBranch = new DrugBranch
                        {
                            BranchId = 6,
                            DrugNDC = drug.NDC
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






        public async Task ImportDrugInsuranceAsync(string filePath = "Scripts22-7-2025.csv")
        {
            // ========================================================
            // PHASE 0: Read CSV Records
            // ========================================================
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
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
            var drugDict = drugsFromDb
                                    .GroupBy(d => d.NDC)
                                    .ToDictionary(g => g.Key, g => g.First());
            var classInfoDict = await _context.ClassInfos.ToDictionaryAsync(ci => ci.Id);
            var drugByNameDict = drugsFromDb
                                    .GroupBy(d => d.Name)
                                    .ToDictionary(g => g.Key, g => g.First());
            var drugClassDict = await _context.DrugClasses.ToDictionaryAsync(dc => new { dc.ClassId, dc.DrugId });
            var newInsurances = new List<Insurance>();
            var newInsurancePCNs = new List<InsurancePCN>();
            var newInsuranceRxes = new List<InsuranceRx>();

            var newDrugs = new List<Drug>();
            var newDrugClasses = new List<DrugClass>();
            int batchSize = 1000, countPhase1 = 0;
            var newScriptsDrugs = new List<(string, List<int>)>();

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
                            ACQ = record.AcquisitionCost,
                            AWP = 0,
                            Rxcui = tempDrug.Rxcui,
                            Route = tempDrug.Route,
                            Ingrdient = tempDrug.Ingrdient,
                            TECode = tempDrug.TECode,
                            ApplicationNumber = tempDrug.ApplicationNumber,
                            ApplicationType = tempDrug.ApplicationType
                        };
                        var classInfos = await _context.DrugClasses.Where(x => x.DrugId == tempDrug.Id).Select(x => x.ClassId).ToListAsync();
                        newDrugs.Add(drug);
                        newScriptsDrugs.Add((drug.NDC, classInfos));
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
            foreach (var item in newScriptsDrugs)
            {
                if (drugDict.TryGetValue(item.Item1, out var drug))
                {
                    foreach (var classInfoId in item.Item2)
                    {
                        if (!drugClassDict.TryGetValue(new { ClassId = classInfoId, DrugId = drug.Id }, out var drugClass))
                        {
                            newDrugClasses.Add(drugClass);
                        }
                    }
                }
            }
            if (newDrugClasses.Any())
            {
                _context.DrugClasses.AddRange(newDrugClasses);
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
                .ToDictionary(ci => (ci.InsuranceId, ci.ClassInfoId, ci.Date.Year, ci.Date.Month, ci.BranchId));


            var drugBranchDict = await _context.DrugBranches.ToDictionaryAsync(g => (g.BranchId, g.DrugNDC));
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
            var drugIds = await _context.Drugs.Select(d => d.Id).ToListAsync();
            var allDrugClasses = await _context.DrugClasses
                .Where(dc => drugIds.Contains(dc.DrugId))
                .ToListAsync();

            // Group them by DrugId
            var drugClassMap = allDrugClasses
                .GroupBy(dc => dc.DrugId)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var record in processedRecords)
            {
                decimal qty = 1;
                decimal realQTY = 1;
                record.RemainingStock = new Random().Next(10, 101);
                if (record.Quantity != "tableCell29")
                {
                    realQTY = decimal.Parse(record.Quantity);
                }

                // Normalize NDC and parse the date.
                record.NDCCode = NormalizeNdcTo11Digits(record.NDCCode);
                DateTime recordDate = DateTime.ParseExact(record.Date, "MM-dd-yy", CultureInfo.InvariantCulture)
                                                    .ToUniversalTime();
                decimal netValue = record.PatientPayment / qty + record.InsurancePayment / qty - record.AcquisitionCost / qty;
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

                var classInfoIds = drugClassMap.ContainsKey(drug.Id) ? drugClassMap[drug.Id] : new List<DrugClass>();

                // Only process if there are valid classInfoIds


                // -----------------------
                // Merge DrugInsurance
                // ------------------
                if (!branchDict.TryGetValue(record.Branch, out var branch))
                    continue;

                var diKey = (insuranceItem.Id, drug.Id, branch.Id);
                if (diDict.TryGetValue(diKey, out var existingDI))
                {
                    if (existingDI.Date < recordDate)
                    {
                        existingDI.Net = netValue;
                        existingDI.Quantity = realQTY;
                        existingDI.AcquisitionCost = record.AcquisitionCost;
                        existingDI.Discount = record.Discount;
                        existingDI.InsurancePayment = record.InsurancePayment;
                        existingDI.PatientPayment = record.PatientPayment;
                        existingDI.Date = recordDate;
                        existingDI.ScriptCode = record.Script;
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
                        ScriptCode = record.Script,
                        Date = recordDate,
                        Prescriber = record.Prescriber,
                        Quantity = realQTY,
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
                foreach (var classInfoId in classInfoIds)
                {
                    var ciKey = (insuranceItem.Id, classInfoId.ClassId, recordDate.Year, recordDate.Month, branch.Id);
                    if (ciDict.TryGetValue(ciKey, out var existingCI))
                    {
                        // Update if this record has a higher net value.
                        if (netValue > existingCI.BestNet)
                        {
                            existingCI.BestNet = netValue / realQTY;
                            existingCI.BestACQ = record.AcquisitionCost / realQTY;
                            existingCI.BestInsurancePayment = record.InsurancePayment / realQTY;
                            existingCI.BestPatientPayment = record.PatientPayment / realQTY;
                            existingCI.DrugId = drug.Id;
                            existingCI.Qty = realQTY;
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
                            ClassInfoId = classInfoId.ClassId,
                            DrugId = drug.Id,
                            BranchId = branch.Id,
                            Date = yearMonth,
                            ScriptDateTime = yearMonth,
                            ScriptCode = record.Script,
                            BestNet = netValue / realQTY,
                            BestACQ = record.AcquisitionCost / realQTY,
                            BestInsurancePayment = record.InsurancePayment / realQTY,
                            BestPatientPayment = record.PatientPayment / realQTY,
                            Qty = realQTY,
                        };
                        newClassInsurances.Add(newCI);
                        ciDict.Add(ciKey, newCI);
                    }
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
                var tempkey = (branch.Id, drug.NDC);
                if (!drugBranchDict.TryGetValue(tempkey, out var drugBranch))
                {
                    var newDrugBranch = new DrugBranch
                    {
                        BranchId = branch.Id,
                        DrugNDC = drug.NDC,
                        Stock = record.RemainingStock
                    };
                    newDrugBranches.Add(newDrugBranch);
                    drugBranchDict.Add(tempkey, newDrugBranch);
                }
                if (!userDict.ContainsKey(record.User))
                {
                    var newUser = new User { ShortName = record.User, Name = record.User, Email = $"{record.User}@pharmacy.com", Password = BCrypt.Net.BCrypt.HashPassword("DefaultPass123"), BranchId = branch.Id };
                    newUsers.Add(newUser);
                    userDict[record.User] = newUser;
                }
                if (!userDict.ContainsKey(record.Prescriber))
                {
                    var newPrescriber = new User { ShortName = record.Prescriber, Name = record.Prescriber, Email = $"{record.Prescriber}@pharmacy.com", Password = BCrypt.Net.BCrypt.HashPassword("DefaultPass123"), BranchId = branch.Id };
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
                if (!scriptDict.TryGetValue(record.Script, out var script))
                    continue;
                decimal realQTY = 1;
                record.RemainingStock = new Random().Next(10, 101);
                if (record.Quantity != "tableCell29")
                {
                    realQTY = decimal.Parse(record.Quantity);
                }
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
                        RxNumber = record.RxNumber,
                        UserEmail = prescriber.Email,
                        PF = record.PF,
                        Quantity = realQTY,
                        RemainingStock = record.RemainingStock,
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



        internal async Task<DrugsAlternativesReadDto> GetDetails(string ndc, int insuranceId)
        {
            var query = from di in _context.DrugInsurances
                        join irx in _context.InsuranceRxes on di.InsuranceId equals irx.Id
                        join ipcn in _context.InsurancePCNs on irx.InsurancePCNId equals ipcn.Id
                        join ins in _context.Insurances on ipcn.InsuranceId equals ins.Id
                        where di.NDCCode == ndc
                              && di.InsuranceId == insuranceId
                              && di.Date == _context.DrugInsurances
                                                .Where(x => x.NDCCode == ndc && x.InsuranceId == insuranceId)
                                                .Max(x => x.Date) // Get the newest date
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
            return dto;
        }

        internal async Task<DrugClass> getClassbyId(int id)
        {
            var item = await _context.DrugClasses.FirstOrDefaultAsync(x => x.Id == id);
            return item;
        }
        internal async Task<ClassInfo> getClassbyName(string name, string type = "ClassV1")
        {
            var item = await _context.ClassInfos.FirstOrDefaultAsync(x => x.Name == name);
            return item;
        }
        internal async Task<IEnumerable<ClassInfoReadDto>> GetClassesByDrugId(int drugId)
        {
            return await _context.DrugClasses
                                .Where(dc => dc.DrugId == drugId)
                                .Select(dc => new ClassInfoReadDto
                                {
                                    Id = dc.ClassInfo.Id,
                                    Name = dc.ClassInfo.Name,
                                    ClassTypeId = dc.ClassInfo.ClassTypeId,
                                    ClassTypeName = dc.ClassInfo.ClassType.Name
                                })
                                .ToListAsync();

        }
        internal async Task<ICollection<Drug>> GetDrugsByClass(int classId)
        {
            var items = await _context.Drugs
                .Where(x => x.DrugClasses.Any(dc => dc.ClassId == classId))
                .GroupBy(x => x.Name)
                .Select(g => g.First())
                .ToListAsync();
            return items;
        }
        internal async Task<ICollection<DrugInsurance>> GetAllLatest()
        {
            var items = await _context.DrugInsurances
                .AsNoTracking()
                .ToListAsync();
            return items;
        }


        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugs(int classInfoId)
        {
            // Step 1: Get all drugs in that ClassInfo (via DrugClasses table)
            var query =
                        from dc in _context.DrugClasses
                        where dc.ClassId == classInfoId
                        join d in _context.Drugs on dc.DrugId equals d.Id
                        join ci in _context.ClassInfos on dc.ClassId equals ci.Id

                        join diGroup in _context.DrugInsurances
                            on dc.DrugId equals diGroup.DrugId into diGroup
                        from di in diGroup.DefaultIfEmpty() // now di can be null

                        join dbGroup in _context.DrugBranches
                            on new { DrugNDC = d.NDC, BranchId = di != null ? di.BranchId : 1 }
                            equals new { dbGroup.DrugNDC, dbGroup.BranchId } into dbGroup
                        from db in dbGroup.DefaultIfEmpty()

                        select new
                        {
                            Drug = d,
                            DrugBranch = db,
                            DrugClass = dc,
                            ClassInfo = ci,
                            DrugInsurance = di
                        };


            var list = await query.ToListAsync();

            var branchDict = await _context.Branches.ToDictionaryAsync(x => x.Id);

            var insuranceRxDict = await _context.InsuranceRxes
                .Include(ir => ir.InsurancePCN)
                    .ThenInclude(ipcn => ipcn.Insurance)
                .ToDictionaryAsync(x => x.Id);

            var result = list.Select(item =>
            {
                var di = item.DrugInsurance;

                var dto = _mapper.Map<DrugsAlternativesReadDto>(di ?? new DrugInsurance
                {
                    DrugId = item.Drug.Id,
                    NDCCode = item.Drug.NDC,
                    Net = 0,
                    Date = DateTime.UtcNow,
                    Quantity = 1,
                    AcquisitionCost = 0,
                    Discount = 0,
                    InsurancePayment = 0,
                    PatientPayment = 0,
                    BranchId = 1,
                    InsuranceId = 0,
                    Drug = item.Drug
                });

                dto.DrugName = item.Drug.Name;
                dto.NDCCode = item.Drug.NDC;
                dto.DrugClassId = item.DrugClass.Id;
                dto.DrugClass = item.ClassInfo.Name;
                dto.Quantity = di?.Quantity ?? 1;
                dto.ApplicationNumber = item.Drug.ApplicationNumber;
                dto.ApplicationType = item.Drug.ApplicationType;
                dto.Route = item.Drug.Route;
                dto.Strength = item.Drug.Strength;
                dto.Form = item.Drug.Form;
                dto.Ingrdient = item.Drug.Ingrdient;
                dto.StrengthUnit = item.Drug.StrengthUnit;
                dto.Type = item.Drug.Type;
                dto.TECode = item.Drug.TECode;
                dto.Stock = item.DrugBranch?.Stock ?? 0;
                dto.ScriptCode = di?.ScriptCode;

                if (di != null && insuranceRxDict.TryGetValue(di.InsuranceId, out var insuranceRx))
                {
                    dto.insuranceName = insuranceRx.RxGroup;
                    dto.pcn = insuranceRx.InsurancePCN?.PCN;
                    dto.bin = insuranceRx.InsurancePCN?.Insurance?.Bin;
                    dto.rxgroup = insuranceRx.RxGroup;
                    dto.BinFullName = insuranceRx.InsurancePCN?.Insurance?.Name;
                    dto.binId = insuranceRx.InsurancePCN?.Insurance?.Id ?? 0;
                    dto.pcnId = insuranceRx.InsurancePCN?.Id ?? 0;
                    dto.rxgroupId = insuranceRx.Id;
                }

                if (di != null && branchDict.TryGetValue(di.BranchId, out var branch))
                    dto.branchName = branch.Name;

                return dto;
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
            public int RemainingStock { get; set; } = 0;
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
            [Name("EPC_Class_Name_Cleaned")]
            public string? ClassV4 { get; set; }
            [Name("ROUTE_AND_CLASS_RENAMED_FOR_PHARMACIST")]
            public string? ClassV5 { get; set; }
            [Name("PHARM_CLASSES")]
            public string? PHARM_CLASSES { get; set; }
        }

        public async Task<ICollection<AuditReadDto>> GetAllLatestScriptsPaginated(int pageNumber, int pageSize, string classVersion = "ClassV1", string matchOn = "BIN")
        {
            // Use classVersion as part of the cache key
            string cacheKey = $"AllLatestScripts_{classVersion}_{matchOn}";
            List<AuditReadDto> allData;

            // Try to get the specific classVersion from the cache
            if (!_cache.TryGetValue(cacheKey, out allData))
            {
                // Cache miss: Load the entire dataset for this classVersion
                allData = await GetAuditDtosWithBestBeforeOrPrevMonthAsync(classVersion, matchOn);

                // Set up cache options (e.g., 120 minutes sliding expiration)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(120));

                // Cache this version separately
                _cache.Set(cacheKey, allData, cacheOptions);
            }

            // Paginate the cached data
            var pagedData = allData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedData;
        }

 private async Task<List<AuditReadDto>> GetAuditDtosWithBestBeforeOrPrevMonthAsync(string classTypeName, string matchOn)
        {
            // Step 1️⃣ Load all ScriptItems + navigation
            var scriptItems = await _context.ScriptItems
                .AsNoTracking()
                .Include(si => si.Script).ThenInclude(s => s.Branch)
                .Include(si => si.Drug).ThenInclude(d => d.DrugClasses).ThenInclude(dc => dc.ClassInfo).ThenInclude(ci => ci.ClassType)
                .Include(si => si.Insurance)
                    .ThenInclude(irx => irx.InsurancePCN)
                        .ThenInclude(pcn => pcn.Insurance)
                .Include(si => si.Prescriber)
                .Where(si => si.Drug.DrugClasses.Any(dc => dc.ClassInfo.ClassType.Name == classTypeName))
                .ToListAsync();

            if (!scriptItems.Any()) return new();

            // Step 2️⃣ Load all ClassInsurances with full navigation to BIN
            var classInsurances = await _context.ClassInsurances
                .AsNoTracking()
                .Include(ci => ci.Drug)
                .Include(ci => ci.Insurance)
                    .ThenInclude(irx => irx.InsurancePCN)
                        .ThenInclude(pcn => pcn.Insurance)
                .ToListAsync();

            // Step 3️⃣ Group ClassInsurances by (BranchId, BIN, ClassInfoId)
            var ciGroups = classInsurances
                .Where(ci => ci.Insurance?.InsurancePCN?.Insurance?.Bin != null)
                .GroupBy(ci =>
                {
                    string? matchValue = matchOn.ToUpper() switch
                    {
                        "BIN" => ci.Insurance?.InsurancePCN?.Insurance?.Bin,
                        "PCN" => ci.Insurance?.InsurancePCN?.PCN,
                        "RX" => ci.Insurance?.RxGroup,
                        _ => null
                    };

                    return new
                    {
                        ci.BranchId,
                        Match = matchValue,
                        ci.ClassInfoId
                    };
                })
                .Where(g => g.Key.Match != null)
                .ToDictionary(g => g.Key, g => g.ToList());


            // Step 4️⃣ Build ScriptItem RemainingStock dictionary
            var stockDict = scriptItems
                .GroupBy(si => (si.Script.ScriptCode, si.DrugId))
                .ToDictionary(
                    g => g.Key,
                    g => g.First().RemainingStock
                );

            var auditDtos = new List<AuditReadDto>();

            foreach (var si in scriptItems)
            {
                var scriptDate = si.Script.Date.ToUniversalTime();
                var prevMonth = StartOfMonth(scriptDate).AddMonths(-1);

                var drugClassInfoId = si.Drug.DrugClasses
                    .First(dc => dc.ClassInfo.ClassType.Name == classTypeName)
                    .ClassId;

                string? matchValue = matchOn.ToUpper() switch
                {
                    "BIN" => si.Insurance?.InsurancePCN?.Insurance?.Bin,
                    "PCN" => si.Insurance?.InsurancePCN?.PCN,
                    "RX" => si.Insurance?.RxGroup,
                    _ => null
                };

                if (matchValue == null) continue;

                var key = new
                {
                    si.Script.BranchId,
                    Match = matchValue,
                    ClassInfoId = drugClassInfoId
                };

                ClassInsurance? bestAlt = null;

                if (ciGroups.TryGetValue(key, out var ciList))
                {
                    // First try: previous month
                    bestAlt = ciList
                        .Where(ci => StartOfMonth(ci.Date) == prevMonth)
                        .OrderByDescending(ci => ci.BestNet)
                        .FirstOrDefault();

                    // Fallback: most recent ≤ scriptDate
                    if (bestAlt == null)
                    {
                        bestAlt = ciList
                            .Where(ci => ci.Date <= scriptDate)
                            .OrderByDescending(ci => ci.Date)
                            .ThenByDescending(ci => ci.BestNet)
                            .FirstOrDefault();
                    }
                }

                var dto = new AuditReadDto
                {
                    Date = si.Script.Date,
                    RemainingStock = si.RemainingStock,
                    ScriptCode = si.Script.ScriptCode,
                    RxNumber = si.RxNumber,
                    User = si.UserEmail.Replace(".@pharmacy.com", ""),
                    Prescriber = si.Prescriber?.Name ?? "",
                    DrugName = si.Drug.Name,
                    DrugId = si.DrugId,
                    InsuranceRx = si.Insurance?.RxGroup ?? "",
                    BINCode = si.Insurance?.InsurancePCN?.Insurance?.Bin ?? "",
                    BINName = si.Insurance?.InsurancePCN?.Insurance?.Name ?? "",
                    PCNName = si.Insurance?.InsurancePCN?.PCN ?? "",
                    RxGroupId = si.Insurance?.Id ?? 1,
                    PcnId = si.Insurance.InsurancePCN?.Id ?? 1,
                    BinId = si.Insurance?.InsurancePCN?.Insurance?.Id ?? 1,
                    InsuranceId = si.InsuranceId,
                    PF = si.PF,
                    Quantity = si.Quantity,
                    AcquisitionCost = si.AcquisitionCost,
                    Discount = si.Discount,
                    InsurancePayment = si.InsurancePayment,
                    PatientPayment = si.PatientPayment,
                    NetProfit = si.NetProfit,
                    NDCCode = si.NDCCode,
                    DrugClass = si.Drug.DrugClasses.First(dc => dc.ClassInfo.ClassType.Name == classTypeName).ClassInfo.Name,
                    BranchCode = si.Script.Branch.Code,
                    NetProfitPerItem = si.NetProfit / si.Quantity,

                };

                if (bestAlt != null)
                {
                    dto.HighestDrugId = bestAlt.DrugId;
                    dto.HighestDrugName = bestAlt.Drug?.Name ?? "";
                    dto.HighestDrugNDC = bestAlt.Drug?.NDC ?? "";
                    dto.HighestNet = bestAlt.BestNet * si.Quantity;
                    dto.HighestScriptCode = bestAlt.ScriptCode;
                    dto.HighestScriptDate = bestAlt.ScriptDateTime;
                    dto.HighestNetProfitPerItem = bestAlt.BestNet;
                    dto.HighestQuantity = bestAlt.Qty;
                    dto.HighestBINCode = bestAlt.Insurance?.InsurancePCN?.Insurance?.Bin ?? "";
                    dto.HighestBINName = bestAlt.Insurance?.InsurancePCN?.Insurance?.Name ?? "";
                    dto.HighestPCNName = bestAlt.Insurance?.InsurancePCN?.PCN ?? "";
                    dto.HighestInsuranceRx = bestAlt.Insurance?.RxGroup ?? "";
                    dto.HighestRxGroupId = bestAlt.InsuranceId;
                    dto.HighestPcnId = bestAlt.Insurance?.InsurancePCN?.Id ?? 1;
                    dto.HighestBinId = bestAlt.Insurance?.InsurancePCN?.Insurance?.Id ?? 1;
                    stockDict.TryGetValue(
                        (bestAlt.ScriptCode, bestAlt.DrugId),
                        out int altRemainingStock);

                    dto.HighestRemainingStock = altRemainingStock;
                }

                auditDtos.Add(dto);
            }

            return auditDtos;
        }
        // Helper method
        private static DateTime StartOfMonth(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }



        private async Task<List<AuditReadDto>> LoadAllLatestScriptsFromDatabaseAsync(string classType = "ClassV1")
        {
            var auditData = await (
                from script in _context.Scripts
                join scriptItem in _context.ScriptItems on script.Id equals scriptItem.ScriptId
                join insurance in _context.InsuranceRxes on scriptItem.InsuranceId equals insurance.Id into insuranceGroup
                from insurance in insuranceGroup.DefaultIfEmpty()
                join drug in _context.Drugs on scriptItem.DrugId equals drug.Id
                join drugClass in _context.DrugClasses on drug.Id equals drugClass.DrugId
                join classItem in _context.ClassInfos on drugClass.ClassId equals classItem.Id into classGroup
                from classItem in classGroup.DefaultIfEmpty()
                join classTypeEntity in _context.ClassTypes on classItem.ClassTypeId equals classTypeEntity.Id into classTypeGroup
                from classTypeEntity in classTypeGroup.DefaultIfEmpty()
                join branch in _context.Branches on script.BranchId equals branch.Id into branchGroup
                from branch in branchGroup.DefaultIfEmpty()
                join classInsurance in _context.ClassInsurances
                    on new
                    {
                        InsuranceId = insurance != null ? (int?)insurance.Id : null,
                        ClassId = classItem != null ? (int?)classItem.Id : null,
                        Year = script.Date.Year,
                        Month = script.Date.Month,
                        BranchId = branch != null ? (int?)branch.Id : null
                    }
                    equals new
                    {
                        InsuranceId = (int?)classInsurance.InsuranceId,
                        ClassId = (int?)classInsurance.ClassInfoId,
                        Year = classInsurance.Date.Year,
                        Month = classInsurance.Date.Month,
                        BranchId = (int?)classInsurance.BranchId
                    }
                    into classInsuranceGroup
                from classInsurance in classInsuranceGroup.DefaultIfEmpty()
                join bestDrug in _context.Drugs on classInsurance.DrugId equals bestDrug.Id into bestDrugGroup
                from bestDrug in bestDrugGroup.DefaultIfEmpty()
                join user in _context.Users on script.UserId equals user.Id into userGroup
                from user in userGroup.DefaultIfEmpty()
                join prescriber in _context.Users on scriptItem.UserEmail equals prescriber.Email into prescriberGroup
                from prescriber in prescriberGroup.DefaultIfEmpty()
                    // Filter by ClassType.Name
                where classItem == null || classTypeEntity.Name == classType
                let prevMonth = script.Date.AddMonths(-1)
                let bestNetEntryPrevMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) &&
                                 ci.ClassInfoId == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == prevMonth.Year &&
                                 ci.Date.Month == prevMonth.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntryCurrentMonth = _context.ClassInsurances
                    .Where(ci => ci.InsuranceId == (insurance != null ? insurance.Id : 0) &&
                                 ci.ClassInfoId == (classItem != null ? classItem.Id : 0) &&
                                 ci.Date.Year == script.Date.Year &&
                                 ci.Date.Month == script.Date.Month)
                    .OrderByDescending(ci => ci.BestNet)
                    .FirstOrDefault()
                let bestNetEntry = bestNetEntryPrevMonth ?? bestNetEntryCurrentMonth
                let bestScript = bestNetEntryPrevMonth != null
                    ? _context.Scripts.FirstOrDefault(s => s.ScriptCode == bestNetEntry.ScriptCode)
                    : null
                select new AuditReadDto
                {
                    RemainingStock = scriptItem.RemainingStock,
                    HighestRemainingStock = scriptItem.RemainingStock,
                    Date = script.Date,
                    ScriptCode = script.ScriptCode,
                    RxNumber = scriptItem.RxNumber,
                    User = user != null ? user.Name : "Unknown",
                    DrugName = drug != null ? drug.Name : null,
                    NDCCode = drug != null ? drug.NDC : null,
                    DrugId = drug != null ? drug.Id : 0,
                    HighestDrugName = bestDrug != null ? bestDrug.Name : null,
                    HighestDrugNDC = bestDrug != null ? bestDrug.NDC : null,
                    HighestDrugId = bestDrug != null ? bestDrug.Id : 0,
                    BranchCode = branch != null ? branch.Name : null,
                    InsuranceRx = insurance != null ? insurance.RxGroup : null,
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
                    HighestNet = bestNetEntry != null ? bestNetEntry.BestNet : 0,
                    HighestScriptCode = bestNetEntry != null ? bestNetEntry.ScriptCode : null,
                    HighestScriptDate = bestNetEntry != null ? bestNetEntry.ScriptDateTime : DateTime.MinValue
                }
            ).ToListAsync();

            return auditData;
        }

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
                    Net = di.Net,
                    date = di.Date,
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
                join prescriber in _context.Users on scriptItem.UserEmail equals prescriber.Email into prescriberGroup
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

                join drugClass in _context.DrugClasses
                    on drug.Id equals drugClass.DrugId
                join classInfo in _context.ClassInfos
                    on drugClass.ClassId equals classInfo.Id

                join db in _context.DrugBranches
                    on drug.NDC equals db.DrugNDC

                where classInfo.Id == classId && db.BranchId == branchId

                select drug
            ).ToListAsync();

            return items;
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
        internal async Task<ICollection<DrugMediReadDto>> GetAllMediDrugs(int classId)
        {
            var items = await (
                from drugmedi in _context.DrugMedis
                join drug in _context.Drugs on drugmedi.DrugId equals drug.Id
                join drugClass in _context.DrugClasses on drug.Id equals drugClass.DrugId
                join classInfo in _context.ClassInfos on drugClass.ClassId equals classInfo.Id
                where classInfo.Id == classId
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


        internal async Task<ICollection<Drug>> GetDrugsByInsuranceNamePaginated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            int offset = (pageNumber - 1) * pageSize;

            // Enable fuzzy search
            await _context.Database.ExecuteSqlRawAsync("SET pg_trgm.similarity_threshold = 0.3;");

            var sql = @"
WITH ranked AS (
    SELECT d.*,
           ROW_NUMBER() OVER (
               PARTITION BY d.""name_unaccent""
               ORDER BY (
                   similarity(d.""name_unaccent"", unaccent({1})) * 0.5 +
                   ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) * 0.3 +
                   CASE WHEN d.""name_soundex"" = soundex(unaccent({1})) THEN 0.1 ELSE 0 END +
                   CASE WHEN d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%' THEN 0.1 ELSE 0 END
               ) DESC
           ) AS rn,
           similarity(d.""name_unaccent"", unaccent({1})) AS sim,
           ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) AS ts_rank
    FROM ""DrugInsurances"" di
    INNER JOIN ""Drugs"" d ON di.""DrugId"" = d.""Id""
    INNER JOIN ""InsuranceRxes"" rx ON di.""InsuranceId"" = rx.""Id""
    WHERE LOWER(rx.""RxGroup"") = LOWER({0})
      AND (
          d.""name_unaccent"" % unaccent({1}) OR
          d.""name_tsv"" @@ plainto_tsquery(unaccent({1})) OR
          d.""name_soundex"" = soundex(unaccent({1})) OR
          d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%'
      )
)
SELECT *
FROM ranked
WHERE rn = 1
ORDER BY sim DESC, ts_rank DESC
LIMIT {2} OFFSET {3};
";

            var drugs = await _context.Drugs
                .FromSqlRaw(sql, insurance, drugName, pageSize, offset)
                .AsNoTracking()
                .ToListAsync();

            return drugs;
        }

        internal async Task<ICollection<Drug>> GetDrugsByPCNPaginated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            int offset = (pageNumber - 1) * pageSize;

            await _context.Database.ExecuteSqlRawAsync("SET pg_trgm.similarity_threshold = 0.2;");

            var sql = @"
WITH ranked AS (
    SELECT d.*,
           ROW_NUMBER() OVER (
               PARTITION BY d.""name_unaccent""
               ORDER BY (
                   similarity(d.""name_unaccent"", unaccent({1})) * 0.5 +
                   ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) * 0.3 +
                   CASE WHEN d.""name_soundex"" = soundex(unaccent({1})) THEN 0.1 ELSE 0 END +
                   CASE WHEN d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%' THEN 0.1 ELSE 0 END
               ) DESC
           ) AS rn,
           similarity(d.""name_unaccent"", unaccent({1})) AS sim,
           ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) AS ts_rank
    FROM ""DrugInsurances"" di
    INNER JOIN ""Drugs"" d ON di.""DrugId"" = d.""Id""
    INNER JOIN ""InsuranceRxes"" rx ON di.""InsuranceId"" = rx.""Id""
    INNER JOIN ""InsurancePCNs"" pcn ON rx.""InsurancePCNId"" = pcn.""Id""
    WHERE LOWER(pcn.""PCN"") = LOWER({0})
      AND (
          d.""name_unaccent"" % unaccent({1}) OR
          d.""name_tsv"" @@ plainto_tsquery(unaccent({1})) OR
          d.""name_soundex"" = soundex(unaccent({1})) OR
          d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%'
      )
)
SELECT *
FROM ranked
WHERE rn = 1
ORDER BY sim DESC, ts_rank DESC
LIMIT {2} OFFSET {3};
";

            var drugs = await _context.Drugs
                .FromSqlRaw(sql, insurance, drugName, pageSize, offset)
                .AsNoTracking()
                .ToListAsync();

            return drugs;
        }



        internal async Task<ICollection<Drug>> GetDrugsByBINPaginated(
    string insurance, string drugName, int pageSize, int pageNumber)
        {
            int offset = (pageNumber - 1) * pageSize;

            await _context.Database.ExecuteSqlRawAsync("SET pg_trgm.similarity_threshold = 0.2;");

            var sql = @"
WITH ranked AS (
    SELECT d.*,
           di.""Net"",
           di.""InsurancePayment"",
           ROW_NUMBER() OVER (
               PARTITION BY d.""name_unaccent""
               ORDER BY (
                   similarity(d.""name_unaccent"", unaccent({1})) * 0.5 +
                   ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) * 0.3 +
                   CASE WHEN d.""name_soundex"" = soundex(unaccent({1})) THEN 0.1 ELSE 0 END +
                   CASE WHEN d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%' THEN 0.1 ELSE 0 END
               ) DESC,
               di.""Net"" DESC,
               di.""InsurancePayment"" DESC
           ) AS rn,
           similarity(d.""name_unaccent"", unaccent({1})) AS sim,
           ts_rank(d.""name_tsv"", plainto_tsquery(unaccent({1}))) AS ts_rank
    FROM ""DrugInsurances"" di
    INNER JOIN ""Drugs"" d ON di.""DrugId"" = d.""Id""
    INNER JOIN ""InsuranceRxes"" rx ON di.""InsuranceId"" = rx.""Id""
    INNER JOIN ""InsurancePCNs"" pcn ON rx.""InsurancePCNId"" = pcn.""Id""
    INNER JOIN ""Insurances"" i ON pcn.""InsuranceId"" = i.""Id""
    WHERE LOWER(i.""Bin"") = LOWER({0})
      AND (
          d.""name_unaccent"" % unaccent({1}) OR
          d.""name_tsv"" @@ plainto_tsquery(unaccent({1})) OR
          d.""name_soundex"" = soundex(unaccent({1})) OR
          d.""name_unaccent"" ILIKE '%' || unaccent({1}) || '%'
      )
)
SELECT *
FROM ranked
WHERE rn = 1
ORDER BY sim DESC, ts_rank DESC, ""Net"" DESC, ""InsurancePayment"" DESC
LIMIT {2} OFFSET {3};
";

            var drugs = await _context.Drugs
                .FromSqlRaw(sql, insurance, drugName, pageSize, offset)
                .AsNoTracking()
                .ToListAsync();

            return drugs;
        }

        internal async Task<ICollection<DrugModal>> GetDrugClassesByInsuranceNamePaginated(
            string insurance,
            string drugClassName,
            int pageSize,
            int pageNumber,
            string classType = "ClassV1"
        )
        {
            var query =
                from di in _context.DrugInsurances
                join drug in _context.Drugs on di.DrugId equals drug.Id
                join dc in _context.DrugClasses on drug.Id equals dc.DrugId
                join ci in _context.ClassInfos on dc.ClassId equals ci.Id
                join ct in _context.ClassTypes on ci.ClassTypeId equals ct.Id
                join ins in _context.InsuranceRxes on di.InsuranceId equals ins.Id
                where EF.Functions.ILike(ins.RxGroup, insurance)
                   && EF.Functions.ILike(ci.Name, $"%{drugClassName}%")
                   && EF.Functions.ILike(ct.Name, classType)
                orderby di.Net descending, di.InsurancePayment descending
                select new
                {
                    Drug = drug,
                    DrugInsurance = di,
                    ClassInfo = ci,
                    ClassType = ct
                };

            // Run SQL and materialize in memory
            var rawResults = await query.ToListAsync();

            // Group by ClassId and select the highest Net per class
            var groupedResults = rawResults
                .GroupBy(x => x.ClassInfo.Id)
                .Select(g => g.First())
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = groupedResults
                .Select(x => new DrugModal
                {
                    Id = x.Drug.Id,
                    Name = x.Drug.Name,
                    Ndc = x.Drug.NDC,
                    Form = x.Drug.Form,
                    Strength = x.Drug.Strength,
                    ClassId = x.ClassInfo.Id,
                    ClassType = x.ClassType.Name,
                    ClassName = x.ClassInfo.Name,
                    Acq = x.Drug.ACQ,
                    Awp = x.Drug.AWP,
                    Rxcui = x.Drug.Rxcui ?? 0,
                    Route = x.Drug.Route,
                    TeCode = x.Drug.TECode,
                    Ingrdient = x.Drug.Ingrdient,
                    ApplicationNumber = x.Drug.ApplicationNumber,
                    ApplicationType = x.Drug.ApplicationType,
                    StrengthUnit = x.Drug.StrengthUnit,
                    Type = x.Drug.Type
                })
                .ToList();

            return result;
        }

        internal async Task<ICollection<DrugModal>> GetDrugClassesByPCNPaginated(
            string insurance,
            string drugClassName,
            int pageSize,
            int pageNumber,
            string classType = "ClassV1")
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize > 0 ? pageSize : 10;

            // Step 1: Query all matching rows ordered by Net + InsurancePayment
            var query =
                from di in _context.DrugInsurances
                join drug in _context.Drugs on di.DrugId equals drug.Id
                join dc in _context.DrugClasses on drug.Id equals dc.DrugId
                join ci in _context.ClassInfos on dc.ClassId equals ci.Id
                join ins in _context.InsuranceRxes on di.InsuranceId equals ins.Id
                join pcn in _context.InsurancePCNs on ins.InsurancePCNId equals pcn.Id
                join ct in _context.ClassTypes on ci.ClassTypeId equals ct.Id
                where EF.Functions.ILike(pcn.PCN, insurance)
                   && EF.Functions.ILike(ci.Name, $"%{drugClassName}%")
                   && EF.Functions.ILike(ct.Name, classType)
                orderby di.Net descending, di.InsurancePayment descending
                select new
                {
                    Drug = drug,
                    DrugInsurance = di,
                    ClassInfo = ci,
                    ClassType = ct
                };

            var rawResults = await query.ToListAsync();

            // Step 2: Distinct by ClassInfo.Id — in memory
            var distinctResults = rawResults
                .GroupBy(x => x.ClassInfo.Id)
                .Select(g => g.First()) // take best (highest Net) drug for each class
                .OrderBy(x => x.ClassInfo.Id) // stable ordering
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugModal
                {
                    Id = x.Drug.Id,
                    Name = x.Drug.Name,
                    Ndc = x.Drug.NDC,
                    Form = x.Drug.Form,
                    Strength = x.Drug.Strength,
                    ClassId = x.ClassInfo.Id,
                    ClassType = x.ClassType.Name,
                    ClassName = x.ClassInfo.Name,
                    Acq = x.Drug.ACQ,
                    Awp = x.Drug.AWP,
                    Rxcui = x.Drug.Rxcui ?? 0,
                    Route = x.Drug.Route,
                    TeCode = x.Drug.TECode,
                    Ingrdient = x.Drug.Ingrdient,
                    ApplicationNumber = x.Drug.ApplicationNumber,
                    ApplicationType = x.Drug.ApplicationType,
                    StrengthUnit = x.Drug.StrengthUnit,
                    Type = x.Drug.Type
                })
                .ToList();

            return distinctResults;
        }

        internal async Task<ICollection<DrugModal>> GetDrugClassesByBINPaginated(
            string insurance,
            string drugClassName,
            int pageSize,
            int pageNumber,
            string classType = "ClassV1")
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize > 0 ? pageSize : 10;

            // Step 1: query all matching rows, ordered
            var query =
                from di in _context.DrugInsurances
                join drug in _context.Drugs on di.DrugId equals drug.Id
                join dc in _context.DrugClasses on drug.Id equals dc.DrugId
                join ci in _context.ClassInfos on dc.ClassId equals ci.Id
                join ct in _context.ClassTypes on ci.ClassTypeId equals ct.Id
                join ins in _context.InsuranceRxes on di.InsuranceId equals ins.Id
                join pcn in _context.InsurancePCNs on ins.InsurancePCNId equals pcn.Id
                join insMain in _context.Insurances on pcn.InsuranceId equals insMain.Id
                where EF.Functions.ILike(insMain.Bin, insurance)
                   && EF.Functions.ILike(ci.Name, $"%{drugClassName}%")
                   && EF.Functions.ILike(ct.Name, classType)
                orderby di.Net descending, di.InsurancePayment descending
                select new
                {
                    Drug = drug,
                    DrugInsurance = di,
                    ClassInfo = ci,
                    ClassType = ct
                };

            var rawResults = await query.ToListAsync();

            // Step 2: distinct by ClassInfo.Id — in memory
            var distinctResults = rawResults
                .GroupBy(x => x.ClassInfo.Id)
                .Select(g => g.First()) // pick best drug for each class
                .OrderBy(x => x.ClassInfo.Id) // stable order
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugModal
                {
                    Id = x.Drug.Id,
                    Name = x.Drug.Name,
                    Ndc = x.Drug.NDC,
                    Form = x.Drug.Form,
                    Strength = x.Drug.Strength,
                    ClassId = x.ClassInfo.Id,
                    ClassType = x.ClassType.Name,
                    ClassName = x.ClassInfo.Name,
                    Acq = x.Drug.ACQ,
                    Awp = x.Drug.AWP,
                    Rxcui = x.Drug.Rxcui ?? 0,
                    Route = x.Drug.Route,
                    TeCode = x.Drug.TECode,
                    Ingrdient = x.Drug.Ingrdient,
                    ApplicationNumber = x.Drug.ApplicationNumber,
                    ApplicationType = x.Drug.ApplicationType,
                    StrengthUnit = x.Drug.StrengthUnit,
                    Type = x.Drug.Type
                })
                .ToList();

            return distinctResults;
        }


        internal async Task<IEnumerable<Drug>> GetDrugsByClassId(int classId, string classType, int pageSize, int pageNumber)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize > 0 ? pageSize : 10;

            var query = from drug in _context.Drugs
                        join drugClass in _context.DrugClasses on drug.Id equals drugClass.DrugId
                        join classInfo in _context.ClassInfos on drugClass.ClassId equals classInfo.Id
                        join classTypeEntity in _context.ClassTypes on classInfo.ClassTypeId equals classTypeEntity.Id
                        where classInfo.Id == classId && classTypeEntity.Name == classType
                        select drug;

            var pagedResults = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return pagedResults;
        }
        internal async Task<int> ClearClassNames()
        {
            var classes = await _context.ClassInfos
                                         .Where(x => x.ClassTypeId == 2)
                                         .ToListAsync();

            string[] patternsToRemove = {
        @"\s?\(ONT\)",
        @"\s?\(CRE\)",
        @"\s?\(TAB\)",
        @"\s?\(CAP\)"
    };

            int cleanedCount = 0;

            foreach (var classInfo in classes)
            {
                string originalName = classInfo.Name;
                string cleanedName = originalName;

                foreach (var pattern in patternsToRemove)
                {
                    cleanedName = Regex.Replace(cleanedName, pattern, "", RegexOptions.IgnoreCase);
                }

                cleanedName = cleanedName.Trim();

                if (cleanedName != originalName)
                {
                    classInfo.Name = cleanedName;
                    cleanedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return cleanedCount;
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