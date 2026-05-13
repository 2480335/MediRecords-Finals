using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("User")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(30)")]
    public string Name { get; set; } = null!;

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [ForeignKey("RoleIdNavigation")]
    public int RoleId { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(100)")]
    public string Email { get; set; } = null!;

    [Column(TypeName = "VARCHAR(15)")]
    public string Phone { get; set; } = null!;

    [Required]
    [Column(TypeName = "NVARCHAR(MAX)")]
    public string Password { get; set; } = null!;

    public bool Status { get; set; }

    /*-------------------------Foreign Key References--------------------------------*/
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<ProviderSchedule> ProviderSchedules { get; set; }= new List<ProviderSchedule>();
    public virtual ICollection<Patient> Patients { get; set; }= new List<Patient>();
    public virtual ICollection<ClinicalTemplate> ClinicalTemplates { get; set; } = new List<ClinicalTemplate>();
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual UserRole? RoleIdNavigation { get; set; }
}
