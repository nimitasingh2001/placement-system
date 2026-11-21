using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Dtos;

namespace PlacementApi.Services;

public interface IDashboardService
{
    Task<List<BatchStatsDto>> GetBatchStatsAsync(CancellationToken ct);
    Task<List<CompanyHiringDto>> GetCompanyHiringAsync(CancellationToken ct);
    Task<List<StatusCountDto>> GetApplicationFunnelAsync(CancellationToken ct);
}

public class DashboardService : IDashboardService
{
    private static readonly string[] AllStatuses = { "Applied", "Shortlisted", "Interview", "Selected", "Rejected" };

    private readonly PlacementDbContext _db;

    public DashboardService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<List<BatchStatsDto>> GetBatchStatsAsync(CancellationToken ct)
    {
        return await _db.VwPlacementStatsByBatches
            .AsNoTracking()
            .OrderBy(v => v.GraduationYear)
                        .Select(v => new BatchStatsDto(v.GraduationYear, v.TotalStudents ?? 0, v.PlacedCount ?? 0, v.PlacementPct))
            .ToListAsync(ct);
    }

    public async Task<List<CompanyHiringDto>> GetCompanyHiringAsync(CancellationToken ct)
    {
        return await _db.VwCompanyHiringSummaries
            .AsNoTracking()
            .OrderByDescending(v => v.Hires)
            .ThenBy(v => v.CompanyName)
            .Select(v => new CompanyHiringDto(v.CompanyId, v.CompanyName, v.Hires ?? 0, v.AvgPackageLpa, v.MaxPackageLpa))
            .ToListAsync(ct);
    }

    public async Task<List<StatusCountDto>> GetApplicationFunnelAsync(CancellationToken ct)
    {
        // LINQ GroupBy becomes SQL GROUP BY: SELECT Status, COUNT(*) ... GROUP BY Status
        var counts = await _db.Applications
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // a status with no applications is missing from the SQL result, so fill in zeros in C#
        return AllStatuses
            .Select(s => new StatusCountDto(s, counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0))
            .ToList();
    }
}