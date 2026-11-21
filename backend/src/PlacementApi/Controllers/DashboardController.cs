using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("batch-stats")]
    public async Task<ActionResult<List<BatchStatsDto>>> BatchStats(CancellationToken ct)
        => await _dashboard.GetBatchStatsAsync(ct);

    [HttpGet("company-hiring")]
    public async Task<ActionResult<List<CompanyHiringDto>>> CompanyHiring(CancellationToken ct)
        => await _dashboard.GetCompanyHiringAsync(ct);

    [HttpGet("application-funnel")]
    public async Task<ActionResult<List<StatusCountDto>>> ApplicationFunnel(CancellationToken ct)
        => await _dashboard.GetApplicationFunnelAsync(ct);
}