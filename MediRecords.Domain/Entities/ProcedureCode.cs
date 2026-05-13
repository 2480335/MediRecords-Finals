using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("ProcedureCode")]
public class ProcedureCode
{
    [Key]
    public int CodeId { get; set; }

    [Column(TypeName = "VARCHAR(20)")]
    public string? Code { get; set; }

    [Column(TypeName = "VARCHAR(255)")]
    public string? Description { get; set; }

    [Column(TypeName = "DECIMAL(18,2)")]
    public decimal Price { get; set; }

    public virtual ICollection<VisitChargeRef> VisitChargeRefs { get; set; } = new List<VisitChargeRef>();
}