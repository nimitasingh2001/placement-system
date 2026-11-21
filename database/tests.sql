-- =============================================================================
-- tests.sql
-- The test blocks used while building the database, kept for re-checking.
--
-- HOW TO USE
--   * Run ONE section at a time (select it, press F5). Variables only live
--     inside one batch, which is why sections are separated by GO.
--   * Press Ctrl+T first (Results to Text) so the output is easy to read/copy.
--   * Start from freshly seeded data: Student 30, Job 12, Application 42,
--     StudentSkill 90, Placement 17 (run 06 and 07 to reset).
--   * Sections A, B2, D roll back and leave nothing behind.
--     Section C temporarily adds a constraint: ALWAYS run its last line.
-- =============================================================================
USE PlacementDb;
GO

-- -----------------------------------------------------------------------------
-- 0. Sanity check.  Expected: 30, 12, 42, 90, 17
-- -----------------------------------------------------------------------------
SELECT 'Student' AS TableName, COUNT(*) AS Rows FROM Student
UNION ALL SELECT 'Job',          COUNT(*) FROM Job
UNION ALL SELECT 'Application',  COUNT(*) FROM Application
UNION ALL SELECT 'StudentSkill', COUNT(*) FROM StudentSkill
UNION ALL SELECT 'Placement',    COUNT(*) FROM Placement;
GO

-- -----------------------------------------------------------------------------
-- A. usp_ApplyForJob
--    Expected: 1 success row, then 6 errors:
--    50006 (CGPA), 50004 (closed), 50001 (no student), 50003 (no job),
--    50007 (duplicate), 50005 (deadline)
-- -----------------------------------------------------------------------------
DECLARE @S1 INT = (SELECT StudentId FROM Student WHERE RollNo = 'MCA26001');   -- CGPA 8.5
DECLARE @S2 INT = (SELECT StudentId FROM Student WHERE RollNo = 'MCA26002');   -- CGPA 7.4
DECLARE @JFull  INT = (SELECT JobId FROM Job WHERE Title = 'Full Stack Developer');
DECLARE @JCloud INT = (SELECT JobId FROM Job WHERE Title = 'Cloud Engineer');
DECLARE @JData  INT = (SELECT JobId FROM Job WHERE Title = 'Data Analyst');
DECLARE @JProd  INT = (SELECT JobId FROM Job WHERE Title = 'Product Developer');
DECLARE @App INT;

BEGIN TRAN;

EXEC usp_ApplyForJob @S2, @JFull, @App OUTPUT;
SELECT @App AS NewApplicationId, 'should succeed' AS Expected;

BEGIN TRY EXEC usp_ApplyForJob @S2, @JCloud, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'CGPA too low' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_ApplyForJob @S1, @JProd, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'closed job' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_ApplyForJob 999999, @JFull, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'no such student' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_ApplyForJob @S1, 999999, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'no such job' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_ApplyForJob @S2, @JData, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'duplicate application' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

UPDATE Job SET Deadline = '2020-01-01' WHERE JobId = @JCloud;
BEGIN TRY EXEC usp_ApplyForJob @S1, @JCloud, @App OUTPUT; END TRY
BEGIN CATCH SELECT 'deadline passed' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

ROLLBACK;
GO

-- -----------------------------------------------------------------------------
-- B1. usp_SelectApplication failures (nothing changes)
--     Expected errors: 50012, 50011, 50010
-- -----------------------------------------------------------------------------
DECLARE @AppApplied  INT = (SELECT ApplicationId FROM vw_ApplicationDetails
                            WHERE RollNo = 'MCA26006' AND JobTitle = 'Full Stack Developer');   -- only Applied
DECLARE @AppSelected INT = (SELECT ApplicationId FROM vw_ApplicationDetails
                            WHERE RollNo = 'MCA26003' AND JobTitle = 'Digital Developer');      -- already Selected
DECLARE @Pid INT;

BEGIN TRY EXEC usp_SelectApplication @AppApplied, NULL, @Pid OUTPUT; END TRY
BEGIN CATCH SELECT 'not at Interview' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_SelectApplication @AppSelected, NULL, @Pid OUTPUT; END TRY
BEGIN CATCH SELECT 'already selected' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_SelectApplication 999999, NULL, @Pid OUTPUT; END TRY
BEGIN CATCH SELECT 'not found' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;
GO

-- -----------------------------------------------------------------------------
-- B2. Dream offer: an already placed student gets and accepts a second offer
--     Expected: 2 rows for Tanvi Shah (TCS Digital Developer 7.00, Accenture
--     Cloud Engineer 7.50); Offers2026 = 6; batch 2026 = 10 students, 5 placed, 50.0
-- -----------------------------------------------------------------------------
BEGIN TRAN;

INSERT INTO Application (StudentId, JobId, Status)
SELECT s.StudentId, j.JobId, 'Interview'
FROM Student s CROSS JOIN Job j
WHERE s.RollNo = 'MCA26003' AND j.Title = 'Cloud Engineer';

DECLARE @AppDream INT = SCOPE_IDENTITY();
DECLARE @Pid INT;

EXEC usp_SelectApplication @AppDream, NULL, @Pid OUTPUT;

SELECT StudentName, CompanyName, JobTitle, PackageLpa, AcademicYear
FROM vw_PlacementDetails
WHERE RollNo = 'MCA26003'
ORDER BY PackageLpa;

SELECT COUNT(*) AS Offers2026 FROM vw_PlacementDetails WHERE GraduationYear = 2026;
SELECT * FROM vw_PlacementStatsByBatch WHERE GraduationYear = 2026;

