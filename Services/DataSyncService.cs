using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models; // Add this if Log is in Models namespace

public class DataSyncService
{
    private readonly GlobalDBContext _globalDb;
    private readonly SearchToolDBContext _localDb;

    public DataSyncService(GlobalDBContext globalDb, SearchToolDBContext localDb)
    {
        _globalDb = globalDb;
        _localDb = localDb;
    }

    public async Task SyncUsersAsync()
    {
        var globalUsers = await _globalDb.Users.AsNoTracking().ToListAsync();
        var localUsers = await _localDb.Users.ToListAsync();
        var emailToUserMap = localUsers.ToDictionary(u => u.Email, u => u);
        var localBranchIds = new HashSet<int>(await _localDb.Branches.Select(b => b.Id).ToListAsync());

        foreach (var globalUser in globalUsers)
        {
            // Skip if the branch does not exist in local DB
            if (!localBranchIds.Contains(globalUser.BranchId))
                continue;

            if (emailToUserMap.TryGetValue(globalUser.Email, out var existingLocalUser))
            {
                // Update existing user
                _localDb.Entry(existingLocalUser).CurrentValues.SetValues(globalUser);
            }
            else
            {
                // Insert new user — remove Id to let EF generate one
                globalUser.Id = 0;
                await _localDb.Users.AddAsync(globalUser);
            }
        }

        await _localDb.SaveChangesAsync();
    }

    public async Task SyncLogsAsync()
    {
        var globalLogs = await _globalDb.Logs
            .Include(l => l.User) // Ensure User info is loaded (for email)
            .AsNoTracking()
            .ToListAsync();

        var localUsers = await _localDb.Users
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        var emailToUserId = localUsers.ToDictionary(u => u.Email, u => u.Id);

        // HashSet for faster lookup
        var localLogIds = new HashSet<int>(await _localDb.Logs.Select(l => l.Id).ToListAsync());

        int skipped = 0, inserted = 0, updated = 0;

        foreach (var log in globalLogs)
        {
            if (log.User == null || string.IsNullOrWhiteSpace(log.User.Email))
            {
                skipped++;
                continue;
            }

            if (!emailToUserId.TryGetValue(log.User.Email, out int resolvedUserId))
            {
                skipped++;
                continue;
            }

            log.UserId = resolvedUserId;
            log.User = null;

            if (localLogIds.Contains(log.Id))
            {
                // Existing: update
                var existing = await _localDb.Logs.FindAsync(log.Id);
                if (existing != null)
                {
                    _localDb.Entry(existing).CurrentValues.SetValues(log);
                    updated++;
                }
            }
            else
            {
                // New: reset Id to avoid PK conflict
                log.Id = 0; // ✅ Let EF generate a new local ID
                await _localDb.Logs.AddAsync(log);
                inserted++;
            }
        }

        await _localDb.SaveChangesAsync();

        Console.WriteLine($"Logs Sync - Inserted: {inserted}, Updated: {updated}, Skipped: {skipped}");
    }

    public async Task SyncUsersAndLogsAsync()
    {
        await SyncUsersAsync();
        await SyncLogsAsync();
    }

    // Placeholder for Excel sync logic
    internal async Task<byte[]> GetLogsCsvAsync()
    {
        var logs = await _localDb.Logs
            .AsNoTracking()
            .Include(l => l.User) // Ensure User info is loaded (for email)
            .ToListAsync();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
        };

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, config))
        {
            await csv.WriteRecordsAsync(logs);
        }
        return memoryStream.ToArray();
    }

    internal async Task<byte[]> SyncUserByExcel()
    {
        var logs = await _localDb.Users
           .AsNoTracking()
           .ToListAsync();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
        };

        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, config))
        {
            await csv.WriteRecordsAsync(logs);
        }
        return memoryStream.ToArray();
    }


    public class SimpleLogImportDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }

        public string Action { get; set; }
        public DateTime Date { get; set; }
    }

    public async Task ImportLogsFromCsvWithoutIdAsync(string filePath = "logs (5).csv")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Encoding = Encoding.UTF8,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        var localUsers = await _localDb.Users
                  .Select(u => new { u.Email, u.Id })
                  .ToListAsync();
        var logs = new List<SimpleLogImportDto>();

        await foreach (var record in csv.GetRecordsAsync<SimpleLogImportDto>())
        {
            // Always normalize to UTC for comparison and storage
            record.Date = record.Date.Kind == DateTimeKind.Utc
                ? record.Date
                : DateTime.SpecifyKind(record.Date, DateTimeKind.Utc);
            logs.Add(record);
        }

        // Remove duplicates from CSV by UserId, Action, Date (all in UTC)
        var uniqueLogs = logs
            .GroupBy(l => new { l.UserId, l.Action, l.Date })
            .Select(g => g.First())
            .ToList();

        // Also normalize existing DB dates to UTC for comparison
        var existing = await _localDb.Logs
            .Select(l => new { l.UserId, l.Action, l.Date })
            .ToListAsync();
        var existingSet = new HashSet<(int, string, DateTime)>(
            existing.Select(e =>
                (e.UserId, e.Action, e.Date.Kind == DateTimeKind.Utc
                    ? e.Date
                    : DateTime.SpecifyKind(e.Date, DateTimeKind.Utc)))
        );

        var newLogEntities = uniqueLogs
        .Select(l =>
        {
            var user = localUsers.FirstOrDefault(u => u.Email == l.Email);
            if (user == null)
            {
                // Optionally log or collect skipped emails here
                return null;
            }
            return new Log
            {
                UserId = user.Id,
                Action = l.Action,
                Date = l.Date // Already UTC
            };
        })
        .Where(log => log != null)
        .ToList();

        await _localDb.Logs.AddRangeAsync(newLogEntities);
        await _localDb.SaveChangesAsync();
    }



}
