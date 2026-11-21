using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skills;

    public SkillsController(ISkillService skills)
    {
        _skills = skills;
    }

    // everyone who is logged in can read the catalogue; only admins can also see inactive skills
    [HttpGet]
    public async Task<ActionResult<List<SkillDto>>> List([FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => await _skills.ListAsync(includeInactive && User.IsInRole("Admin"), ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SkillDto>> Create(SaveSkillRequest request, CancellationToken ct)
    {
        var created = await _skills.CreateAsync(request, ct);
        return Created($"/api/skills/{created.SkillId}", created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SkillDto>> Update(int id, SaveSkillRequest request, CancellationToken ct)
        => await _skills.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _skills.DeactivateAsync(id, ct);
        return NoContent();
    }
}