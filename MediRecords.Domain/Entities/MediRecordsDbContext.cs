using System;
using DotNetEnv;
using MediRecords.Domain.Enums;
using MediRecords.Models;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Domain.Entities;

public class MediRecordsDbContext : DbContext
{
    public MediRecordsDbContext() { }
    public MediRecordsDbContext(DbContextOptions<MediRecordsDbContext> options) : base(options) { }

    public virtual DbSet<Allergy> Allergies { get; set; }
    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<CarePlan> CarePlans { get; set; }
    public virtual DbSet<ClinicalTemplate> ClinicalTemplates { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<Encounter> Encounters { get; set; }
    public virtual DbSet<FollowUp> FollowUps { get; set; }
    public virtual DbSet<ImagingOrder> ImagingOrders { get; set; }
    public virtual DbSet<ImagingReport> ImagingReports { get; set; }
    public virtual DbSet<Immunization> Immunizations { get; set; }
    public virtual DbSet<LabOrder> LabOrders { get; set; }
    public virtual DbSet<LabResult> LabResults { get; set; }
    public virtual DbSet<MedicalHistory> MedicalHistories { get; set; }
    public virtual DbSet<MedicationList> MedicationLists { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<NursingNote> NursingNotes { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<Prescription> Prescriptions { get; set; }
    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public virtual DbSet<ProblemList> ProblemLists { get; set; }
    public virtual DbSet<ProcedureCode> ProcedureCodes { get; set; }
    public virtual DbSet<ProviderSchedule> ProviderSchedules { get; set; }
    public virtual DbSet<SOAPNote> SOAPNotes { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<VisitChargeRef> VisitChargeRefs { get; set; }
    public virtual DbSet<VitalSign> VitalSign { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        if (!optionsBuilder.IsConfigured) {
            
            Env.Load();
            // 1) Prefer environment variable during library-only development
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException(
                "Connection string not found. " +
                "Set ConnectionStrings__DefaultConnection in environment variables or .env");
            }

            optionsBuilder.UseSqlServer(cs);
        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User and Security
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);

            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Password).HasColumnType("nvarchar(max)");

            e.HasOne(x => x.RoleIdNavigation)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.RoleId);
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.AuditId);

            e.HasOne(x => x.UserIdNavigation)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);

            e.HasOne(x => x.UserIdNavigation)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Patient Core
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(x => x.PatientId);

            e.Property(x => x.MRN)
                .HasMaxLength(50)
                .IsRequired();

            e.HasIndex(x => x.MRN).IsUnique();

            e.Property(x => x.AddressJSON).HasColumnType("nvarchar(max)");

            e.Property(x => x.Status).HasConversion<string>();
            
