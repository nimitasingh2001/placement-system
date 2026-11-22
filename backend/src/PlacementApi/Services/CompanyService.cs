using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;

namespace PlacementApi.Services;

public class CompanyService : ICompanyService
{
    private readonly PlacementDbContext _db;

    public CompanyService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<List<CompanyDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new CompanyDto(c.CompanyId, c.CompanyName, c.Industry, c.Location, c.Website))
            .ToListAsync(ct);
    }

    public async Task<CompanyDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.CompanyId == id && c.IsActive)
            .Select(c => new CompanyDto(c.CompanyId, c.CompanyName, c.Industry, c.Location, c.Website))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct)
    {
        var company = new Company
        {
            CompanyName = request.CompanyName.Trim(),
            Industry = request.Industry?.Trim(),
            Location = request.Location.Trim(),
            Website = request.Website.Trim(),
            IsActive = true
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(ct);   // runs the INSERT and fills in CompanyId

        return new CompanyDto(company.CompanyId, company.CompanyName, company.Industry, company.Location, company.Website);
    }
}