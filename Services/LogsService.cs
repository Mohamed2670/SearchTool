using SearchTool_ServerSide.Dtos.LogDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class LogsService(LogRepository _logRepository)
    {
        internal async Task<ICollection<LogsReadDto>> GetAllLogs(int userId)
        {
            var items = await _logRepository.GetAll(userId);
            return items;
        }
    }

}