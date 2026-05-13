using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;

namespace MediRecords.Domain.Entities;

[Table("Notification")]
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [ForeignKey("UserIdNavigation")]
    public int UserId { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(MAX)")]
    public string Message { get; set; } = null!;

    [Column(TypeName = "VARCHAR(50)")]
    public string Category { get; set; } = null!;  //(Appointment/Lab/Rx/FollowUp)

    public bool Status { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    /*-------------------------Foreign Key References--------------------------------*/
    public virtual User? UserIdNavigation { get; set; }
}
