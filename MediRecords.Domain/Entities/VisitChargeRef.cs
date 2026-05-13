using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MediRecords.Domain.Entities;

[Table("VisitChargeRef")]
public class VisitChargeRef
{
    [Key]
    public int ChargeId { get; set; }

    
    [ForeignKey("EncounterIdNavigation")]
    public int EncounterId { get; set; }

    [ForeignKey("ProcedureCodeNavigation")]
    public int CodeId { get; set; }

    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal Amount { get; set; }

    public bool Status { get; set; }

    public virtual Encounter? EncounterIdNavigation { get; set; }
    public virtual ProcedureCode? ProcedureCodeNavigation { get; set; }
}