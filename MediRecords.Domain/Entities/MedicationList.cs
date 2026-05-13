using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediRecords.Domain.Enums;

namespace MediRecords.Domain.Entities
{
    [Table("MedicationList")]
    public class MedicationList
    {
        [Key]
        public int MedId { get; set; }

        [Required]
        [ForeignKey("PatientIdNavigation")]
        public int PatientId { get; set; }
        
        [Required]
        [Column(TypeName = "VARCHAR(100)")]
        public string DrugName { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string Dose { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string Frequency { get; set; } = null!;

        [Column(TypeName = "VARCHAR(50)")]
        public string? Route { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }
        
        public MedicationStatus Status { get; set; } = MedicationStatus.Active;

        public virtual Patient? PatientIdNavigation { get; set; }
    }
}