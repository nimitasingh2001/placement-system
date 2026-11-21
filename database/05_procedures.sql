-- =============================================================================
-- 05_procedures.sql
-- The three business-rule procedures. Cross-table rules that a CHECK
-- constraint cannot express live here; constraints remain as the safety net.
--
-- Error numbers (THROW, 50000+) so the API can map them to HTTP status codes:
--   50001 student not found            50010 application not found
--   50002 student inactive             50011 already selected
--   50003 job not found                50012 not at Interview stage
--   50004 job not open                 50014 package must be > 0
--   50005 deadline passed              50020 application not found (status)
--   50006 CGPA below minimum           50021 invalid status
--   50007 duplicate application        50022 use usp_SelectApplication
--                                      50023 stage change not allowed
--                                      50024 changed by someone else
-- =============================================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE PlacementDb;
GO

-- -----------------------------------------------------------------------------
-- Student applies for a job (FR-13, 14, 15)
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE usp_ApplyForJob
    @StudentId     INT,
    @JobId         INT,
    @ApplicationId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Cgpa DECIMAL(4,2), @StudentActive BIT;
    SELECT @Cgpa = Cgpa, @StudentActive = IsActive
    FROM Student WHERE StudentId = @StudentId;

    IF @Cgpa IS NULL      THROW 50001, 'Student not found.', 1;
    IF @StudentActive = 0 THROW 50002, 'This student account is inactive.', 1;

    DECLARE @MinCgpa DECIMAL(4,2), @Status NVARCHAR(10), @Deadline DATE, @CompanyActive BIT;
    SELECT @MinCgpa = j.MinCgpa, @Status = j.Status, @Deadline = j.Deadline, @CompanyActive = c.IsActive
    FROM Job j
    JOIN Company c ON c.CompanyId = j.CompanyId
    WHERE j.JobId = @JobId;

    IF @Status IS NULL                         THROW 50003, 'Job not found.', 1;
    IF @Status <> 'Open' OR @CompanyActive = 0 THROW 50004, 'This job is not open for applications.', 1;
    IF @Deadline < CAST(GETDATE() AS DATE)     THROW 50005, 'The application deadline has passed.', 1;
    IF @Cgpa < @MinCgpa                        THROW 50006, 'Your CGPA is below the minimum required for this job.', 1;

    IF EXISTS (SELECT 1 FROM Application WHERE StudentId = @StudentId AND JobId = @JobId)
        THROW 50007, 'You have already applied to this job.', 1;

    BEGIN TRY
        INSERT INTO Application (StudentId, JobId) VALUES (@StudentId, @JobId);
        SET @ApplicationId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        -- two requests at the same instant can both pass the check above;
        -- the UNIQUE constraint stops the second and we turn it into a friendly error
        IF ERROR_NUMBER() IN (2627, 2601)
            THROW 50007, 'You have already applied to this job.', 1;
        THROW;
    END CATCH
END
GO

-- -----------------------------------------------------------------------------
-- Admin moves an application through the stages.
--   Applied -> Shortlisted -> Interview -> (Selected via usp_SelectApplication)
--   Applied / Shortlisted / Interview -> Rejected.  Selected and Rejected are final.
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE usp_UpdateApplicationStatus
    @ApplicationId INT,
    @NewStatus     NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Current NVARCHAR(15), @Msg NVARCHAR(200);

    SELECT @Current = Status FROM Application WHERE ApplicationId = @ApplicationId;

    IF @Current IS NULL
        THROW 50020, 'Application not found.', 1;

    IF @NewStatus = 'Selected'
        THROW 50022, 'Use usp_SelectApplication to select a candidate.', 1;

    IF @NewStatus NOT IN ('Shortlisted', 'Interview', 'Rejected')
        THROW 50021, 'Invalid status.', 1;

    IF NOT (   (@Current = 'Applied'     AND @NewStatus IN ('Shortlisted', 'Rejected'))
            OR (@Current = 'Shortlisted' AND @NewStatus IN ('Interview', 'Rejected'))
            OR (@Current = 'Interview'   AND @NewStatus = 'Rejected'))
    BEGIN
        SET @Msg = CONCAT('Cannot change status from ', @Current, ' to ', @NewStatus, '.');
        THROW 50023, @Msg, 1;
    END

    -- only update if nobody changed it since we read it
    UPDATE Application
    SET Status = @NewStatus
    WHERE ApplicationId = @ApplicationId AND Status = @Current;

    IF @@ROWCOUNT = 0
        THROW 50024, 'The application was changed by someone else. Please refresh and try again.', 1;
END
GO

-- -----------------------------------------------------------------------------
-- Admin selects a candidate (FR-19, 20): status -> Selected AND a Placement row,
-- in one transaction (both happen or neither). Only an application at the
-- Interview stage can be selected. A student may hold more than one placement.
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE usp_SelectApplication
    @ApplicationId INT,
    @PackageLpa    DECIMAL(5,2) = NULL,   -- NULL = use the job's advertised package
    @PlacementId   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @Status NVARCHAR(15), @JobPackage DECIMAL(5,2), @GradYear INT;

        -- lock this application so two admins cannot process it at the same time
        SELECT @Status = a.Status,
               @JobPackage = j.PackageLpa,
               @GradYear = s.GraduationYear
        FROM Application a WITH (UPDLOCK, HOLDLOCK)
        JOIN Job j     ON j.JobId     = a.JobId
        JOIN Student s ON s.StudentId = a.StudentId
        WHERE a.ApplicationId = @ApplicationId;

        IF @Status IS NULL        THROW 50010, 'Application not found.', 1;
        IF @Status = 'Selected'   THROW 50011, 'This application is already selected.', 1;
        IF @Status <> 'Interview' THROW 50012, 'Only an application at the Interview stage can be selected.', 1;

        IF @PackageLpa IS NULL SET @PackageLpa = @JobPackage;
        IF @PackageLpa <= 0    THROW 50014, 'Package must be greater than zero.', 1;

        UPDATE Application SET Status = 'Selected' WHERE ApplicationId = @ApplicationId;

        INSERT INTO Placement (ApplicationId, PackageLpa, AcademicYear)
        VALUES (@ApplicationId, @PackageLpa,
                CAST(@GradYear - 1 AS NVARCHAR(4)) + '-' + CAST(@GradYear AS NVARCHAR(4)));
        SET @PlacementId = SCOPE_IDENTITY();

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;     -- re-raise so the caller sees the original error
    END CATCH
END
GO

PRINT 'Procedures created: 3.';
GO
