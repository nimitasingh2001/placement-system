-- =============================================================================
-- optional/queries.sql
-- The practice queries written before the views and procedures existed.
-- Each answers a requirement from the SRS. Run one at a time.
-- =============================================================================
USE PlacementDb;
GO

-- Query 1: open jobs with company name (JOIN via foreign key). Expected: 3 rows
SELECT j.JobId, j.Title, c.CompanyName, j.Location, j.PackageLpa, j.Deadline
FROM Job j
JOIN Company c ON c.CompanyId = j.CompanyId
WHERE j.Status = 'Open'
ORDER BY j.Deadline;
GO

-- Query 2: jobs a student can apply to (CGPA, open, deadline, not applied yet)
-- Expected for MCA26002: 1 row (Full Stack Developer)
SELECT j.JobId, j.Title, c.CompanyName, j.Location, j.MinCgpa
FROM Student s
JOIN Job j     ON s.Cgpa >= j.MinCgpa
JOIN Company c ON c.CompanyId = j.CompanyId
WHERE s.RollNo = 'MCA26002'
  AND j.Status = 'Open'
  AND j.Deadline >= CAST(GETDATE() AS DATE)
  AND NOT EXISTS (SELECT 1 FROM Application a
                  WHERE a.StudentId = s.StudentId AND a.JobId = j.JobId);
GO

-- Query 3: placement percentage per batch (LEFT JOIN keeps unplaced students)
-- Expected: 2024 10/6/60.0, 2025 10/6/60.0, 2026 10/5/50.0
WITH PlacedStudents AS (
    SELECT DISTINCT a.StudentId
    FROM Application a
    JOIN Placement p ON p.ApplicationId = a.ApplicationId
)
SELECT s.GraduationYear,
       COUNT(*)            AS TotalStudents,
       COUNT(ps.StudentId) AS Placed,
       CAST(100.0 * COUNT(ps.StudentId) / COUNT(*) AS DECIMAL(5,1)) AS PlacementPct
FROM Student s
LEFT JOIN PlacedStudents ps ON ps.StudentId = s.StudentId
GROUP BY s.GraduationYear
ORDER BY s.GraduationYear;
GO

-- Query 4: hires and package per company (four-table JOIN chain)
SELECT c.CompanyName,
       COUNT(*)                    AS Hires,
       ROUND(AVG(p.PackageLpa), 2) AS AvgPackageLpa,
       MAX(p.PackageLpa)           AS MaxPackageLpa
FROM Placement p
JOIN Application a ON a.ApplicationId = p.ApplicationId
JOIN Job j         ON j.JobId = a.JobId
JOIN Company c     ON c.CompanyId = j.CompanyId
GROUP BY c.CompanyName
ORDER BY Hires DESC, c.CompanyName;
GO

-- Query 5: skill match of one student across open jobs.
-- Expected for MCA26001: Full Stack 4/3/75.0, Data Analyst 3/1/33.3, Cloud Engineer 3/0/0.0
SELECT j.Title,
       COUNT(*)          AS MandatorySkills,
       COUNT(ss.SkillId) AS Matched,
       CAST(100.0 * COUNT(ss.SkillId) / COUNT(*) AS DECIMAL(5,1)) AS MatchPct
FROM Job j
JOIN JobSkill js ON js.JobId = j.JobId AND js.IsMandatory = 1
LEFT JOIN StudentSkill ss
       ON ss.SkillId = js.SkillId
      AND ss.StudentId = (SELECT StudentId FROM Student WHERE RollNo = 'MCA26001')
WHERE j.Status = 'Open'
GROUP BY j.Title
ORDER BY MatchPct DESC;
GO

-- Query 5b: which mandatory skill is missing? Expected: ASP.NET Core
SELECT sk.SkillName AS MissingSkill
FROM JobSkill js
JOIN Job j    ON j.JobId = js.JobId
JOIN Skill sk ON sk.SkillId = js.SkillId
WHERE j.Title = 'Full Stack Developer'
  AND js.IsMandatory = 1
  AND NOT EXISTS (SELECT 1
                  FROM StudentSkill ss
                  JOIN Student s ON s.StudentId = ss.StudentId
                  WHERE s.RollNo = 'MCA26001' AND ss.SkillId = js.SkillId);
GO

-- Query 6: most in-demand skills. Expected: SQL 8, Java 6, Data Structures 4, Python 3
SELECT TOP 4 sk.SkillName, COUNT(*) AS JobsRequiring
FROM JobSkill js
JOIN Skill sk ON sk.SkillId = js.SkillId
WHERE js.IsMandatory = 1
GROUP BY sk.SkillName
ORDER BY JobsRequiring DESC, sk.SkillName;
GO
