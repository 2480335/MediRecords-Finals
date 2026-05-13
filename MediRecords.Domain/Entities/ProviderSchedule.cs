using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("ProviderSchedule")]
public class ProviderSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    [ForeignKey("ProviderIdNavigation")]
    public int ProviderId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public int SlotDuration { get; set; }

    [Required]
    public bool Status { get; set; }

    public virtual User? ProviderIdNavigation { get; set; }
}