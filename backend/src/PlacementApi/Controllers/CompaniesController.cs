using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Dtos;

namespace PlacementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly PlacementDbContext _db;

    public CompaniesController(PlacementDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll()
    {
        var companies = await _db.Companies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new CompanyDto(c.CompanyId, c.CompanyName, c.Industry, c.Location, c.Website))
            .ToListAsync();

        return companies;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var company = await _db.Companies
            .AsNoTracking()
            .Where(c => c.CompanyId == id && c.IsActive)
            .Select(c => new CompanyDto(c.CompanyId, c.CompanyName, c.Industry, c.Location, c.Website))
            .FirstOrDefaultAsync();

        return company is null ? NotFound() : company;
    }
}