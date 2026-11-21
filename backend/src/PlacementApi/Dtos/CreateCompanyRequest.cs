using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public class CreateCompanyRequest
{
    [Required, StringLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Industry { get; set; }

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(200), Url]
    public string Website { get; set; } = string.Empty;
}