-- =============================================================================
-- optional/index_experiments.sql
-- Compares a forced full scan (WITH (INDEX(0))) with the real index.
-- Needs bulk_data.sql loaded. Read the MESSAGES tab: "logical reads" is the
-- number to compare. Press Ctrl+M first to also see the execution plan.
-- Results measured here: 567 -> 3 (applications by job), 297 -> 22 (students by
-- skill), 31 -> 31 (open jobs: the optimizer correctly preferred the scan).
-- =============================================================================
USE PlacementDb;
GO
SET STATISTICS IO ON;

DECLARE @JobId   INT = (SELECT JobId   FROM Job   WHERE Title = 'Bulk Job 100');
DECLARE @SkillId INT = (SELECT SkillId FROM Skill WHERE SkillName = 'SQL');

PRINT '=== 1A: Application, forced scan ===';
SELECT a.ApplicationId, a.StudentId, a.AppliedOn, a.Status
FROM Application a WITH (INDEX(0))
WHERE a.JobId = @JobId;

PRINT '=== 1B: Application, with index ===';
SELECT a.ApplicationId, a.StudentId, a.AppliedOn, a.Status
FROM Application a
WHERE a.JobId = @JobId;

PRINT '=== 2A: StudentSkill, forced scan ===';
SELECT ss.StudentId
FROM StudentSkill ss WITH (INDEX(0))
WHERE ss.SkillId = @SkillId AND ss.Proficiency = 'Advanced';

PRINT '=== 2B: StudentSkill, with index ===';
SELECT ss.StudentId
FROM StudentSkill ss
WHERE ss.SkillId = @SkillId AND ss.Proficiency = 'Advanced';

PRINT '=== 3A: Job, forced scan ===';
SELECT JobId, Title, Deadline
FROM Job WITH (INDEX(0))
WHERE Status = 'Open' AND Deadline >= CAST(GETDATE() AS DATE)
ORDER BY Deadline;

PRINT '=== 3B: Job, with index ===';
SELECT JobId, Title, Deadline
FROM Job
WHERE Status = 'Open' AND Deadline >= CAST(GETDATE() AS DATE)
ORDER BY Deadline;
GO

-- 3C: make the Job index covering, then measure again
CREATE INDEX IX_Job_Status_Deadline
ON Job(Status, Deadline)
INCLUDE (Title)
WITH (DROP_EXISTING = ON);
GO

SET STATISTICS IO ON;
PRINT '=== 3C: Job, covering index ===';
SELECT JobId, Title, Deadline
FROM Job
WHERE Status = 'Open' AND Deadline >= CAST(GETDATE() AS DATE)
ORDER BY Deadline;
GO
