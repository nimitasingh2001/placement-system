using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;

namespace PlacementApi.Services;

public interface IJobService
{
    Task<PagedResult<JobListItemDto>> ListAsync(int? companyId, string? status, string? search, int page, int pageSize, CancellationToken ct);
    Task<JobDto> GetAsync(int id, CancellationToken ct);
    Task<JobDto> CreateAsync(SaveJobRequest request, CancellationToken ct);
    Task<JobDto> UpdateAsync(int id, SaveJobRequest request, CancellationToken ct);
    Task CloseAsync(int id, CancellationToken ct);
}

public class JobService : IJobService
{
    private readonly PlacementDbContext _db;

    public JobService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<JobListItemDto>> ListAsync(
        int? companyId, string? status, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Jobs.AsNoTracking().AsQueryable();

        if (companyId is not null)
            query = query.Where(j => j.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(j => j.Title.Contains(search));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(j => j.Deadline)
            .ThenBy(j => j.JobId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobListItemDto(
                j.JobId, j.Title, j.CompanyId, j.Company.CompanyName, j.Location,
                j.MinCgpa, j.PackageLpa, j.Deadline, j.Status))
            .ToListAsync(ct);

        return new PagedResult<JobListItemDto>(items, page, pageSize, total);
    }

    public async Task<JobDto> GetAsync(int id, CancellationToken ct)
    {
        var job = await _db.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.JobId == id, ct)
            ?? throw new ApiException(404, "Job not found.");

        return ToDto(job);
    }

    public async Task<JobDto> CreateAsync(SaveJobRequest request, CancellationToken ct)
    {
        await ValidateAsync(request, ct);

        if (request.Deadline!.Value < DateOnly.FromDateTime(DateTime.Today))
            throw new ApiException(400, "The deadline must be today or later.");

        var job = new Job
        {
            CompanyId = request.CompanyId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            MinCgpa = request.MinCgpa,
            PackageLpa = request.PackageLpa,
            Deadline = request.Deadline.Value,
            Status = "Open",
            JobSkills = request.Skills
                .Select(s => new JobSkill { SkillId = s.SkillId, IsMandatory = s.IsMandatory })
                .ToList()
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(ct);   // inserts the job, then its skills, in ONE transaction

        return await GetAsync(job.JobId, ct);
    }

    public async Task<JobDto> UpdateAsync(int id, SaveJobRequest request, CancellationToken ct)
    {
        await ValidateAsync(request, ct);

        var job = await _db.Jobs
            .Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.JobId == id, ct)
            ?? throw new ApiException(404, "Job not found.");

        job.CompanyId = request.CompanyId;
        job.Title = request.Title.Trim();
        job.Description = request.Description.Trim();
        job.Location = request.Location.Trim();
        job.MinCgpa = request.MinCgpa;
        job.PackageLpa = request.PackageLpa;
        job.Deadline = request.Deadline!.Value;

        // compare the old skill list with the new one and change only the difference
        var wanted = request.Skills.ToDictionary(s => s.SkillId);
        foreach (var existing in job.JobSkills.ToList())
        {
            if (wanted.TryGetValue(existing.SkillId, out var input))
            {
                existing.IsMandatory = input.IsMandatory;   // keep it, maybe change the flag
                wanted.Remove(existing.SkillId);
            }
            else
            {
                _db.JobSkills.Remove(existing);             // no longer required
            }
        }
        foreach (var added in wanted.Values)                // whatever is left is new
            job.JobSkills.Add(new JobSkill { SkillId = added.SkillId, IsMandatory = added.IsMandatory });

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task CloseAsync(int id, CancellationToken ct)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == id, ct)
            ?? throw new ApiException(404, "Job not found.");

        job.Status = "Closed";
        await _db.SaveChangesAsync(ct);
    }

    // checks that need the database; plain format checks are already done by the [attributes] on the request
    private async Task ValidateAsync(SaveJobRequest request, CancellationToken ct)
    {
        var companyOk = await _db.Companies
            .AnyAsync(c => c.CompanyId == request.CompanyId && c.IsActive, ct);
        if (!companyOk)
            throw new ApiException(400, "The company does not exist or is inactive.");

        var skillIds = request.Skills.Select(s => s.SkillId).ToList();
        if (skillIds.Distinct().Count() != skillIds.Count)
            throw new ApiException(400, "A skill is listed more than once.");

        var found = await _db.Skills.CountAsync(s => skillIds.Contains(s.SkillId) && s.IsActive, ct);
        if (found != skillIds.Count)
            throw new ApiException(400, "One or more skills do not exist or are inactive.");
    }

    private static JobDto ToDto(Job j) => new(
        j.JobId, j.CompanyId, j.Company.CompanyName, j.Title, j.Description, j.Location,
        j.MinCgpa, j.PackageLpa, j.Deadline, j.Status,
        j.JobSkills
            .OrderByDescending(s => s.IsMandatory)
            .ThenBy(s => s.Skill.SkillName)
            .Select(s => new JobSkillDto(s.SkillId, s.Skill.SkillName, s.IsMandatory))
            .ToList());
}