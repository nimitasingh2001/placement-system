-- =============================================================================
-- 06_seed_part1.sql
-- Development data, part 1: 15 skills, 6 companies, 30 students.
--
-- WARNING: this DELETES the rows of every table first (including AppUser),
-- so it is a DEV RESET. Never run it against data you want to keep.
-- Re-runnable. Run 07_seed_part2.sql afterwards.
-- =============================================================================
USE PlacementDb;
GO

-- Reset: delete children first, then parents
DELETE FROM Placement;
DELETE FROM Application;
DELETE FROM JobSkill;
DELETE FROM StudentSkill;
DELETE FROM AppUser;
DELETE FROM Job;
DELETE FROM Student;
DELETE FROM Company;
DELETE FROM Skill;
GO

-- Restart ID counters so the first new row gets 1 again.
-- Only tables whose counter has been used are reseeded: on a brand-new empty
-- table, RESEED 0 would make the first row get 0 instead of 1.
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'DBCC CHECKIDENT (''dbo.' + t.name + N''', RESEED, 0) WITH NO_INFOMSGS;' + CHAR(10)
FROM sys.identity_columns ic
JOIN sys.tables t ON t.object_id = ic.object_id
WHERE ic.last_value IS NOT NULL
  AND t.name IN (N'Placement', N'Application', N'AppUser', N'Job', N'Student', N'Company', N'Skill');
EXEC sp_executesql @sql;
GO

SET XACT_ABORT ON;
BEGIN TRAN;

INSERT INTO Skill (SkillName, Category) VALUES
('SQL', 'Database'),
('PL/SQL', 'Database'),
('MongoDB', 'Database'),
('Java', 'Programming'),
('Python', 'Programming'),
('C#', 'Programming'),
('JavaScript', 'Programming'),
('Data Structures', 'Programming'),
('React', 'Web'),
('ASP.NET Core', 'Web'),
('HTML/CSS', 'Web'),
('Git', 'Tools'),
('Docker', 'Tools'),
('Power BI', 'Analytics'),
('Communication', 'Soft Skill');

INSERT INTO Company (CompanyName, Industry, Location, Website) VALUES
('Infosys',   'IT Services',   'Bengaluru', 'https://www.infosys.com'),
('TCS',       'IT Services',   'Mumbai',    'https://www.tcs.com'),
('Wipro',     'IT Services',   'Bengaluru', 'https://www.wipro.com'),
('HCLTech',   'IT Services',   'Noida',     'https://www.hcltech.com'),
('Accenture', 'Consulting',    'Gurugram',  'https://www.accenture.com'),
('Zoho',      'Product',       'Chennai',   'https://www.zoho.com');

INSERT INTO Student (RollNo, FirstName, LastName, Email, Phone, Course, Cgpa, GraduationYear) VALUES
-- 2024 batch
('MCA24001', 'Rohan',  'Sharma',  'rohan.sharma@example.com',  '+919800000101', 'MCA', 8.20, 2024),
('MCA24002', 'Priya',  'Singh',   'priya.singh@example.com',   '+919800000102', 'MCA', 7.80, 2024),
('MCA24003', 'Aman',   'Gupta',   'aman.gupta@example.com',    '+919800000103', 'MCA', 6.90, 2024),
('MCA24004', 'Neha',   'Yadav',   'neha.yadav@example.com',    '+919800000104', 'MCA', 8.90, 2024),
('MCA24005', 'Karan',  'Mehta',   'karan.mehta@example.com',   '+919800000105', 'MCA', 7.10, 2024),
('MCA24006', 'Simran', 'Kaur',    'simran.kaur@example.com',   '+919800000106', 'MCA', 7.60, 2024),
('MCA24007', 'Vikas',  'Kumar',   'vikas.kumar@example.com',   '+919800000107', 'MCA', 6.40, 2024),
('MCA24008', 'Pooja',  'Nair',    'pooja.nair@example.com',    '+919800000108', 'MCA', 8.40, 2024),
('MCA24009', 'Rahul',  'Joshi',   'rahul.joshi@example.com',   '+919800000109', 'MCA', 7.30, 2024),
('MCA24010', 'Ananya', 'Das',     'ananya.das@example.com',    '+919800000110', 'MCA', 9.10, 2024),
-- 2025 batch
('MCA25001', 'Arjun',  'Reddy',   'arjun.reddy@example.com',   '+919800000111', 'MCA', 8.00, 2025),
('MCA25002', 'Isha',   'Malhotra','isha.malhotra@example.com', '+919800000112', 'MCA', 7.50, 2025),
('MCA25003', 'Mohit',  'Tiwari',  'mohit.tiwari@example.com',  '+919800000113', 'MCA', 6.80, 2025),
('MCA25004', 'Kavya',  'Iyer',    'kavya.iyer@example.com',    '+919800000114', 'MCA', 8.70, 2025),
('MCA25005', 'Sahil',  'Khan',    'sahil.khan@example.com',    '+919800000115', 'MCA', 7.20, 2025),
('MCA25006', 'Divya',  'Menon',   'divya.menon@example.com',   '+919800000116', 'MCA', 8.30, 2025),
('MCA25007', 'Nikhil', 'Rao',     'nikhil.rao@example.com',    '+919800000117', 'MCA', 6.60, 2025),
('MCA25008', 'Sneha',  'Patil',   'sneha.patil@example.com',   '+919800000118', 'MCA', 7.90, 2025),
('MCA25009', 'Harsh',  'Vardhan', 'harsh.vardhan@example.com', '+919800000119', 'MCA', 7.00, 2025),
('MCA25010', 'Meera',  'Pillai',  'meera.pillai@example.com',  '+919800000120', 'MCA', 8.60, 2025),
-- 2026 batch
('MCA26001', 'Asha',   'Verma',   'asha.verma@example.com',    '+919800000121', 'MCA', 8.50, 2026),
('MCA26002', 'Rishi',  'Kapoor',  'rishi.kapoor@example.com',  '+919800000122', 'MCA', 7.40, 2026),
('MCA26003', 'Tanvi',  'Shah',    'tanvi.shah@example.com',    '+919800000123', 'MCA', 8.10, 2026),
('MCA26004', 'Deepak', 'Chauhan', 'deepak.chauhan@example.com','+919800000124', 'MCA', 6.70, 2026),
('MCA26005', 'Aditi',  'Bansal',  'aditi.bansal@example.com',  '+919800000125', 'MCA', 9.00, 2026),
('MCA26006', 'Yash',   'Agarwal', 'yash.agarwal@example.com',  '+919800000126', 'MCA', 7.70, 2026),
('MCA26007', 'Ritu',   'Saxena',  'ritu.saxena@example.com',   '+919800000127', 'MCA', 7.00, 2026),
('MCA26008', 'Manish', 'Pandey',  'manish.pandey@example.com', '+919800000128', 'MCA', 6.50, 2026),
('MCA26009', 'Shreya', 'Ghosh',   'shreya.ghosh@example.com',  '+919800000129', 'MCA', 8.80, 2026),
('MCA26010', 'Varun',  'Bhatia',  'varun.bhatia@example.com',  '+919800000130', 'MCA', 7.25, 2026);

-- Safety check: roll everything back if a row was lost
IF (SELECT COUNT(*) FROM Skill) <> 15
   OR (SELECT COUNT(*) FROM Company) <> 6
   OR (SELECT COUNT(*) FROM Student) <> 30
    THROW 50090, 'Seed part 1: unexpected row counts, rolled back.', 1;

COMMIT;
GO

-- Expected: Skill 15, Company 6, Student 30 (10 per graduation year 2024, 2025, 2026)
SELECT 'Skill' AS TableName, COUNT(*) AS Rows FROM Skill
UNION ALL SELECT 'Company', COUNT(*) FROM Company
UNION ALL SELECT 'Student', COUNT(*) FROM Student;
GO
