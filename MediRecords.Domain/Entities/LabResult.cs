using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("LabResult")]
public class LabResult
{
    [Key]
    public int ResultId { get; set; }

    [Required]
    [ForeignKey("LabOrderIdNavigation")]
    public int LabOrderId { get; set; }

    [Column(TypeName = "VARCHAR(MAX)")]
    public string? ResultJson { get; set; }

    public DateTime ResultDate { get; set; }
    
    public bool Status { get; set; }  // Final/Corrected

    /*-------------------------Foreign Key References--------------------------------*/
    public virtual LabOrder? LabOrderIdNavigation { get; set; }
}
