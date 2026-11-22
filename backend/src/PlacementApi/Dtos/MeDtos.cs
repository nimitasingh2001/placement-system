using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public class UpdatePhoneRequest
{
    [Required, RegularExpression(@"^\+?[0-9]{7,14}$", ErrorMessage = "Phone must be 7 to 14 digits, with an optional leading +.")]
    public string Phone { get; set; } = string.Empty;
}

public class StudentSkillInput
{
    [Range(1, int.MaxValue)]
    public int SkillId { get; set; }

    [Required, RegularExpression("^(Beginner|Intermediate|Advanced)$", ErrorMessage = "Proficiency must be Beginner, Intermediate or Advanced.")]
    public string Proficiency { get; set; } = string.Empty;
}

public class ReplaceSkillsRequest
{
    [MaxLength(50)]
    public List<StudentSkillInput> Skills { get; set; } = new();
}

public class ApplyRequest
{
    [Range(1, int.MaxValue)]
    public int JobId { get; set; }
}

public record ApplyResponse(int ApplicationId);

// classes with settable properties, because they are filled straight from raw SQL results
public class RecommendedJobDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal MinCgpa { get; set; }
    public decimal PackageLpa { get; set; }
    public DateOnly Deadline { get; set; }
    public int MandatorySkills { get; set; }
    public int MatchedSkills { get; set; }
    public decimal MatchPct { get; set; }
    public bool AlreadyApplied { get; set; }
}

public class SkillRefDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
}

public record SkillGapDto(int JobId, string Title, List<SkillRefDto> MissingMandatorySkills);

public record MyApplicationDto(
    int ApplicationId, DateOnly AppliedOn, string Status,
    int JobId, string JobTitle, string CompanyName, decimal? OfferedPackageLpa);