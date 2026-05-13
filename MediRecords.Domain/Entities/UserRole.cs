using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediRecords.Domain.Entities;

[Table("UserRole")]
public class UserRole
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(30)")]
    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
