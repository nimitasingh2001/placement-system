-- =============================================================================
-- 07_seed_part2.sql
-- Development data, part 2: 12 jobs, 45 job-skill rows, 90 student-skill rows,
-- 42 applications and 17 placements (generated from the Selected applications).
--
-- Requires 06_seed_part1.sql first. Re-runnable: it clears and reloads only
-- Placement, Application, JobSkill, StudentSkill and Job.
-- IDs are looked up by name (roll number, job title, skill name), never hard-coded.
-- =============================================================================
USE PlacementDb;
GO

-- Part 1 must have been loaded
IF (SELECT COUNT(*) FROM Student) < 30 OR (SELECT COUNT(*) FROM Skill) < 15 OR (SELECT COUNT(*) FROM Company) < 6
    THROW 50091, 'Run 06_seed_part1.sql first.', 1;
GO

DELETE FROM Placement;
DELETE FROM Application;
DELETE FROM JobSkill;
DELETE FROM StudentSkill;
DELETE FROM Job;
GO

-- Restart ID counters (only for tables whose counter has been used)
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'DBCC CHECKIDENT (''dbo.' + t.name + N''', RESEED, 0) WITH NO_INFOMSGS;' + CHAR(10)
FROM sys.identity_columns ic
JOIN sys.tables t ON t.object_id = ic.object_id
WHERE ic.last_value IS NOT NULL
  AND t.name IN (N'Placement', N'Application', N'Job');
EXEC sp_executesql @sql;
GO

SET XACT_ABORT ON;
BEGIN TRAN;

-- 1. JOBS (9 closed from past years, 3 open)
INSERT INTO Job (CompanyId, Title, Description, Location, MinCgpa, PackageLpa, Deadline, Status)
SELECT c.CompanyId, v.Title, v.Description, v.Location, v.MinCgpa, v.PackageLpa, CAST(v.Deadline AS DATE), v.Status
FROM (VALUES
 ('Infosys',   'Systems Engineer',            'Entry-level systems role: build and support enterprise applications.', 'Bengaluru', 6.5, 4.0, '2024-02-28', 'Closed'),
 ('TCS',       'Assistant System Engineer',   'Develop and test client applications as part of a delivery team.',     'Mumbai',    6.0, 3.6, '2024-03-15', 'Closed'),
 ('Wipro',     'Project Engineer',            'Support software projects across development and testing.',            'Bengaluru', 6.5, 3.8, '2024-03-31', 'Closed'),
 ('Infosys',   'Specialist Programmer',       'Solve complex coding problems and build high-performance components.','Bengaluru', 7.5, 9.5, '2025-02-20', 'Closed'),
 ('HCLTech',   'Software Developer',          'Develop business applications using .NET and SQL Server.',             'Noida',     7.0, 5.0, '2025-03-10', 'Closed'),
 ('Zoho',      'Member Technical Staff',      'Design and build product features in a small engineering team.',       'Chennai',   7.5, 7.0, '2025-03-25', 'Closed'),
 ('Accenture', 'Associate Software Engineer', 'Work on client projects, writing and testing application code.',       'Gurugram',  6.5, 4.5, '2026-02-15', 'Closed'),
 ('TCS',       'Digital Developer',           'Build modern web front ends for digital transformation projects.',     'Mumbai',    7.0, 7.0, '2026-03-05', 'Closed'),
 ('Zoho',      'Product Developer',           'Own features end to end in a product engineering team.',               'Chennai',   8.0, 8.5, '2026-03-20', 'Closed'),
 ('Wipro',     'Full Stack Developer',        'Build web applications using React and ASP.NET Core.',                 'Bengaluru', 7.0, 6.0, '2026-12-15', 'Open'),
 ('HCLTech',   'Data Analyst',                'Analyse business data and build dashboards for clients.',              'Noida',     7.0, 5.5, '2026-12-31', 'Open'),
 ('Accenture', 'Cloud Engineer',              'Deploy and operate cloud-based applications and pipelines.',           'Gurugram',  7.5, 7.5, '2027-01-15', 'Open')
) AS v(CompanyName, Title, Description, Location, MinCgpa, PackageLpa, Deadline, Status)
JOIN Company c ON c.CompanyName = v.CompanyName;

-- 2. JOB SKILLS (job title, skill, 1 = mandatory / 0 = optional)
INSERT INTO JobSkill (JobId, SkillId, IsMandatory)
SELECT j.JobId, sk.SkillId, v.IsMandatory
FROM (VALUES
 ('Systems Engineer','SQL',1), ('Systems Engineer','Java',1), ('Systems Engineer','Data Structures',1), ('Systems Engineer','Communication',0),
 ('Assistant System Engineer','SQL',1), ('Assistant System Engineer','Java',1), ('Assistant System Engineer','Python',0), ('Assistant System Engineer','Communication',0),
 ('Project Engineer','SQL',1), ('Project Engineer','Java',1), ('Project Engineer','Git',0),
 ('Specialist Programmer','Java',1), ('Specialist Programmer','Data Structures',1), ('Specialist Programmer','SQL',1), ('Specialist Programmer','Docker',0),
 ('Software Developer','C#',1), ('Software Developer','ASP.NET Core',1), ('Software Developer','SQL',1), ('Software Developer','Git',0),
 ('Member Technical Staff','Java',1), ('Member Technical Staff','Data Structures',1), ('Member Technical Staff','JavaScript',0),
 ('Associate Software Engineer','Python',1), ('Associate Software Engineer','SQL',1), ('Associate Software Engineer','Communication',0),
 ('Digital Developer','JavaScript',1), ('Digital Developer','React',1), ('Digital Developer','HTML/CSS',1), ('Digital Developer','Git',0),
 ('Product Developer','Java',1), ('Product Developer','Data Structures',1), ('Product Developer','SQL',0),
 ('Full Stack Developer','React',1), ('Full Stack Developer','JavaScript',1), ('Full Stack Developer','ASP.NET Core',1), ('Full Stack Developer','SQL',1), ('Full Stack Developer','Git',0),
 ('Data Analyst','SQL',1), ('Data Analyst','Python',1), ('Data Analyst','Power BI',1), ('Data Analyst','Communication',0),
 ('Cloud Engineer','Docker',1), ('Cloud Engineer','Python',1), ('Cloud Engineer','Git',1), ('Cloud Engineer','Java',0)
) AS v(Title, SkillName, IsMandatory)
JOIN Job j    ON j.Title = v.Title
JOIN Skill sk ON sk.SkillName = v.SkillName;

-- 3. STUDENT SKILLS (3 per student)
INSERT INTO StudentSkill (StudentId, SkillId, Proficiency)
SELECT s.StudentId, sk.SkillId, v.Proficiency
FROM (VALUES
 ('MCA24001','SQL','Advanced'),('MCA24001','Java','Intermediate'),('MCA24001','Data Structures','Intermediate'),
 ('MCA24002','Python','Intermediate'),('MCA24002','SQL','Intermediate'),('MCA24002','Communication','Advanced'),
 ('MCA24003','HTML/CSS','Intermediate'),('MCA24003','JavaScript','Beginner'),('MCA24003','SQL','Beginner'),
 ('MCA24004','Java','Advanced'),('MCA24004','SQL','Intermediate'),('MCA24004','Python','Intermediate'),
 ('MCA24005','C#','Intermediate'),('MCA24005','SQL','Intermediate'),('MCA24005','Git','Beginner'),
 ('MCA24006','Java','Intermediate'),('MCA24006','Communication','Advanced'),('MCA24006','SQL','Intermediate'),
 ('MCA24007','HTML/CSS','Beginner'),('MCA24007','JavaScript','Beginner'),('MCA24007','Git','Beginner'),
 ('MCA24008','SQL','Advanced'),('MCA24008','Java','Intermediate'),('MCA24008','Git','Intermediate'),
 ('MCA24009','Java','Intermediate'),('MCA24009','SQL','Intermediate'),('MCA24009','Git','Beginner'),
 ('MCA24010','SQL','Advanced'),('MCA24010','Java','Advanced'),('MCA24010','Data Structures','Advanced'),
 ('MCA25001','C#','Advanced'),('MCA25001','ASP.NET Core','Intermediate'),('MCA25001','SQL','Intermediate'),
 ('MCA25002','Python','Advanced'),('MCA25002','SQL','Intermediate'),('MCA25002','Power BI','Intermediate'),
 ('MCA25003','HTML/CSS','Intermediate'),('MCA25003','JavaScript','Beginner'),('MCA25003','Git','Beginner'),
 ('MCA25004','Java','Advanced'),('MCA25004','Data Structures','Advanced'),('MCA25004','SQL','Advanced'),
 ('MCA25005','C#','Intermediate'),('MCA25005','ASP.NET Core','Intermediate'),('MCA25005','SQL','Intermediate'),
 ('MCA25006','Java','Advanced'),('MCA25006','Data Structures','Intermediate'),('MCA25006','JavaScript','Intermediate'),
 ('MCA25007','Python','Beginner'),('MCA25007','SQL','Beginner'),('MCA25007','Communication','Intermediate'),
 ('MCA25008','C#','Intermediate'),('MCA25008','SQL','Advanced'),('MCA25008','Git','Intermediate'),
 ('MCA25009','Java','Intermediate'),('MCA25009','MongoDB','Beginner'),('MCA25009','SQL','Intermediate'),
 ('MCA25010','Java','Advanced'),('MCA25010','Data Structures','Advanced'),('MCA25010','Docker','Intermediate'),
 ('MCA26001','React','Intermediate'),('MCA26001','JavaScript','Advanced'),('MCA26001','SQL','Intermediate'),
 ('MCA26002','Python','Intermediate'),('MCA26002','SQL','Intermediate'),('MCA26002','Power BI','Beginner'),
 ('MCA26003','JavaScript','Advanced'),('MCA26003','React','Advanced'),('MCA26003','HTML/CSS','Advanced'),
 ('MCA26004','Python','Intermediate'),('MCA26004','SQL','Intermediate'),('MCA26004','Communication','Intermediate'),
 ('MCA26005','React','Advanced'),('MCA26005','JavaScript','Advanced'),('MCA26005','Git','Intermediate'),
 ('MCA26006','ASP.NET Core','Intermediate'),('MCA26006','C#','Intermediate'),('MCA26006','SQL','Intermediate'),
 ('MCA26007','Python','Beginner'),('MCA26007','Power BI','Intermediate'),('MCA26007','Communication','Advanced'),
 ('MCA26008','Python','Intermediate'),('MCA26008','SQL','Beginner'),('MCA26008','Communication','Intermediate'),
 ('MCA26009','Java','Advanced'),('MCA26009','Data Structures','Advanced'),('MCA26009','Docker','Beginner'),
 ('MCA26010','React','Beginner'),('MCA26010','JavaScript','Intermediate'),('MCA26010','SQL','Beginner')
) AS v(RollNo, SkillName, Proficiency)
JOIN Student s ON s.RollNo = v.RollNo
JOIN Skill sk  ON sk.SkillName = v.SkillName;

-- 4. APPLICATIONS (roll no, job title, applied on, final status)
INSERT INTO Application (StudentId, JobId, AppliedOn, Status)
SELECT s.StudentId, j.JobId, CAST(v.AppliedOn AS DATE), v.Status
FROM (VALUES
 -- 2024 batch
 ('MCA24001','Systems Engineer','2024-01-20','Selected'),
 ('MCA24002','Systems Engineer','2024-01-21','Rejected'),
 ('MCA24003','Systems Engineer','2024-01-22','Rejected'),
 ('MCA24004','Assistant System Engineer','2024-02-05','Selected'),
 ('MCA24005','Assistant System Engineer','2024-02-06','Rejected'),
 ('MCA24006','Assistant System Engineer','2024-02-06','Selected'),
 ('MCA24007','Assistant System Engineer','2024-02-07','Rejected'),
 ('MCA24008','Project Engineer','2024-02-15','Selected'),
 ('MCA24009','Project Engineer','2024-02-16','Selected'),
 ('MCA24003','Project Engineer','2024-02-16','Rejected'),
 ('MCA24010','Project Engineer','2024-02-17','Selected'),
 -- 2025 batch
 ('MCA25004','Specialist Programmer','2025-01-15','Selected'),
 ('MCA25010','Specialist Programmer','2025-01-15','Selected'),
 ('MCA25001','Specialist Programmer','2025-01-16','Rejected'),
 ('MCA25006','Specialist Programmer','2025-01-16','Rejected'),
 ('MCA25002','Specialist Programmer','2025-01-17','Rejected'),
 ('MCA25001','Software Developer','2025-02-10','Selected'),
 ('MCA25005','Software Developer','2025-02-11','Selected'),
 ('MCA25009','Software Developer','2025-02-11','Rejected'),
 ('MCA25008','Software Developer','2025-02-12','Selected'),
 ('MCA25002','Software Developer','2025-02-12','Rejected'),
 ('MCA25006','Member Technical Staff','2025-02-25','Selected'),
 ('MCA25002','Member Technical Staff','2025-02-26','Rejected'),
 -- 2026 batch, closed jobs
 ('MCA26004','Associate Software Engineer','2026-01-18','Selected'),
 ('MCA26008','Associate Software Engineer','2026-01-18','Selected'),
 ('MCA26002','Associate Software Engineer','2026-01-19','Rejected'),
 ('MCA26007','Associate Software Engineer','2026-01-19','Rejected'),
 ('MCA26005','Digital Developer','2026-02-10','Selected'),
 ('MCA26003','Digital Developer','2026-02-10','Selected'),
 ('MCA26006','Digital Developer','2026-02-11','Rejected'),
 ('MCA26010','Digital Developer','2026-02-11','Rejected'),
 ('MCA26009','Product Developer','2026-02-25','Selected'),
 ('MCA26001','Product Developer','2026-02-25','Rejected'),
 -- 2026 batch, open jobs
 ('MCA26001','Full Stack Developer','2026-09-20','Shortlisted'),
 ('MCA26006','Full Stack Developer','2026-09-21','Applied'),
 ('MCA26007','Full Stack Developer','2026-09-22','Applied'),
 ('MCA26010','Full Stack Developer','2026-09-22','Interview'),
 ('MCA26001','Data Analyst','2026-09-25','Applied'),
 ('MCA26002','Data Analyst','2026-09-26','Applied'),
 ('MCA26006','Data Analyst','2026-09-27','Shortlisted'),
 ('MCA26001','Cloud Engineer','2026-09-28','Applied'),
 ('MCA26006','Cloud Engineer','2026-09-28','Applied')
) AS v(RollNo, Title, AppliedOn, Status)
JOIN Student s ON s.RollNo = v.RollNo
JOIN Job j     ON j.Title  = v.Title;

-- 5. PLACEMENTS, generated from every Selected application (a one-off seed shortcut;
--    the application itself uses usp_SelectApplication)
INSERT INTO Placement (ApplicationId, PlacementDate, PackageLpa, AcademicYear)
SELECT a.ApplicationId,
       DATEADD(DAY, 30, j.Deadline),
       j.PackageLpa,
       CAST(s.GraduationYear - 1 AS NVARCHAR(4)) + '-' + CAST(s.GraduationYear AS NVARCHAR(4))
FROM Application a
JOIN Job j     ON j.JobId = a.JobId
JOIN Student s ON s.StudentId = a.StudentId
WHERE a.Status = 'Selected';

-- Safety check: roll everything back if a name was mistyped and a row was dropped by a join
IF (SELECT COUNT(*) FROM Job) <> 12
   OR (SELECT COUNT(*) FROM JobSkill) <> 45
   OR (SELECT COUNT(*) FROM StudentSkill) <> 90
   OR (SELECT COUNT(*) FROM Application) <> 42
   OR (SELECT COUNT(*) FROM Placement) <> 17
    THROW 50092, 'Seed part 2: unexpected row counts, rolled back.', 1;

COMMIT;
GO

-- Expected: Job 12, JobSkill 45, StudentSkill 90, Application 42, Placement 17
-- Placements per academic year: 2023-2024 = 6, 2024-2025 = 6, 2025-2026 = 5
SELECT 'Job' AS TableName, COUNT(*) AS Rows FROM Job
UNION ALL SELECT 'JobSkill',     COUNT(*) FROM JobSkill
UNION ALL SELECT 'StudentSkill', COUNT(*) FROM StudentSkill
UNION ALL SELECT 'Application',  COUNT(*) FROM Application
UNION ALL SELECT 'Placement',    COUNT(*) FROM Placement;

SELECT AcademicYear, COUNT(*) AS Placed
FROM Placement
GROUP BY AcademicYear
ORDER BY AcademicYear;
GO
