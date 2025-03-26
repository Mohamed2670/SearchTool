using SearchTool_ServerSide.Dtos.InsuranceDtos.cs;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class InsuranceService(InsuranceRepository _insuranceRepository)
    {
        internal async Task<InsuranceReadDto> GetInsuranceDetails(int id)
        {
            var item = await _insuranceRepository.GetInsuranceDetails(id);
            return item;
        }
        internal async Task<ICollection<InsuranceRx>> GetAllRxGroups()
        {
            var items = await _insuranceRepository.GetAllRxGroups();
            return items;
        }
        internal async Task<ICollection<InsuranceRx>> GetAllRxGroupsByPcnId(int id)
        {
            var items = await _insuranceRepository.GetAllRxGroupsByPcnId(id);
            return items;
        }
        internal async Task<ICollection<InsurancePCN>> GetAllPCNByBinId(int id)
        {
            var items = await _insuranceRepository.GetAllPCNByBinId(id);
            return items;
        }
        internal async Task<ICollection<Insurance>> GetAllBIN()
        {
            var items = await _insuranceRepository.GetAllBIN();
            return items;
        }
        internal async Task<InsuranceReadDto?> GetInsurancePCNDetails(int id)
        {
            var item = await _insuranceRepository.GetInsurancePCNDetails(id);
            return item;
        }

        internal async Task<Insurance> GetInsuranceBINDetails(int id)
        {
            var item = await _insuranceRepository.GetInsuranceBINDetails(id);
            return item;
        }

        internal async Task<ICollection<InsuranceRx>> GetAllRxGroupsByBINId(int id)
        {
            var items = await _insuranceRepository.GetAllRxGroupsByBINId(id);
            return items;
        }

        internal async Task<ICollection<InsurancePCN>> GetAllPCNsByBINId(int id)
        {
            var items = await _insuranceRepository.GetAllPCNsByBINId(id);
            return items;
        }
    }
}