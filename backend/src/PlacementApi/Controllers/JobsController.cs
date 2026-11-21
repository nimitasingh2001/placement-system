using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobs;

    public JobsController(IJobService jobs)
    {
        _jobs = jobs;
    }

    // admin overview of every job; students get their own filtered lists in Step 6
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<JobListItemDto>>> List(
        [FromQuery] int? companyId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await _jobs.ListAsync(companyId, status, search, page, pageSize, ct);
    }

    // any logged-in user can open one job (students need its description and required skills)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobDto>> Get(int id, CancellationToken ct)
        => await _jobs.GetAsync(id, ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JobDto>> Create(SaveJobRequest request, CancellationToken ct)
    {
        var job = await _jobs.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = job.JobId }, job);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JobDto>> Update(int id, SaveJobRequest request, CancellationToken ct)
        => await _jobs.UpdateAsync(id, request, ct);

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        await _jobs.CloseAsync(id, ct);
        return NoContent();
    }
}