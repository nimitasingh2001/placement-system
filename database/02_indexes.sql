-- =============================================================================
-- 02_indexes.sql
-- Nonclustered indexes for foreign keys / frequent filters that SQL Server
-- does not index automatically. Primary keys and UNIQUE constraints are
-- already indexed, so they are not repeated here.
--
-- Measured on 100,000 applications (logical reads, scan vs index):
--   applications for one job   567 -> 3
--   students with a skill      297 -> 22
-- =============================================================================
USE PlacementDb;
GO

-- Applications for a job (admin screen). Covering: no key lookup needed.
DROP INDEX IF EXISTS IX_Application_JobId ON Application;
CREATE INDEX IX_Application_JobId
    ON Application(JobId)
    INCLUDE (StudentId, AppliedOn, Status);

-- Students who have a skill (the composite key only helps lookups by StudentId)
DROP INDEX IF EXISTS IX_StudentSkill_SkillId ON StudentSkill;
CREATE INDEX IX_StudentSkill_SkillId
    ON StudentSkill(SkillId)
    INCLUDE (Proficiency);

-- Jobs requiring a skill
DROP INDEX IF EXISTS IX_JobSkill_SkillId ON JobSkill;
CREATE INDEX IX_JobSkill_SkillId
    ON JobSkill(SkillId);

-- Jobs of a company / company-to-job joins
DROP INDEX IF EXISTS IX_Job_CompanyId ON Job;
CREATE INDEX IX_Job_CompanyId
    ON Job(CompanyId);

-- Open jobs ordered by deadline (main student screen).
-- If you ran experiment 3C, add INCLUDE (Title) to make it covering.
DROP INDEX IF EXISTS IX_Job_Status_Deadline ON Job;
CREATE INDEX IX_Job_Status_Deadline
    ON Job(Status, Deadline);
GO

PRINT 'Indexes created: 5.';
GO
