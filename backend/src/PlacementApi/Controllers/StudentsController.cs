using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
    {
        _students = students;
    }

    // GET /api/students?search=sharma&graduationYear=2026&isActive=true&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentListItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] int? graduationYear,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await _students.ListAsync(search, graduationYear, isActive, page, pageSize, ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> Get(int id, CancellationToken ct)
        => await _students.GetAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(SaveStudentRequest request, CancellationToken ct)
    {
        var student = await _students.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = student.StudentId }, student);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentDto>> Update(int id, SaveStudentRequest request, CancellationToken ct)
        => await _students.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _students.DeactivateAsync(id, ct);
        return NoContent();
    }
}