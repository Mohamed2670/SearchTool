using AutoMapper;
using SearchTool_ServerSide.Dtos.LogDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class LogsService(LogRepository _logRepository,IMapper _mapper)
    {
        internal async Task<ICollection<LogsReadDto>> GetAllLogs(int userId)
        {
            var items = await _logRepository.GetAll(userId);
            return items;
        }

        internal async Task<IEnumerable<Log>> GetAllLogsToSharePoint()
        {
            var items = await _logRepository.GetAll();
            return items;   
        }

        internal async Task InsertAllLogsToDB(IEnumerable<AllLogsAddDto> allLogsAddDtos)
        {
            var logs = _mapper.Map<IEnumerable<Log>>(allLogsAddDtos);
            await _logRepository.InsertAllLogsToDB(logs);

        }
    }

}