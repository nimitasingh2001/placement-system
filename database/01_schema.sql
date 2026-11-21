-- =============================================================================
-- 01_schema.sql
-- All 9 tables with primary keys, foreign keys, UNIQUE and CHECK constraints.
--
-- WARNING: drops and recreates every table, so ALL DATA IS LOST.
-- Run order: 00 -> 01 -> 02 -> 03 -> 04 -> 05 -> 06 -> 07
-- =============================================================================
SET QUOTED_IDENTIFIER ON;   -- required for the filtered index on AppUser
SET ANSI_NULLS ON;
GO

USE PlacementDb;
GO

-- Drop children before parents (foreign keys)
DROP TABLE IF EXISTS Placement;
DROP TABLE IF EXISTS Application;
DROP TABLE IF EXISTS JobSkill;
DROP TABLE IF EXISTS StudentSkill;
DROP TABLE IF EXISTS AppUser;
DROP TABLE IF EXISTS Job;
DROP TABLE IF EXISTS Student;
DROP TABLE IF EXISTS Company;
DROP TABLE IF EXISTS Skill;
GO

-- -----------------------------------------------------------------------------
-- Master tables (no foreign keys)
-- -----------------------------------------------------------------------------
CREATE TABLE Skill
(
    SkillId    INT IDENTITY(1,1) NOT NULL,
    SkillName  NVARCHAR(100) NOT NULL,
    Category   NVARCHAR(50)  NULL,
    IsActive   BIT NOT NULL CONSTRAINT DF_Skill_IsActive DEFAULT 1,

    CONSTRAINT PK_Skill PRIMARY KEY (SkillId),
    CONSTRAINT UQ_Skill_SkillName UNIQUE (SkillName)
);
GO

CREATE TABLE Company
(
    CompanyId    INT IDENTITY(1,1) NOT NULL,
    CompanyName  NVARCHAR(150) NOT NULL,
    Industry     NVARCHAR(100) NULL,
    Location     NVARCHAR(100) NOT NULL,
    Website      NVARCHAR(200) NOT NULL,
    IsActive     BIT NOT NULL CONSTRAINT DF_Company_IsActive DEFAULT 1,

    CONSTRAINT PK_Company PRIMARY KEY (CompanyId),
    CONSTRAINT UQ_Company_CompanyName UNIQUE (CompanyName),
    CONSTRAINT CK_Company_Location CHECK (LEN(LTRIM(RTRIM(Location))) > 0),
    CONSTRAINT CK_Company_Website  CHECK (Website LIKE 'http://%' OR Website LIKE 'https://%')
);
GO

CREATE TABLE Student
(
    StudentId       INT IDENTITY(1,1) NOT NULL,
    RollNo          NVARCHAR(20)  NOT NULL,
    FirstName       NVARCHAR(100) NOT NULL,
    LastName        NVARCHAR(100) NULL,
    Email           NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(15)  NOT NULL,
    Course          NVARCHAR(50)  NOT NULL,
    Cgpa            DECIMAL(4,2)  NOT NULL,
    GraduationYear  INT           NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Student_IsActive DEFAULT 1,

    CONSTRAINT PK_Student PRIMARY KEY (StudentId),
    CONSTRAINT UQ_Student_RollNo UNIQUE (RollNo),
    CONSTRAINT UQ_Student_Email  UNIQUE (Email),
    CONSTRAINT CK_Student_Cgpa     CHECK (Cgpa BETWEEN 0 AND 10),
    CONSTRAINT CK_Student_GradYear CHECK (GraduationYear BETWEEN 2000 AND 2100)
);
GO

-- -----------------------------------------------------------------------------
-- Tables that reference the master tables
-- -----------------------------------------------------------------------------
CREATE TABLE Job
(
    JobId       INT IDENTITY(1,1) NOT NULL,
    CompanyId   INT            NOT NULL,
    Title       NVARCHAR(150)  NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Location    NVARCHAR(100)  NOT NULL,
    MinCgpa     DECIMAL(4,2)   NOT NULL CONSTRAINT DF_Job_MinCgpa DEFAULT 0,
    PackageLpa  DECIMAL(5,2)   NOT NULL,
    Deadline    DATE           NOT NULL,
    Status      NVARCHAR(10)   NOT NULL CONSTRAINT DF_Job_Status DEFAULT 'Open',

    CONSTRAINT PK_Job PRIMARY KEY (JobId),
    CONSTRAINT FK_Job_Company FOREIGN KEY (CompanyId) REFERENCES Company(CompanyId),
    CONSTRAINT CK_Job_MinCgpa    CHECK (MinCgpa BETWEEN 0 AND 10),
    CONSTRAINT CK_Job_PackageLpa CHECK (PackageLpa > 0),
    CONSTRAINT CK_Job_Status     CHECK (Status IN ('Open', 'Closed')),
    CONSTRAINT CK_Job_Location   CHECK (LEN(LTRIM(RTRIM(Location))) > 0)
);
GO

