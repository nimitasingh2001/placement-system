using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public record ApplicationListItemDto(
    int ApplicationId,
    DateOnly AppliedOn,
    string Status,
    int StudentId,
    string RollNo,
    string StudentName,
    decimal Cgpa,
    int JobId,
    string JobTitle,
    int CompanyId,
    string CompanyName);

public class UpdateStatusRequest
{
    [Required, StringLength(15)]
    public string Status { get; set; } = string.Empty;
}

public class SelectApplicationRequest
{
    [Range(0.01, 999.99)]
    public decimal? PackageLpa { get; set; }   // leave empty to use the job's advertised package
}

public record SelectApplicationResponse(int PlacementId);