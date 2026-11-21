using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public record JobSkillDto(int SkillId, string SkillName, bool IsMandatory);

public record JobDto(
    int JobId, int CompanyId, string CompanyName, string Title, string Description, string Location,
    decimal MinCgpa, decimal PackageLpa, DateOnly Deadline, string Status, List<JobSkillDto> Skills);

public record JobListItemDto(
    int JobId, string Title, int CompanyId, string CompanyName, string Location,
    decimal MinCgpa, decimal PackageLpa, DateOnly Deadline, string Status);

public class JobSkillInput
{
    [Range(1, int.MaxValue)]
    public int SkillId { get; set; }

    public bool IsMandatory { get; set; } = true;
}

// one request shape for both create and update
public class SaveJobRequest
{
    [Range(1, int.MaxValue)]
    public int CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 10)]
    public decimal MinCgpa { get; set; }

    [Range(0.01, 999.99)]
    public decimal PackageLpa { get; set; }

    [Required]
    public DateOnly? Deadline { get; set; }   // nullable so that [Required] can notice a missing date

    public List<JobSkillInput> Skills { get; set; } = new();
}