CREATE TABLE AppUser
(
    UserId       INT IDENTITY(1,1) NOT NULL,
    Email        NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role         NVARCHAR(10)  NOT NULL,
    StudentId    INT           NULL,

    CONSTRAINT PK_AppUser PRIMARY KEY (UserId),
    CONSTRAINT UQ_AppUser_Email UNIQUE (Email),
    CONSTRAINT FK_AppUser_Student FOREIGN KEY (StudentId) REFERENCES Student(StudentId),
    CONSTRAINT CK_AppUser_Role CHECK (Role IN ('Student', 'Admin')),
    CONSTRAINT CK_AppUser_RoleStudent CHECK (
        (Role = 'Student' AND StudentId IS NOT NULL) OR
        (Role = 'Admin'   AND StudentId IS NULL)
    )
);
GO

-- One login per student, but any number of admins (NULLs are ignored by the filter)
CREATE UNIQUE INDEX UX_AppUser_StudentId
ON AppUser(StudentId)
WHERE StudentId IS NOT NULL;
GO

-- -----------------------------------------------------------------------------
-- Junction tables (composite primary keys)
-- -----------------------------------------------------------------------------
CREATE TABLE StudentSkill
(
    StudentId   INT          NOT NULL,
    SkillId     INT          NOT NULL,
    Proficiency NVARCHAR(15) NOT NULL CONSTRAINT DF_StudentSkill_Proficiency DEFAULT 'Beginner',

    CONSTRAINT PK_StudentSkill PRIMARY KEY (StudentId, SkillId),
    CONSTRAINT FK_StudentSkill_Student FOREIGN KEY (StudentId) REFERENCES Student(StudentId),
    CONSTRAINT FK_StudentSkill_Skill   FOREIGN KEY (SkillId)   REFERENCES Skill(SkillId),
    CONSTRAINT CK_StudentSkill_Proficiency CHECK (Proficiency IN ('Beginner', 'Intermediate', 'Advanced'))
);
GO

CREATE TABLE JobSkill
(
    JobId       INT NOT NULL,
    SkillId     INT NOT NULL,
    IsMandatory BIT NOT NULL CONSTRAINT DF_JobSkill_IsMandatory DEFAULT 1,

    CONSTRAINT PK_JobSkill PRIMARY KEY (JobId, SkillId),
    CONSTRAINT FK_JobSkill_Job   FOREIGN KEY (JobId)   REFERENCES Job(JobId),
    CONSTRAINT FK_JobSkill_Skill FOREIGN KEY (SkillId) REFERENCES Skill(SkillId)
);
GO

-- -----------------------------------------------------------------------------
-- Transaction tables
-- -----------------------------------------------------------------------------
CREATE TABLE Application
(
    ApplicationId INT IDENTITY(1,1) NOT NULL,
    StudentId     INT          NOT NULL,
    JobId         INT          NOT NULL,
    AppliedOn     DATE         NOT NULL CONSTRAINT DF_Application_AppliedOn DEFAULT (CAST(GETDATE() AS DATE)),
    Status        NVARCHAR(15) NOT NULL CONSTRAINT DF_Application_Status DEFAULT 'Applied',

    CONSTRAINT PK_Application PRIMARY KEY (ApplicationId),
    CONSTRAINT FK_Application_Student FOREIGN KEY (StudentId) REFERENCES Student(StudentId),
    CONSTRAINT FK_Application_Job     FOREIGN KEY (JobId)     REFERENCES Job(JobId),
    CONSTRAINT UQ_Application_Student_Job UNIQUE (StudentId, JobId),
    CONSTRAINT CK_Application_Status CHECK (Status IN ('Applied', 'Shortlisted', 'Interview', 'Selected', 'Rejected'))
);
GO

-- Student, job and company are reached through ApplicationId (no duplicated columns).
-- A student may hold more than one placement (second "dream offer"): one row per application.
CREATE TABLE Placement
(
    PlacementId   INT IDENTITY(1,1) NOT NULL,
    ApplicationId INT           NOT NULL,
    PlacementDate DATE          NOT NULL CONSTRAINT DF_Placement_Date DEFAULT (CAST(GETDATE() AS DATE)),
    PackageLpa    DECIMAL(5,2)  NOT NULL,
    AcademicYear  NVARCHAR(9)   NOT NULL,

    CONSTRAINT PK_Placement PRIMARY KEY (PlacementId),
    CONSTRAINT UQ_Placement_ApplicationId UNIQUE (ApplicationId),
    CONSTRAINT FK_Placement_Application FOREIGN KEY (ApplicationId) REFERENCES Application(ApplicationId),
    CONSTRAINT CK_Placement_PackageLpa   CHECK (PackageLpa > 0),
    CONSTRAINT CK_Placement_AcademicYear CHECK (AcademicYear LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]')
);
GO

PRINT 'Schema created: 9 tables.';
GO
