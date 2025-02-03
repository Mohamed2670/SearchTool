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
            await _drugRepository.temp2();
        }
    }
}