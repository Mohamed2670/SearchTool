using System.Diagnostics;
using AutoMapper;
using SearchTool_ServerSide.Dtos;
using SearchTool_ServerSide.Dtos.DrugDtos;
using SearchTool_ServerSide.Dtos.InsuranceDtos.cs;
using SearchTool_ServerSide.Dtos.ScritpsDto;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;
using ServerSide.Models;

namespace SearchTool_ServerSide.Services
{
    public class DrugService(DrugRepository _drugRepository, IMapper _mapper)
    {
        public async Task Procces()
        {
            await _drugRepository.SaveData();
        }
        public async Task Procces2()
        {
            await _drugRepository.ImportDrugInsuranceAsync();
        }
        public async Task<ICollection<Drug>> SearchName(string name, int pageNumber, int pageSize)
        {
            var items = await _drugRepository.GetDrugsByName(name, pageNumber, pageSize);
            return items;
        }

        public async Task<ICollection<Drug>> GetClassesByName(string name, int pageNumber, int pageSize)
        {
            var items = await _drugRepository.GetClassesByName(name, pageNumber, pageSize);
            return items;
        }

        // public async Task<ICollection<Insurance>> GetDrugInsurances(string name)
        // {
        //     var items = await _drugRepository.GetDrugInsurances(name);
        //     return items;
        // }

        public async Task<ICollection<string>> GetDrugNDCs(string name)
        {
            Console.WriteLine("here : ");
            var items = await _drugRepository.GetAllNDCByDrugName(name);
            return items;
        }

        public async Task<DrugInsurance> GetBySelection(string name, string ndc, string insuranceName)
        {
            var item = await _drugRepository.GetBySelection(name, ndc, insuranceName);
            return item;
        }

        internal async Task<ICollection<DrugInsurance>> GetAltrantives(string className, int insuranceId)
        {
            var items = await _drugRepository.GetAltrantives(className, insuranceId);
            return items;

        }



        internal async Task<Drug> SearchByIdNdc(int id, string ndc)
        {
            var item = await _drugRepository.SearchByIdNdc(id, ndc);
            return item;
        }

        internal async Task<Drug> SearchByNdc(string ndc)
        {
            var item = await _drugRepository.GetDrugByNdc(ndc);
            return item;
        }

        internal async Task<DrugsAlternativesReadDto> GetDetails(string ndc, int insuranceId)
        {
            var item = await _drugRepository.GetDetails(ndc, insuranceId);
            return item;
        }

        // internal async Task<ICollection<string>> getDrugNDCsByNameInsuance(string drugName, int insurnaceId)
        // {
        //     var items = await _drugRepository.getDrugNDCsByNameInsuance(drugName, insurnaceId);
        //     return items;
        // }

        internal async Task<DrugClass> getClassbyId(int id)
        {
            var item = await _drugRepository.getClassbyId(id);
            return item;
        }

        internal async Task<ICollection<Drug>> GetDrugsByClass(int classId)
        {
            var items = await _drugRepository.GetDrugsByClass(classId);
            return items;
        }

