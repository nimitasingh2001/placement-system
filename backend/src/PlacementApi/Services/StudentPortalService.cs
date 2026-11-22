using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;

namespace PlacementApi.Services;

public interface IStudentPortalService
{
    Task<StudentDto> GetProfileAsync(int studentId, CancellationToken ct);
    Task<StudentDto> UpdatePhoneAsync(int studentId, string phone, CancellationToken ct);
    Task<List<StudentSkillDto>> ReplaceSkillsAsync(int studentId, ReplaceSkillsRequest request, CancellationToken ct);
    Task<List<RecommendedJobDto>> GetRecommendedJobsAsync(int studentId, CancellationToken ct);
    Task<SkillGapDto> GetSkillGapAsync(int studentId, int jobId, CancellationToken ct);
    Task<int> ApplyAsync(int studentId, int jobId, CancellationToken ct);
    Task<List<MyApplicationDto>> GetMyApplicationsAsync(int studentId, CancellationToken ct);
}

public class StudentPortalService : IStudentPortalService
{
    private readonly PlacementDbContext _db;
    private readonly IStudentService _students;

    public StudentPortalService(PlacementDbContext db, IStudentService students)
    {
        _db = db;
        _students = students;
    }

    public Task<StudentDto> GetProfileAsync(int studentId, CancellationToken ct)
        => _students.GetAsync(studentId, ct);

    public async Task<StudentDto> UpdatePhoneAsync(int studentId, string phone, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new ApiException(404, "Student not found.");

        student.Phone = phone.Trim();
        await _db.SaveChangesAsync(ct);

        return await _students.GetAsync(studentId, ct);
    }

    public async Task<List<StudentSkillDto>> ReplaceSkillsAsync(int studentId, ReplaceSkillsRequest request, CancellationToken ct)
    {
        var ids = request.Skills.Select(s => s.SkillId).ToList();
        if (ids.Distinct().Count() != ids.Count)
            throw new ApiException(400, "A skill is listed more than once.");

        var found = await _db.Skills.CountAsync(s => ids.Contains(s.SkillId) && s.IsActive, ct);
        if (found != ids.Count)
            throw new ApiException(400, "One or more skills do not exist or are inactive.");

        var student = await _db.Students
            .Include(s => s.StudentSkills)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new ApiException(404, "Student not found.");

        // same compare-and-change approach as the job skills
        var wanted = request.Skills.ToDictionary(s => s.SkillId);
        foreach (var existing in student.StudentSkills.ToList())
        {
            if (wanted.TryGetValue(existing.SkillId, out var input))
            {
                existing.Proficiency = input.Proficiency;
                wanted.Remove(existing.SkillId);
            }
            else
            {
                _db.StudentSkills.Remove(existing);
            }
        }
        foreach (var added in wanted.Values)
            student.StudentSkills.Add(new StudentSkill { SkillId = added.SkillId, Proficiency = added.Proficiency });

        await _db.SaveChangesAsync(ct);

        return (await _students.GetAsync(studentId, ct)).Skills;
    }

    public async Task<List<RecommendedJobDto>> GetRecommendedJobsAsync(int studentId, CancellationToken ct)
    {
        // your view and your function, joined in SQL; {studentId} becomes a parameter
        return await _db.Database.SqlQuery<RecommendedJobDto>($@"
            SELECT o.JobId, o.Title, o.CompanyName, o.Location, o.MinCgpa, o.PackageLpa, o.Deadline,
                   m.MandatorySkills, m.MatchedSkills, m.MatchPct,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM Application a
                                          WHERE a.StudentId = s.StudentId AND a.JobId = o.JobId)
                             THEN 1 ELSE 0 END AS bit) AS AlreadyApplied
            FROM Student s
            CROSS APPLY dbo.fn_StudentJobMatch(s.StudentId) m
            JOIN dbo.vw_OpenJobs o ON o.JobId = m.JobId AND s.Cgpa >= o.MinCgpa
            WHERE s.StudentId = {studentId}")
            .OrderByDescending(r => r.MatchPct)      // ordering is added here in LINQ, not inside the raw SQL
            .ThenBy(r => r.Deadline)
            .ToListAsync(ct);
    }

    public async Task<SkillGapDto> GetSkillGapAsync(int studentId, int jobId, CancellationToken ct)
    {
        var title = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.JobId == jobId)
            .Select(j => j.Title)
            .FirstOrDefaultAsync(ct)
            ?? throw new ApiException(404, "Job not found.");

        var missing = await _db.Database
            .SqlQuery<SkillRefDto>($"SELECT SkillId, SkillName FROM dbo.fn_MissingSkills({studentId}, {jobId})")
            .OrderBy(m => m.SkillName)
            .ToListAsync(ct);

        return new SkillGapDto(jobId, title, missing);
    }

    public async Task<int> ApplyAsync(int studentId, int jobId, CancellationToken ct)
    {
        // every rule (CGPA, deadline, open job, duplicate, active student) lives in usp_ApplyForJob
        var student = new SqlParameter("@StudentId", studentId);
        var job = new SqlParameter("@JobId", jobId);
        var applicationId = new SqlParameter("@ApplicationId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC usp_ApplyForJob @StudentId, @JobId, @ApplicationId OUTPUT",
            new object[] { student, job, applicationId }, ct);

        return (int)applicationId.Value;
    }

    public async Task<List<MyApplicationDto>> GetMyApplicationsAsync(int studentId, CancellationToken ct)
    {
        return await _db.VwApplicationDetails
            .AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AppliedOn)
            .ThenByDescending(a => a.ApplicationId)
            .Select(a => new MyApplicationDto(
                a.ApplicationId, a.AppliedOn, a.Status, a.JobId, a.JobTitle, a.CompanyName,
                // the offered package, only for selected applications (a subquery on Placement)
                _db.Placements
                    .Where(p => p.ApplicationId == a.ApplicationId)
                    .Select(p => (decimal?)p.PackageLpa)
                    .FirstOrDefault()))
            .ToListAsync(ct);
    }
}