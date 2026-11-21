namespace PlacementApi.Dtos;

public record BatchStatsDto(int GraduationYear, int TotalStudents, int PlacedCount, decimal? PlacementPct);

public record CompanyHiringDto(int CompanyId, string CompanyName, int Hires, decimal? AvgPackageLpa, decimal? MaxPackageLpa);

public record StatusCountDto(string Status, int Count);