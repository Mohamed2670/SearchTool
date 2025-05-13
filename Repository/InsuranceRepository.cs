using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Dtos.InsuranceDtos.cs;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Repository
{
    public class InsuranceRepository : GenericRepository<Insurance>
    {
        private readonly SearchToolDBContext _context;
        private readonly IMapper _mapper;
        public InsuranceRepository(SearchToolDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
        internal async Task<InsuranceReadDto?> GetInsuranceDetails(int id)
        {
            var item = await (from rx in _context.InsuranceRxes
                              join pcn in _context.InsurancePCNs on rx.InsurancePCNId equals pcn.Id
                              join ins in _context.Insurances on pcn.InsuranceId equals ins.Id
                              where rx.Id == id
                              select new InsuranceReadDto
                              {
                                  RxGroup = rx.RxGroup,
                                  InsurancePCN = pcn.PCN,
                                  InsuranceBin = ins.Bin, // Assuming BIN exists in InsuranceRxes
                                  HelpDeskNumber = ins.HelpDeskNumber,
                                  InsuranceFullName = ins.Name
                              }).FirstOrDefaultAsync(); // Get a single result

            return item; // Return DTO or null if not found
        }
        internal async Task<InsuranceReadDto?> GetInsurancePCNDetails(int id)
        {
            var item = await (from pcn in _context.InsurancePCNs
                              join rx in _context.InsuranceRxes on pcn.Id equals rx.InsurancePCNId
                              join ins in _context.Insurances on pcn.InsuranceId equals ins.Id
                              where pcn.Id == id
                              select new InsuranceReadDto
                              {
                                  RxGroup = rx.RxGroup,
                                  InsurancePCN = pcn.PCN,
                                  InsuranceBin = ins.Bin, // Assuming BIN exists in InsuranceRxes
                                  HelpDeskNumber = ins.HelpDeskNumber,
                                  InsuranceFullName = ins.Name
                              }).FirstOrDefaultAsync(); // Get a single result

            return item; // Return DTO or null if not found
        }
        internal async Task<ICollection<InsuranceRx>> GetAllRxGroups()
        {
            var items = await _context.InsuranceRxes.ToListAsync();
            return items;
        }
        internal async Task<ICollection<InsuranceRx>> GetAllRxGroupsByPcnId(int id)
        {
            var items = await _context.InsuranceRxes.Where(x => x.InsurancePCNId == id).ToListAsync();
            return items;
        }
        internal async Task<ICollection<InsurancePCN>> GetAllPCNByBinId(int id)
        {
            var items = await _context.InsurancePCNs.Where(x => x.InsuranceId == id).ToListAsync();
            return items;
        }
        internal async Task<ICollection<Insurance>> GetAllBIN()
        {
            var items = await _context.Insurances.ToListAsync();
            return items;
        }

        internal async Task<Insurance> GetInsuranceBINDetails(int id)
        {
            return await _context.Insurances.FirstOrDefaultAsync(x => x.Id == id);
        }

        internal async Task<ICollection<InsuranceRx>> GetAllRxGroupsByBINId(int id)
        {
            var items = await _context.InsuranceRxes.Where(x => x.InsurancePCN.Insurance.Id == id).ToListAsync();
            return items;
        }

        internal async Task<ICollection<InsurancePCN>> GetAllPCNsByBINId(int id)
        {
            var items = await _context.InsurancePCNs.Where(x => x.Insurance.Id == id).ToListAsync();
            return items;
        }
        internal async Task<InsuranceRx> GetRXById(int id)
        {
            return await _context.InsuranceRxes.FirstOrDefaultAsync(x => x.Id == id);
        }
        internal async Task<InsurancePCN> GetPCNById(int id)
        {
            return await _context.InsurancePCNs.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}