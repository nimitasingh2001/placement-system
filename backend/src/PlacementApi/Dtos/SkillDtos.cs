using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public record SkillDto(int SkillId, string SkillName, string? Category, bool IsActive);

public class SaveSkillRequest
{
    [Required, StringLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}