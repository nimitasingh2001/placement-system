-- =============================================================================
-- optional/bulk_data.sql
-- Adds a marked bulk data set for index measurements:
--   20,000 students (roll no BULK......), 2,000 jobs ('Bulk Job n'),
--   100,000 applications, 60,000 student skills.
-- Bulk applications are never 'Selected', so no placements are created.
-- Remove it again with optional/bulk_data_cleanup.sql
-- Expected afterwards: Student 20,030 / Job 2,012 / Application 100,042 / StudentSkill 60,090
-- =============================================================================
USE PlacementDb;
GO
SET XACT_ABORT ON;
BEGIN TRAN;

-- 20,000 students
WITH nums AS (
    SELECT TOP (20000) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO Student (RollNo, FirstName, LastName, Email, Phone, Course, Cgpa, GraduationYear)
SELECT 'BULK' + RIGHT('000000' + CAST(n AS NVARCHAR(6)), 6),
       'Bulk',
       'Student' + CAST(n AS NVARCHAR(10)),
       'bulk' + CAST(n AS NVARCHAR(10)) + '@example.com',
       '+91' + RIGHT('0000000000' + CAST(n AS NVARCHAR(10)), 10),
       'MCA',
       CAST(5 + (n % 500) / 100.0 AS DECIMAL(4,2)),
       2022 + (n % 5)
FROM nums;

-- 2,000 jobs spread over the existing companies (5% open, the rest closed)
WITH nums AS (
    SELECT TOP (2000) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
),
comp AS (
    SELECT CompanyId, ROW_NUMBER() OVER (ORDER BY CompanyId) AS rn FROM Company
)
INSERT INTO Job (CompanyId, Title, Description, Location, MinCgpa, PackageLpa, Deadline, Status)
SELECT comp.CompanyId,
       'Bulk Job ' + CAST(nums.n AS NVARCHAR(10)),
       'Bulk test job',
       'Delhi',
       6.0,
       3 + (nums.n % 10),
       CASE WHEN nums.n % 20 = 0
            THEN DATEADD(DAY, 30 + (nums.n % 60), CAST(GETDATE() AS DATE))
            ELSE DATEADD(DAY, -(30 + (nums.n % 600)), CAST(GETDATE() AS DATE)) END,
       CASE WHEN nums.n % 20 = 0 THEN 'Open' ELSE 'Closed' END
FROM nums
JOIN comp ON comp.rn = (nums.n % (SELECT COUNT(*) FROM Company)) + 1;

-- 100,000 applications: 5 distinct jobs per bulk student
WITH bs AS (
    SELECT StudentId, CAST(ROW_NUMBER() OVER (ORDER BY StudentId) AS INT) AS n
    FROM Student WHERE RollNo LIKE 'BULK%'
),
bj AS (
    SELECT JobId, CAST(ROW_NUMBER() OVER (ORDER BY JobId) AS INT) AS m
    FROM Job WHERE Title LIKE 'Bulk Job %'
),
k AS (SELECT 0 AS k UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4)
INSERT INTO Application (StudentId, JobId, AppliedOn, Status)
SELECT bs.StudentId,
       bj.JobId,
       DATEADD(DAY, -((bs.n + k.k) % 400), CAST(GETDATE() AS DATE)),
       CASE (bs.n + k.k) % 4 WHEN 0 THEN 'Applied' WHEN 1 THEN 'Shortlisted' WHEN 2 THEN 'Interview' ELSE 'Rejected' END
FROM bs CROSS JOIN k
JOIN bj ON bj.m = ((bs.n * 7 + k.k * 397) % 2000) + 1;

-- 60,000 student skills: 3 distinct skills per bulk student
WITH bs AS (
    SELECT StudentId, CAST(ROW_NUMBER() OVER (ORDER BY StudentId) AS INT) AS n
    FROM Student WHERE RollNo LIKE 'BULK%'
),
sk AS (
    SELECT SkillId, CAST(ROW_NUMBER() OVER (ORDER BY SkillId) AS INT) AS r FROM Skill
),
k AS (SELECT 0 AS k UNION ALL SELECT 1 UNION ALL SELECT 2)
INSERT INTO StudentSkill (StudentId, SkillId, Proficiency)
SELECT bs.StudentId,
       sk.SkillId,
       CASE (bs.n + k.k) % 3 WHEN 0 THEN 'Beginner' WHEN 1 THEN 'Intermediate' ELSE 'Advanced' END
FROM bs CROSS JOIN k
JOIN sk ON sk.r = ((bs.n * 3 + k.k * 5) % 15) + 1;

COMMIT;
GO

UPDATE STATISTICS Application;
UPDATE STATISTICS StudentSkill;
UPDATE STATISTICS Job;
GO

SELECT 'Student' AS TableName, COUNT(*) AS Rows FROM Student
UNION ALL SELECT 'Job',          COUNT(*) FROM Job
UNION ALL SELECT 'Application',  COUNT(*) FROM Application
UNION ALL SELECT 'StudentSkill', COUNT(*) FROM StudentSkill;
GO
