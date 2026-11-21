-- =============================================================================
-- optional/bulk_data_cleanup.sql
-- Removes the rows added by bulk_data.sql (children first). Indexes stay.
-- Expected afterwards: Student 30, Job 12, Application 42, StudentSkill 90, Placement 17
-- =============================================================================
USE PlacementDb;
GO
DELETE FROM StudentSkill WHERE StudentId IN (SELECT StudentId FROM Student WHERE RollNo LIKE 'BULK%');
DELETE FROM Application  WHERE StudentId IN (SELECT StudentId FROM Student WHERE RollNo LIKE 'BULK%');
DELETE FROM Job          WHERE Title LIKE 'Bulk Job %';
DELETE FROM Student      WHERE RollNo LIKE 'BULK%';
GO

SELECT 'Student' AS TableName, COUNT(*) AS Rows FROM Student
UNION ALL SELECT 'Job',          COUNT(*) FROM Job
UNION ALL SELECT 'Application',  COUNT(*) FROM Application
UNION ALL SELECT 'StudentSkill', COUNT(*) FROM StudentSkill
UNION ALL SELECT 'Placement',    COUNT(*) FROM Placement;
GO
