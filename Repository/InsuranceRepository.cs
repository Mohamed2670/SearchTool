using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Controllers;
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
            return await _context.InsuranceRxes.Include(x => x.InsurancePCN).ThenInclude(x => x.Insurance).FirstOrDefaultAsync(x => x.Id == id);
        }
        internal async Task<InsurancePCN> GetPCNById(int id)
        {
            return await _context.InsurancePCNs.FirstOrDefaultAsync(x => x.Id == id);
        }

        internal async Task ReportStatus(ReportStatusRequest request, string userEmail, CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            // 1) Ensure the parent InsuranceStatus exists (composite PK)
            var status = await _context.InsuranceStatuses.FindAsync(
                new object[] { request.SourceDrugNDC, request.TargetDrugNDC, request.InsuranceRxId }, ct);


            if(status == null)
{
                _context.InsuranceStatuses.Add(new InsuranceStatus
                {
                    SourceDrugNDC = request.SourceDrugNDC,
                    TargetDrugNDC = request.TargetDrugNDC,
                    InsuranceRxId = request.InsuranceRxId,
                    ApprovedStatus = "NA",
                    PriorAuthorizationStatus = "NA"
                });
                try
                {
                    await _context.SaveChangesAsync(ct);
                    // Try to fetch the status again after insert
                    status = await _context.InsuranceStatuses.FindAsync(
                        new object[] { request.SourceDrugNDC, request.TargetDrugNDC, request.InsuranceRxId }, ct);
                }
                catch (DbUpdateException)
                {
                    // Optionally inspect inner exception for unique violation codes and rethrow otherwise.
                    // For brevity we're swallowing here; the next insert uses the existing row.
                    // Try to fetch the status again in case it was inserted by another process
                    status = await _context.InsuranceStatuses.FindAsync(
                        new object[] { request.SourceDrugNDC, request.TargetDrugNDC, request.InsuranceRxId }, ct);
                }
            }

            // Only update if status is not null
            if (status != null)
            {
                var Approved = request.Status == "Approved" ? "Approved" : request.Status == "Rejected" ? "Rejected" : status.ApprovedStatus ?? "NA";
                var PriorAuthorization = request.Status == "PriorAuthorizationYes" ? "Yes" : request.Status == "PriorAuthorizationNo" ? "No" : status.PriorAuthorizationStatus ?? "NA";
                status.ApprovedStatus = Approved;
                status.PriorAuthorizationStatus = PriorAuthorization;
            }

            // 2) Append a new Report row (history)
            _context.Reports.Add(new Report
            {
                SourceDrugNDC = request.SourceDrugNDC,
                TargetDrugNDC = request.TargetDrugNDC,
                InsuranceRxId = request.InsuranceRxId,

                Status = string.IsNullOrWhiteSpace(request.Status) ? "NA" : request.Status,
                StatusDescription = "NA",
                AdditionalInfo = "NA",
                StatusDate = DateTime.UtcNow,

                UserEmail = string.IsNullOrWhiteSpace(userEmail) ? null : userEmail
            });

            await _context.SaveChangesAsync(ct);
        }
        internal async Task<IEnumerable<Report>> GetReportsAsyncByKey(string sourceDrugNDC, string TargetDrugNDC, int insuranceRxId, CancellationToken ct = default, int pageSize = 3)
        {
            return await _context.Reports
                .Where(r => r.SourceDrugNDC == sourceDrugNDC && r.TargetDrugNDC == TargetDrugNDC && r.InsuranceRxId == insuranceRxId)
                .OrderByDescending(r => r.StatusDate)
                .Skip(1)
                .Take(pageSize)
                .ToListAsync(ct);
        }
    }
}