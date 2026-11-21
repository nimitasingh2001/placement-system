-- =============================================================================
-- 03_views.sql
-- Five reusable views. A view stores no data; it is a saved SELECT.
-- Ordering is left to the query that reads the view (views cannot ORDER BY).
-- Student name is built from FirstName/LastName (LastName is optional).
-- =============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PlacementDb;
GO

-- 1. One row per placement with every name needed (the four-table join, written once)
CREATE OR ALTER VIEW vw_PlacementDetails AS
SELECT p.PlacementId,
       p.AcademicYear,
       p.PlacementDate,
       p.PackageLpa,
       s.StudentId,
       s.RollNo,
       CONCAT(s.FirstName, ' ' + s.LastName) AS StudentName,
       s.Course,
       s.GraduationYear,
       c.CompanyId,
       c.CompanyName,
       j.JobId,
       j.Title AS JobTitle
FROM Placement p
JOIN Application a ON a.ApplicationId = p.ApplicationId
JOIN Student s     ON s.StudentId     = a.StudentId
JOIN Job j         ON j.JobId         = a.JobId
JOIN Company c     ON c.CompanyId     = j.CompanyId;
GO

-- 2. Jobs a student can currently apply to (open, not past deadline, active company)
CREATE OR ALTER VIEW vw_OpenJobs AS
SELECT j.JobId,
       j.Title,
       c.CompanyName,
       j.Location,
       j.MinCgpa,
       j.PackageLpa,
       j.Deadline
FROM Job j
JOIN Company c ON c.CompanyId = j.CompanyId
WHERE j.Status = 'Open'
  AND j.Deadline >= CAST(GETDATE() AS DATE)
  AND c.IsActive = 1;
GO

-- 3. Admin application list
CREATE OR ALTER VIEW vw_ApplicationDetails AS
SELECT a.ApplicationId,
       a.AppliedOn,
       a.Status,
       s.StudentId,
       s.RollNo,
       CONCAT(s.FirstName, ' ' + s.LastName) AS StudentName,
       s.Cgpa,
       j.JobId,
       j.Title AS JobTitle,
       c.CompanyId,
       c.CompanyName
FROM Application a
JOIN Student s ON s.StudentId = a.StudentId
JOIN Job j     ON j.JobId     = a.JobId
JOIN Company c ON c.CompanyId = j.CompanyId;
GO

-- 4. Placement percentage per batch (counts STUDENTS placed, not offers)
CREATE OR ALTER VIEW vw_PlacementStatsByBatch AS
WITH PlacedStudents AS (
    SELECT DISTINCT a.StudentId
    FROM Application a
    JOIN Placement p ON p.ApplicationId = a.ApplicationId
)
SELECT s.GraduationYear,
       COUNT(*)            AS TotalStudents,
       COUNT(ps.StudentId) AS PlacedCount,
       CAST(100.0 * COUNT(ps.StudentId) / COUNT(*) AS DECIMAL(5,1)) AS PlacementPct
FROM Student s
LEFT JOIN PlacedStudents ps ON ps.StudentId = s.StudentId
WHERE s.IsActive = 1
GROUP BY s.GraduationYear;
GO

-- 5. Hiring summary per company (counts OFFERS; built on view 1)
CREATE OR ALTER VIEW vw_CompanyHiringSummary AS
SELECT CompanyId,
       CompanyName,
       COUNT(*)                              AS Hires,
       CAST(AVG(PackageLpa) AS DECIMAL(5,2)) AS AvgPackageLpa,
       MAX(PackageLpa)                       AS MaxPackageLpa
FROM vw_PlacementDetails
GROUP BY CompanyId, CompanyName;
GO

PRINT 'Views created: 5.';
GO
