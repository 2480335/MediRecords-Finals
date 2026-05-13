using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;

namespace MediRecords.Domain.Entities;

[Table("LabOrder")]
public class LabOrder
{
    [Key]
    public int LabOrderId { get; set; }

    [Required]
    [ForeignKey("EncounterIdNavigation")]
    public int EncounterId { get; set; }

    [Required]
    [ForeignKey("UserIdNavigation")]
    public int OrderedBy { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(MAX)")]
    public string TestJson { get; set; } = null!;

    public DateTime OrderDate { get; set; }
    
    public bool Status { get; set; }

    /*-------------------------Foreign Key References--------------------------------*/
    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    public virtual Encounter? EncounterIdNavigation { get; set; }
    public virtual User? UserIdNavigation { get; set; }

}
