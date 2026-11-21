-- =============================================================================
-- 04_functions.sql
-- Inline table-valued functions (used in a FROM clause, optimised like a view
-- with parameters). Call with a variable or with CROSS APPLY; a subquery
-- cannot be passed directly as an argument.
-- =============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PlacementDb;
GO

-- Skill match of one student against every currently open job.
-- MatchPct = matched mandatory skills / mandatory skills (a job with none scores 100).
CREATE OR ALTER FUNCTION fn_StudentJobMatch (@StudentId INT)
RETURNS TABLE
AS
RETURN
(
    SELECT j.JobId,
           j.Title,
           c.CompanyName,
           COUNT(js.SkillId) AS MandatorySkills,
           COUNT(ss.SkillId) AS MatchedSkills,
           CAST(CASE WHEN COUNT(js.SkillId) = 0 THEN 100.0
                     ELSE 100.0 * COUNT(ss.SkillId) / COUNT(js.SkillId) END AS DECIMAL(5,1)) AS MatchPct
    FROM Job j
    JOIN Company c ON c.CompanyId = j.CompanyId
    LEFT JOIN JobSkill js     ON js.JobId = j.JobId AND js.IsMandatory = 1
    LEFT JOIN StudentSkill ss ON ss.SkillId = js.SkillId AND ss.StudentId = @StudentId
    WHERE j.Status = 'Open'
      AND j.Deadline >= CAST(GETDATE() AS DATE)
    GROUP BY j.JobId, j.Title, c.CompanyName
);
GO

-- Mandatory skills of a job that a student does not have yet (skill-gap screen)
CREATE OR ALTER FUNCTION fn_MissingSkills (@StudentId INT, @JobId INT)
RETURNS TABLE
AS
RETURN
(
    SELECT sk.SkillId, sk.SkillName
    FROM JobSkill js
    JOIN Skill sk ON sk.SkillId = js.SkillId
    WHERE js.JobId = @JobId
      AND js.IsMandatory = 1
      AND NOT EXISTS (SELECT 1
                      FROM StudentSkill ss
                      WHERE ss.StudentId = @StudentId AND ss.SkillId = js.SkillId)
);
GO

PRINT 'Functions created: 2.';
GO