ROLLBACK;
GO

-- -----------------------------------------------------------------------------
-- C. Atomicity: a failure AFTER the status update must undo the update.
--    A temporary constraint makes every Placement insert fail.
--    Expected: error 547, Status still 'Interview', PlacementRows still 17.
--    IMPORTANT: if this stops early, run the last ALTER TABLE line on its own.
-- -----------------------------------------------------------------------------
DECLARE @AppInterview INT = (SELECT ApplicationId FROM vw_ApplicationDetails
                             WHERE RollNo = 'MCA26010' AND JobTitle = 'Full Stack Developer');
DECLARE @Pid INT;

ALTER TABLE Placement WITH NOCHECK
    ADD CONSTRAINT CK_Placement_TempFail CHECK (PackageLpa < 0);

BEGIN TRY
    EXEC usp_SelectApplication @AppInterview, NULL, @Pid OUTPUT;
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg;
END CATCH;

SELECT Status FROM Application WHERE ApplicationId = @AppInterview;
SELECT COUNT(*) AS PlacementRows FROM Placement;

ALTER TABLE Placement DROP CONSTRAINT CK_Placement_TempFail;
GO

-- -----------------------------------------------------------------------------
-- D. usp_UpdateApplicationStatus
--    Expected: 50023, Shortlisted, Interview, 50022, 50021, Rejected, 50023, 50020
-- -----------------------------------------------------------------------------
DECLARE @App INT = (SELECT ApplicationId FROM vw_ApplicationDetails
                    WHERE RollNo = 'MCA26006' AND JobTitle = 'Full Stack Developer');

BEGIN TRAN;

BEGIN TRY EXEC usp_UpdateApplicationStatus @App, 'Interview'; END TRY      -- skipping a stage
BEGIN CATCH SELECT 'skip a stage' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

EXEC usp_UpdateApplicationStatus @App, 'Shortlisted';
SELECT Status AS AfterShortlist FROM Application WHERE ApplicationId = @App;

EXEC usp_UpdateApplicationStatus @App, 'Interview';
SELECT Status AS AfterInterview FROM Application WHERE ApplicationId = @App;

BEGIN TRY EXEC usp_UpdateApplicationStatus @App, 'Selected'; END TRY
BEGIN CATCH SELECT 'select via this proc' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_UpdateApplicationStatus @App, 'Hired'; END TRY
BEGIN CATCH SELECT 'invalid status' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

EXEC usp_UpdateApplicationStatus @App, 'Rejected';
SELECT Status AS AfterReject FROM Application WHERE ApplicationId = @App;

BEGIN TRY EXEC usp_UpdateApplicationStatus @App, 'Interview'; END TRY      -- Rejected is final
BEGIN CATCH SELECT 'move out of Rejected' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY EXEC usp_UpdateApplicationStatus 999999, 'Rejected'; END TRY
BEGIN CATCH SELECT 'not found' AS Test, ERROR_NUMBER() AS Err, ERROR_MESSAGE() AS Msg; END CATCH;

ROLLBACK;
GO

-- -----------------------------------------------------------------------------
-- E. Skill functions and the "recommended jobs" query
--    Expected: Full Stack 4/3/75.0, Data Analyst 3/1/33.3, Cloud Engineer 3/0/0.0
--    Missing skill for Full Stack Developer: ASP.NET Core
-- -----------------------------------------------------------------------------
DECLARE @S INT = (SELECT StudentId FROM Student WHERE RollNo = 'MCA26001');
DECLARE @J INT = (SELECT JobId FROM Job WHERE Title = 'Full Stack Developer');

SELECT * FROM fn_StudentJobMatch(@S) ORDER BY MatchPct DESC, Title;
SELECT * FROM fn_MissingSkills(@S, @J);
GO

-- Recommended jobs: open, student meets the CGPA minimum, best skill match first.
-- (CROSS APPLY is needed to pass a column of the student row into the function.)
-- Expected for MCA26001: 3 rows. Try MCA26002 (CGPA 7.4): 2 rows.
SELECT o.JobId, o.Title, o.CompanyName, o.Location, o.PackageLpa, o.Deadline, m.MatchPct
FROM Student s
CROSS APPLY fn_StudentJobMatch(s.StudentId) m
JOIN vw_OpenJobs o ON o.JobId = m.JobId
                  AND s.Cgpa >= o.MinCgpa
WHERE s.RollNo = 'MCA26001'
ORDER BY m.MatchPct DESC;
GO

-- -----------------------------------------------------------------------------
-- F. Views.  Expected: 17, 3, 42; batch stats 2024 = 10/6/60.0,
--    2025 = 10/6/60.0, 2026 = 10/5/50.0; company summary below
-- -----------------------------------------------------------------------------
SELECT COUNT(*) AS Placements   FROM vw_PlacementDetails;
SELECT COUNT(*) AS OpenJobs     FROM vw_OpenJobs;
SELECT COUNT(*) AS Applications FROM vw_ApplicationDetails;

SELECT * FROM vw_PlacementStatsByBatch ORDER BY GraduationYear;

-- TCS 4 / 5.30 / 7.00, HCLTech 3 / 5.00 / 5.00, Infosys 3 / 7.67 / 9.50,
-- Wipro 3 / 3.80 / 3.80, Accenture 2 / 4.50 / 4.50, Zoho 2 / 7.75 / 8.50
SELECT * FROM vw_CompanyHiringSummary ORDER BY Hires DESC, CompanyName;
GO
