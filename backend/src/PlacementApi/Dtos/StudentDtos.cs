using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public record StudentSkillDto(int SkillId, string SkillName, string Proficiency);

public record StudentListItemDto(
    int StudentId, string RollNo, string FirstName, string? LastName, string Email, string Phone,
    string Course, decimal Cgpa, int GraduationYear, bool IsActive, bool HasAccount);

public record StudentDto(
    int StudentId, string RollNo, string FirstName, string? LastName, string Email, string Phone,
    string Course, decimal Cgpa, int GraduationYear, bool IsActive, bool HasAccount,
    List<StudentSkillDto> Skills);

public class SaveStudentRequest
{
    [Required, StringLength(20)]
    public string RollNo { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\+?[0-9]{7,14}$", ErrorMessage = "Phone must be 7 to 14 digits, with an optional leading +.")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Course { get; set; } = string.Empty;

    [Range(0, 10)]
    public decimal Cgpa { get; set; }

    [Range(2000, 2100)]
    public int GraduationYear { get; set; }

    public bool IsActive { get; set; } = true;   // used when updating (to reactivate); a new student is always active
}