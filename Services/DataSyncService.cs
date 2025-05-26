using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;

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
}