        internal async Task<ICollection<DrugInsurance>> GetAllLatest()
        {
            var items = await _drugRepository.GetAllLatest();
            return items;
        }
        internal async Task<ICollection<AuditReadDto>> GetAllLatestScripts()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var items = await _drugRepository.GetAllLatestScripts();
            stopwatch.Stop();
            Console.WriteLine("Total time taken : " + stopwatch.ElapsedMilliseconds);
            return items;
        }
        internal async Task<ICollection<AuditReadDto>> GetAllLatestScriptsPaginated(int page = 1, int pageSize = 1000, int classVersion = 1)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var items = await _drugRepository.GetAllLatestScriptsPaginated(page, pageSize, classVersion);
            stopwatch.Stop();
            Console.WriteLine("Total time taken : " + stopwatch.ElapsedMilliseconds);
            return items;
        }

        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugs(int classId)
        {
            var items = await _drugRepository.GetAllDrugs(classId);
            return items;
        }
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV2(int classId)
        {
            var items = await _drugRepository.GetAllDrugsV2(classId);
            return items;
        }
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV3(int classId)
        {
            var items = await _drugRepository.GetAllDrugsV3(classId);
            return items;
        }
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV4(int classId)
        {
            var items = await _drugRepository.GetAllDrugsV4(classId);
            return items;
        }
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugsV2Insu(int classId)
        {
            var items = await _drugRepository.GetAllDrugsV2Insu(classId);
            return items;
        }

        internal async Task<Drug> GetDrugById(int id)
        {
            var item = await _drugRepository.GetDrugById(id);
            return item;
        }

        // internal async Task oneway()
        // {
        //     await _drugRepository.oneway();
        // }

        internal async Task<ICollection<DrugInsuranceReadDto>> GetInsuranceByNdc(string ndc)
        {
            var items = await _drugRepository.GetInsuranceByNdc(ndc);
            return items;

        }

        internal async Task<ICollection<ScriptItemDto>> GetScriptByScriptCode(string scriptCode)
        {
            var items = await _drugRepository.GetScriptByScriptCode(scriptCode);
            return items;
        }
        internal async Task ImportInsurancesFromCsvAsync()
        {
            await _drugRepository.ImportInsurancesFromCsvAsync();
        }


        internal async Task<ICollection<Drug>> GetDrugsByClassBranch(int classId, int branchId)
        {
            var items = await _drugRepository.GetDrugsByClassBranch(classId, branchId);
            return items;
        }
        internal async Task<Script> GetScriptAsync(string scriptCode)
        {
            var item = await _drugRepository.GetScriptAsync(scriptCode);
            return item;
        }

        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAlternativesByClassIdBranchId(int classId, int branchId)
        {
            var items = await _drugRepository.GetAlternativesByClassIdBranchId(classId, branchId);
            return items;
        }

        internal async Task<ICollection<Drug>> GetDrugsByInsurance(int insuranceId, string drug)
        {
            var items = await _drugRepository.GetDrugsByInsurance(insuranceId, drug);
            return items;
        }
        internal async Task<ICollection<Drug>> GetDrugsByInsurance(string insruance)
        {
            var items = await _drugRepository.GetDrugsByInsurance(insruance);
            return items;
        }
        internal async Task<ICollection<Drug>> GetDrugsByInsuranceNameDrugName(string insuranceName, string drugName, int pageSize = 1000, int pageNumber = 1)
        {
            var items = await _drugRepository.GetDrugsByInsuranceNameDrugName(insuranceName, drugName, pageSize, pageNumber);
            return items;
        }
        internal async Task<ICollection<Drug>> GetDrugsByPCN(string pcn)
        {
            var items = await _drugRepository.GetDrugsByPCN(pcn);
            return items;
        }
        internal async Task<ICollection<Drug>> GetDrugsByBIN(string bin)
        {
            var items = await _drugRepository.GetDrugsByBIN(bin);
            return items;
        }
        internal async Task<ICollection<Insurance>> GetInsurances(string insruance)
        {
            var items = await _drugRepository.GetInsurances(insruance);
            return items;
        }

        internal async Task<ICollection<Insurance>> GetInsurancesBinsByName(string bin)
        {
            var items = await _drugRepository.GetInsurancesBinsByName(bin);
            return items;
        }
        internal async Task<ICollection<InsurancePCN>> GetInsurancesPcnByBinId(int binId)
        {
            var items = await _drugRepository.GetInsurancesPcnByBinId(binId);
            return items;
        }
        internal async Task<ICollection<InsuranceRx>> GetInsurancesRxByPcnId(int pcnId)
        {
            var items = await _drugRepository.GetInsurancesRxByPcnId(pcnId);
            return items;
        }



        internal async Task<IEnumerable<Drug>> GetAll()
        {
            var items = await _drugRepository.GetAll();
            return items;
        }
        internal async Task<IEnumerable<Drug>> GetAllV2()
        {
            var items = await _drugRepository.GetAllV2();
            return items;
        }

        public async Task<ICollection<AuditReadDto>> GetLatestScriptsByMonthYear(int month, int year)
        {
            var items = await _drugRepository.GetLatestScriptsByMonthYear(month, year);
            return items;
        }
        public async Task AddScripts(ICollection<ScriptAddDto> scriptAddDtos)
        {
            await _drugRepository.AddScripts(scriptAddDtos);
        }

        internal async Task<DrugBestAlternativeReadDto> GetBestAlternativeByNDCRxGroupId(int classId, int rxGroupId)
        {
            var items = await _drugRepository.GetBestAlternativeByNDCRxGroupId(classId, rxGroupId);
            return items;
        }
        internal async Task AddMediCare()
        {
            await _drugRepository.AddMediCare();
        }

        internal async Task<ICollection<DrugMediReadDto>> GetAllMediDrugs(int classId)
        {
            var items = await _drugRepository.GetAllMediDrugs(classId);
            return items;
        }

        internal async Task<ICollection<Drug>> GetDrugsByInsuranceNamePagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            return await _drugRepository.GetDrugsByInsuranceNamePagintated(insurance, drugName, pageSize, pageNumber);
        }
        internal async Task<ICollection<Drug>> GetDrugsByPCNPagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            return await _drugRepository.GetDrugsByPCNPagintated(insurance, drugName, pageSize, pageNumber);
        }
        internal async Task<ICollection<Drug>> GetDrugsByBINPagintated(string insurance, string drugName, int pageSize, int pageNumber)
        {
            return await _drugRepository.GetDrugsByBINPagintated(insurance, drugName, pageSize, pageNumber);
        }

        internal async Task<ICollection<Drug>> GetDrugClassesByInsuranceNamePagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var items = await _drugRepository.GetDrugClassesByInsuranceNamePagintated(insurance, drugClassName, pageSize, pageNumber);
            return items;
        }

        internal async Task<ICollection<Drug>> GetDrugClassesByPCNPagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var items = await _drugRepository.GetDrugClassesByPCNPagintated(insurance, drugClassName, pageSize, pageNumber);
            return items;
        }

        internal async Task<ICollection<Drug>> GetDrugClassesByBINPagintated(string insurance, string drugClassName, int pageSize, int pageNumber)
        {
            var items = await _drugRepository.GetDrugClassesByBINPagintated(insurance, drugClassName, pageSize, pageNumber);
            return items;
        }

        internal async Task<ICollection<FullDrugReadDto>> GetAllDrugsWithClassNames()
        {
            var drugs = await _drugRepository.GetAllDrugsWithClassNames();
            return drugs;
        }
    }
}