            e.HasOne(x => x.PrimaryProviderIdNavigation)
                .WithMany(u => u.Patients)
                .HasForeignKey(x => x.PrimaryProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //Patient-linked Tables
        // Explicit configuration for Patient-linked tables to set up cascade delete from Patient -> Child, otherwise EF Core will default to Restrict which causes issues when deleting patients with existing records.
        modelBuilder.Entity<ProblemList>(e =>
        {
            e.ToTable("ProblemList");

            e.HasKey(x => x.ProblemId);

            e.Property(x => x.Status).HasConversion<string>();
            
            e.Property(x => x.ProblemId)
                .ValueGeneratedOnAdd();

            e.Property(x => x.Diagnosis)
                .IsRequired()
                .HasMaxLength(255);

            e.Property(x => x.Status)
                .HasMaxLength(20);

            e.Property(x => x.StartDate)
                .IsRequired();

            e.Property(x => x.EndDate);

            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.ProblemLists)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Allergy>(e =>
        {
            e.ToTable("Allergy");

            e.HasKey(x => x.AllergyId);

            e.Property(x => x.Status).HasConversion<string>();

            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.Allergies)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicalHistory>(e =>
        {
            e.ToTable("MedicalHistory");

            e.HasKey(x => x.HistoryId);
            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicationList>(e =>
        {
            e.ToTable("MedicationList");

            e.HasKey(x => x.MedId);
            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.MedicationLists)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Immunization>(e =>
        {
            e.ToTable("Immunization");

            e.HasKey(x => x.ImmunizationId);
            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.Immunizations)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CarePlan>(e =>
        {
            e.ToTable("CarePlan");

            e.HasKey(x => x.CarePlanId);
            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.CarePlans)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //Encounter and Notes
        modelBuilder.Entity<Encounter>(e =>
        {
            e.HasKey(x => x.EncounterId);

            e.Property(x => x.Status)
                .HasColumnType("INT")
                .HasConversion(
                    v => (int)v,
                    v => (EncounterStatus)v);

            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.Encounters)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ProviderIdNavigation)
                .WithMany(u => u.Encounters)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SOAPNote>(e =>
        {
            e.HasKey(x => x.NoteId);

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.SOAPNotes)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NursingNote>(e =>
        {
            e.HasKey(x => x.NursingNoteId);

            e.HasOne(x => x.EncounterNavigation)
                .WithMany(e => e.NursingNotes)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

         modelBuilder.Entity<VitalSign>(e =>
        {
            e.HasKey(x => x.VitalsId);

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.VitalSigns)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // Scheduling
        modelBuilder.Entity<ProviderSchedule>(e =>
        {
            e.HasKey(x => x.ScheduleId);

            e.HasOne(x => x.ProviderIdNavigation)
                .WithMany(u => u.ProviderSchedules)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(x => x.AppointmentId);

            e.Property(x => x.Status).HasConversion<string>();
            
            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.Appointments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ProviderIdNavigation)
                .WithMany(u => u.Appointments)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FollowUp>(e =>
        {
            e.HasKey(x => x.FollowupId);

            e.Property(x => x.EncounterId)
                .IsRequired();

            e.Property(x => x.RecommendedDate)
                .IsRequired();

            e.Property(x => x.Notes)
                .HasColumnType("VARCHAR(MAX)")
                .IsRequired();

            e.Property(x => x.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(enc => enc.FollowUps)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        //Orders and Diagnostics
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(x => x.PrescriptionId);

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.Prescriptions)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrescriptionItem>(e =>
        {
            e.HasKey(x => x.ItemId);

            e.HasOne(x => x.PrescriptionIdNavigation)
                .WithMany(p => p.PrescriptionItems)
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LabOrder>(e =>
        {
            e.HasKey(x => x.LabOrderId);
            e.Property(x => x.TestJson).HasColumnType("nvarchar(max)");

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.LabOrders)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.UserIdNavigation)
                .WithMany(u => u.LabOrders)
                .HasForeignKey(x => x.OrderedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LabResult>(e =>
        {
            e.HasKey(x => x.ResultId);
            e.Property(x => x.ResultJson).HasColumnType("nvarchar(max)");

            e.HasOne(x => x.LabOrderIdNavigation)
                .WithMany(o => o.LabResults)
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicalHistory>(e =>
        {
            e.HasKey(x => x.HistoryId);

            e.Property(x => x.PatientId)
                .IsRequired();

            e.Property(x => x.Condition)
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.Notes);

            e.Property(x => x.RecordedDate)
                .HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Imaging
        modelBuilder.Entity<ImagingOrder>(e =>
        {
            e.HasKey(x => x.ImagingOrderId);

            e.Property(x => x.StudyType).HasConversion<string>();

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.ImagingOrders)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImagingReport>(e =>
        {
            e.HasKey(x => x.ReportId);

            e.HasOne(x => x.ImagingOrderIdNavigation)
                .WithMany(o => o.ImagingReports)
                .HasForeignKey(x => x.ImagingOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClinicalTemplate>(e =>
        {
            e.HasKey(x => x.TemplateId);

            e.Property(x => x.TemplateId)
                .ValueGeneratedNever();

            e.Property(x => x.Name)
                .HasColumnType("varchar(100)")
                .IsRequired();

            e.Property(x => x.TemplateType)
                .HasColumnType("varchar(50)")
                .IsRequired();

            e.Property(x => x.ContentJSON)
                .HasColumnType("nvarchar(max)");

            e.Property(x => x.CreatedBy)
                .IsRequired();

            e.Property(x => x.CreatedDate)
                .IsRequired();

            e.Property(x => x.Status)
                .IsRequired();

            e.HasOne(x => x.CreatedByNavigation)
                .WithMany(u => u.ClinicalTemplates)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //Billing and Documents
        modelBuilder.Entity<ProcedureCode>(e =>
        {
            e.HasKey(x => x.CodeId);
            e.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<VisitChargeRef>(e =>
        {
            e.HasKey(x => x.ChargeId);
            e.Property(x => x.Amount).HasPrecision(18, 2);

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.VisitChargeRefs)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ProcedureCodeNavigation)
                .WithMany(e => e.VisitChargeRefs)
                .HasForeignKey(x => x.CodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.HasKey(x => x.DocumentID);

            e.HasOne(x => x.PatientIdNavigation)
                .WithMany(p => p.Documents)
                .HasForeignKey(x => x.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EncounterIdNavigation)
                .WithMany(e => e.Documents)
                .HasForeignKey(x => x.EncounterID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
