using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Dtos;

namespace PlacementApi.Services;

public interface IApplicationService
{
    Task<PagedResult<ApplicationListItemDto>> ListAsync(int? jobId, string? status, int page, int pageSize, CancellationToken ct);
    Task UpdateStatusAsync(int applicationId, string newStatus, CancellationToken ct);
    Task<int> SelectAsync(int applicationId, decimal? packageLpa, CancellationToken ct);
}

public class ApplicationService : IApplicationService
{
    private readonly PlacementDbContext _db;

    public ApplicationService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ApplicationListItemDto>> ListAsync(
        int? jobId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.VwApplicationDetails.AsNoTracking().AsQueryable();

        // build the query step by step: each filter is added only when the caller asked for it
        if (jobId is not null)
            query = query.Where(a => a.JobId == jobId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.AppliedOn)
            .ThenBy(a => a.ApplicationId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApplicationListItemDto(
                a.ApplicationId, a.AppliedOn, a.Status,
                a.StudentId, a.RollNo, a.StudentName ?? string.Empty, a.Cgpa,
                a.JobId, a.JobTitle, a.CompanyId, a.CompanyName))
            .ToListAsync(ct);

        return new PagedResult<ApplicationListItemDto>(items, page, pageSize, total);
    }

    public async Task UpdateStatusAsync(int applicationId, string newStatus, CancellationToken ct)
    {
        // the interpolated values become SQL parameters (@p0, @p1), never pasted into the text
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC usp_UpdateApplicationStatus @ApplicationId = {applicationId}, @NewStatus = {newStatus}", ct);
    }

    public async Task<int> SelectAsync(int applicationId, decimal? packageLpa, CancellationToken ct)
    {
        // this procedure has an OUTPUT parameter, so the parameters are built by hand
        var appId = new SqlParameter("@ApplicationId", applicationId);
        var package = new SqlParameter("@PackageLpa", SqlDbType.Decimal)
        {
            Precision = 5,
            Scale = 2,
            Value = (object?)packageLpa ?? DBNull.Value
        };
        var placementId = new SqlParameter("@PlacementId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC usp_SelectApplication @ApplicationId, @PackageLpa, @PlacementId OUTPUT",
            new object[] { appId, package, placementId }, ct);

        return (int)placementId.Value;
    }
}