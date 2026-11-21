using PlacementApi.Dtos;

namespace PlacementApi.Services;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync(CancellationToken ct);
    Task<CompanyDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken ct);
}