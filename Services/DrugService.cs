using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class DrugService(DrugRepository _drugRepository)
    {
        public async Task Procces()
        {
            await _drugRepository.SaveData();
        }
        public async Task Procces2()
        {
             await _drugRepository.ImportDrugInsuranceAsync();
        }
        public async Task<ICollection<Drug>> SearchName( string name )
        {
            var items = await _drugRepository.GetDrugsByName(name);
            return items;
        }

        public async Task<ICollection<Insurance>> GetDrugInsurances(string name)
        {
            var items = await _drugRepository.GetDrugInsurances(name);
            return items;
        }

        public async Task<ICollection<string>> GetDrugNDCs(string name)
        {
            var items = await _drugRepository.GetAllNDCByDrugName(name);
            return items;
        }

    }
}