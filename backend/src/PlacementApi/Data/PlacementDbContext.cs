using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Data.Entities;

namespace PlacementApi.Data;

public partial class PlacementDbContext : DbContext
{
    public PlacementDbContext(DbContextOptions<PlacementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobSkill> JobSkills { get; set; }

    public virtual DbSet<Placement> Placements { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentSkill> StudentSkills { get; set; }

    public virtual DbSet<VwApplicationDetail> VwApplicationDetails { get; set; }

    public virtual DbSet<VwCompanyHiringSummary> VwCompanyHiringSummaries { get; set; }

    public virtual DbSet<VwOpenJob> VwOpenJobs { get; set; }

    public virtual DbSet<VwPlacementDetail> VwPlacementDetails { get; set; }

    public virtual DbSet<VwPlacementStatsByBatch> VwPlacementStatsByBatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.Email, "UQ_AppUser_Email").IsUnique();

            entity.HasIndex(e => e.StudentId, "UX_AppUser_StudentId")
                .IsUnique()
                .HasFilter("([StudentId] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(10);

            entity.HasOne(d => d.Student).WithOne(p => p.AppUser)
                .HasForeignKey<AppUser>(d => d.StudentId)
                .HasConstraintName("FK_AppUser_Student");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Application");

            entity.HasIndex(e => e.JobId, "IX_Application_JobId");

            entity.HasIndex(e => new { e.StudentId, e.JobId }, "UQ_Application_Student_Job").IsUnique();

            entity.Property(e => e.AppliedOn).HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_Application_AppliedOn");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .HasDefaultValue("Applied", "DF_Application_Status");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Application_Job");

            entity.HasOne(d => d.Student).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Application_Student");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.HasIndex(e => e.CompanyName, "UQ_Company_CompanyName").IsUnique();

            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Company_IsActive");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("Job");

            entity.HasIndex(e => e.CompanyId, "IX_Job_CompanyId");

            entity.HasIndex(e => new { e.Status, e.Deadline }, "IX_Job_Status_Deadline");

            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MinCgpa).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.PackageLpa).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValue("Open", "DF_Job_Status");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Job_Company");
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillId });

            entity.ToTable("JobSkill");

            entity.HasIndex(e => e.SkillId, "IX_JobSkill_SkillId");

            entity.Property(e => e.IsMandatory).HasDefaultValue(true, "DF_JobSkill_IsMandatory");

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobSkill_Job");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobSkill_Skill");
        });

        modelBuilder.Entity<Placement>(entity =>
        {
            entity.ToTable("Placement");

            entity.HasIndex(e => e.ApplicationId, "UQ_Placement_ApplicationId").IsUnique();

            entity.Property(e => e.AcademicYear).HasMaxLength(9);
            entity.Property(e => e.PackageLpa).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PlacementDate).HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_Placement_Date");

            entity.HasOne(d => d.Application).WithOne(p => p.Placement)
                .HasForeignKey<Placement>(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Placement_Application");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skill");

            entity.HasIndex(e => e.SkillName, "UQ_Skill_SkillName").IsUnique();

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Skill_IsActive");
            entity.Property(e => e.SkillName).HasMaxLength(100);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.HasIndex(e => e.Email, "UQ_Student_Email").IsUnique();

            entity.HasIndex(e => e.RollNo, "UQ_Student_RollNo").IsUnique();

            entity.Property(e => e.Cgpa).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Course).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Student_IsActive");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.RollNo).HasMaxLength(20);
        });

        modelBuilder.Entity<StudentSkill>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.SkillId });

            entity.ToTable("StudentSkill");

            entity.HasIndex(e => e.SkillId, "IX_StudentSkill_SkillId");

            entity.Property(e => e.Proficiency)
                .HasMaxLength(15)
                .HasDefaultValue("Beginner", "DF_StudentSkill_Proficiency");

            entity.HasOne(d => d.Skill).WithMany(p => p.StudentSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentSkill_Skill");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSkills)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentSkill_Student");
        });

        modelBuilder.Entity<VwApplicationDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ApplicationDetails");

            entity.Property(e => e.Cgpa).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.JobTitle).HasMaxLength(150);
            entity.Property(e => e.RollNo).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(15);
            entity.Property(e => e.StudentName).HasMaxLength(201);
        });

        modelBuilder.Entity<VwCompanyHiringSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CompanyHiringSummary");

            entity.Property(e => e.AvgPackageLpa).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.MaxPackageLpa).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwOpenJob>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_OpenJobs");

            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MinCgpa).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.PackageLpa).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        modelBuilder.Entity<VwPlacementDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PlacementDetails");

            entity.Property(e => e.AcademicYear).HasMaxLength(9);
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Course).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(150);
            entity.Property(e => e.PackageLpa).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.RollNo).HasMaxLength(20);
            entity.Property(e => e.StudentName).HasMaxLength(201);
        });

        modelBuilder.Entity<VwPlacementStatsByBatch>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PlacementStatsByBatch");

            entity.Property(e => e.PlacementPct).HasColumnType("decimal(5, 1)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
