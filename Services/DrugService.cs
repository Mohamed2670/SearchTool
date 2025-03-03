using AutoMapper;
using SearchTool_ServerSide.Dtos;
using SearchTool_ServerSide.Dtos.DrugDtos;
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
        public async Task<ICollection<Drug>> SearchName(string name)
        {
            var items = await _drugRepository.GetDrugsByName(name);
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

        internal async Task<DrugInsurance> GetDetails(string ndc, int insuranceId)
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
            var items = await _drugRepository.GetAllLatestScripts();

            return items;
        }
        internal async Task<ICollection<DrugsAlternativesReadDto>> GetAllDrugs(int classId)
        {
            var items = await _drugRepository.GetAllDrugs(classId);
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

        internal async Task<ICollection<DrugInsurance>> GetInsuranceByNdc(string ndc)
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

        internal async Task<Insurance> GetInsuranceDetails(string shortName)
        {
            var item = await _drugRepository.GetInsuranceDetails(shortName);
            return item;
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
         internal async Task<ICollection<Insurance>> GetInsurances(string insruance)
        {
            var items = await _drugRepository.GetInsurances(insruance);
            return items;
        }
    }
}