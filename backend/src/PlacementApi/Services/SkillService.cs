using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;

namespace PlacementApi.Services;

public interface ISkillService
{
    Task<List<SkillDto>> ListAsync(bool includeInactive, CancellationToken ct);
    Task<SkillDto> CreateAsync(SaveSkillRequest request, CancellationToken ct);
    Task<SkillDto> UpdateAsync(int id, SaveSkillRequest request, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
}

public class SkillService : ISkillService
{
    private readonly PlacementDbContext _db;

    public SkillService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<List<SkillDto>> ListAsync(bool includeInactive, CancellationToken ct)
    {
        var query = _db.Skills.AsNoTracking().AsQueryable();
        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Category).ThenBy(s => s.SkillName)
            .Select(s => new SkillDto(s.SkillId, s.SkillName, s.Category, s.IsActive))
            .ToListAsync(ct);
    }

    public async Task<SkillDto> CreateAsync(SaveSkillRequest request, CancellationToken ct)
    {
        var skill = new Skill
        {
            SkillName = request.SkillName.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim()
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync(ct);   // a duplicate name breaks UQ_Skill_SkillName, which the handler turns into a 409

        return new SkillDto(skill.SkillId, skill.SkillName, skill.Category, skill.IsActive);
    }

    public async Task<SkillDto> UpdateAsync(int id, SaveSkillRequest request, CancellationToken ct)
    {
        var skill = await _db.Skills.FirstOrDefaultAsync(s => s.SkillId == id, ct)
            ?? throw new ApiException(404, "Skill not found.");

        skill.SkillName = request.SkillName.Trim();
        skill.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        skill.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return new SkillDto(skill.SkillId, skill.SkillName, skill.Category, skill.IsActive);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var skill = await _db.Skills.FirstOrDefaultAsync(s => s.SkillId == id, ct)
            ?? throw new ApiException(404, "Skill not found.");

        skill.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }
}