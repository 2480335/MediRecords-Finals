using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities
{
    [Table("PrescriptionItem")]
    public class PrescriptionItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [ForeignKey("PrescriptionIdNavigation")]
        public int PrescriptionId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(100)")]
        public string DrugName { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string Dose { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string Frequency { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string Route { get; set; } = null!;

        public int DurationDays { get; set; }
        
        public string Instructions { get; set; } = null!;
        
        public virtual Prescription? PrescriptionIdNavigation { get; set; }
    }
}