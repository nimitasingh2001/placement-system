using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Auth;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly IStudentPortalService _portal;

    public MeController(IStudentPortalService portal)
    {
        _portal = portal;
    }

    // the student id always comes from the token, never from the request
    private int StudentId => User.GetStudentId()
        ?? throw new ApiException(403, "This account is not linked to a student.");

    [HttpGet("profile")]
    public async Task<ActionResult<StudentDto>> GetProfile(CancellationToken ct)
        => await _portal.GetProfileAsync(StudentId, ct);

    [HttpPut("profile")]
    public async Task<ActionResult<StudentDto>> UpdateProfile(UpdatePhoneRequest request, CancellationToken ct)
        => await _portal.UpdatePhoneAsync(StudentId, request.Phone, ct);

    [HttpPut("skills")]
    public async Task<ActionResult<List<StudentSkillDto>>> ReplaceSkills(ReplaceSkillsRequest request, CancellationToken ct)
        => await _portal.ReplaceSkillsAsync(StudentId, request, ct);

    [HttpGet("jobs/recommended")]
    public async Task<ActionResult<List<RecommendedJobDto>>> Recommended(CancellationToken ct)
        => await _portal.GetRecommendedJobsAsync(StudentId, ct);

    [HttpGet("jobs/{jobId:int}/skill-gap")]
    public async Task<ActionResult<SkillGapDto>> SkillGap(int jobId, CancellationToken ct)
        => await _portal.GetSkillGapAsync(StudentId, jobId, ct);

    [HttpPost("applications")]
    public async Task<ActionResult<ApplyResponse>> Apply(ApplyRequest request, CancellationToken ct)
    {
        var applicationId = await _portal.ApplyAsync(StudentId, request.JobId, ct);
        return Created("/api/me/applications", new ApplyResponse(applicationId));
    }

    [HttpGet("applications")]
    public async Task<ActionResult<List<MyApplicationDto>>> MyApplications(CancellationToken ct)
        => await _portal.GetMyApplicationsAsync(StudentId, ct);
}