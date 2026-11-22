using Microsoft.EntityFrameworkCore;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;

namespace PlacementApi.Services;

public interface IStudentService
{
    Task<PagedResult<StudentListItemDto>> ListAsync(string? search, int? graduationYear, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<StudentDto> GetAsync(int id, CancellationToken ct);
    Task<StudentDto> CreateAsync(SaveStudentRequest request, CancellationToken ct);
    Task<StudentDto> UpdateAsync(int id, SaveStudentRequest request, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
}

public class StudentService : IStudentService
{
    private readonly PlacementDbContext _db;

    public StudentService(PlacementDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<StudentListItemDto>> ListAsync(
        string? search, int? graduationYear, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Students.AsNoTracking().AsQueryable();

        if (graduationYear is not null)
            query = query.Where(s => s.GraduationYear == graduationYear);
        if (isActive is not null)
            query = query.Where(s => s.IsActive == isActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.RollNo.Contains(term) ||
                s.FirstName.Contains(term) ||
                (s.LastName != null && s.LastName.Contains(term)) ||
                s.Email.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.RollNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentListItemDto(
                s.StudentId, s.RollNo, s.FirstName, s.LastName, s.Email, s.Phone,
                s.Course, s.Cgpa, s.GraduationYear, s.IsActive,
                _db.AppUsers.Any(u => u.StudentId == s.StudentId)))   // becomes a SQL EXISTS(...) subquery
            .ToListAsync(ct);

        return new PagedResult<StudentListItemDto>(items, page, pageSize, total);
    }

    public async Task<StudentDto> GetAsync(int id, CancellationToken ct)
    {
        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.StudentSkills).ThenInclude(ss => ss.Skill)
            .FirstOrDefaultAsync(s => s.StudentId == id, ct)
            ?? throw new ApiException(404, "Student not found.");

        var hasAccount = await _db.AppUsers.AnyAsync(u => u.StudentId == id, ct);

        return new StudentDto(
            student.StudentId, student.RollNo, student.FirstName, student.LastName, student.Email, student.Phone,
            student.Course, student.Cgpa, student.GraduationYear, student.IsActive, hasAccount,
            student.StudentSkills
                .OrderBy(ss => ss.Skill.SkillName)
                .Select(ss => new StudentSkillDto(ss.SkillId, ss.Skill.SkillName, ss.Proficiency))
                .ToList());
    }

    public async Task<StudentDto> CreateAsync(SaveStudentRequest request, CancellationToken ct)
    {
        var rollNo = request.RollNo.Trim();
        var email = request.Email.Trim();

        // friendly message first; the UNIQUE constraints in the database remain the safety net
        if (await _db.Students.AnyAsync(s => s.RollNo == rollNo || s.Email == email, ct))
            throw new ApiException(409, "A student with this roll number or email already exists.");

        var student = new Student
        {
            RollNo = rollNo,
            FirstName = request.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Course = request.Course.Trim(),
            Cgpa = request.Cgpa,
            GraduationYear = request.GraduationYear,
            IsActive = true
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync(ct);

        return await GetAsync(student.StudentId, ct);
    }

    public async Task<StudentDto> UpdateAsync(int id, SaveStudentRequest request, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == id, ct)
            ?? throw new ApiException(404, "Student not found.");

        var rollNo = request.RollNo.Trim();
        var email = request.Email.Trim();

        if (await _db.Students.AnyAsync(s => s.StudentId != id && (s.RollNo == rollNo || s.Email == email), ct))
            throw new ApiException(409, "Another student already has this roll number or email.");

        var emailChanged = !string.Equals(student.Email, email, StringComparison.OrdinalIgnoreCase);
        if (emailChanged && await _db.AppUsers.AnyAsync(u => u.StudentId == id, ct))
            throw new ApiException(409, "This student already has a login, so the email cannot be changed.");

        student.RollNo = rollNo;
        student.FirstName = request.FirstName.Trim();
        student.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim();
        student.Email = email;
        student.Phone = request.Phone.Trim();
        student.Course = request.Course.Trim();
        student.Cgpa = request.Cgpa;
        student.GraduationYear = request.GraduationYear;
        student.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == id, ct)
            ?? throw new ApiException(404, "Student not found.");

        student.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }
}