using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companies;

    public CompaniesController(ICompanyService companies)
    {
        _companies = companies;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll(CancellationToken ct)
        => await _companies.GetAllAsync(ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id, CancellationToken ct)
    {
        var company = await _companies.GetByIdAsync(id, ct);
        return company is null ? NotFound() : company;
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CreateCompanyRequest request, CancellationToken ct)
    {
        var created = await _companies.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.CompanyId }, created);
    }
}