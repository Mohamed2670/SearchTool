using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Models; // Log model
using SearchTool_ServerSide.Data; // DbContext

namespace SearchTool_ServerSide.Middleware
{
    public class UserLogsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserLogsMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UserLogsMiddleware(RequestDelegate next, ILogger<UserLogsMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;

        }

        public async Task Invoke(HttpContext context)
        {
            // Immediately check and mark the request as processed
            if (context.Items.ContainsKey("UserLogProcessed"))
            {
                Console.WriteLine("here1");
                await _next(context);
                return;
            }
            context.Items["UserLogProcessed"] = true;

            // Retrieve the user ID from the token
            var userIdStr = GetUserIdFromToken(context);
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            // Process the request first
            await _next(context);
            // Console.WriteLine("here");
            // Log only for authenticated users
            if (userId.HasValue)
            {
                var requestPath = context.Request.Path;
                var method = context.Request.Method;
                var responseStatus = context.Response.StatusCode;
                var endpointName = requestPath.HasValue ? requestPath.Value.Split('/').Last() : "Unknown";
                if(endpointName=="token-test")
                {
                    return;
                }
                _logger.LogInformation($"User: {userId.Value} requested {requestPath} with {method} method and got {responseStatus} response");
                // Save the log to the database
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SearchToolDBContext>();
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                    if (user == null)
                    {
                        _logger.LogWarning($"User with ID {userId.Value} not found in the database");
                        return;
                    }
                    var log = new Log
                    {
                        UserId = userId.Value,
                        Date = DateTime.UtcNow,
                        Action = "User requested " + endpointName 
                    };

                    dbContext.Logs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
            }
        }



        private string GetUserIdFromToken(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null) return null;

            return jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        }
    }
}
