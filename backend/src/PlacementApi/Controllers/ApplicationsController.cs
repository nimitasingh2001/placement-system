using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applications;

    public ApplicationsController(IApplicationService applications)
    {
        _applications = applications;
    }

    // GET /api/applications?jobId=10&status=Interview&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<ApplicationListItemDto>>> List(
        [FromQuery] int? jobId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);   // never let a caller ask for a million rows
        return await _applications.ListAsync(jobId, status, page, pageSize, ct);
    }

    // PATCH /api/applications/5/status   { "status": "Shortlisted" }
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request, CancellationToken ct)
    {
        await _applications.UpdateStatusAsync(id, request.Status, ct);
        return NoContent();
    }

    // POST /api/applications/5/select   { "packageLpa": 6.5 }  or  {}
    [HttpPost("{id:int}/select")]
    public async Task<ActionResult<SelectApplicationResponse>> Select(
        int id, SelectApplicationRequest? request, CancellationToken ct)
    {
        var placementId = await _applications.SelectAsync(id, request?.PackageLpa, ct);
        return new SelectApplicationResponse(placementId);
    }
}