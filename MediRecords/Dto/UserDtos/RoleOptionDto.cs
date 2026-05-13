namespace MediRecords.Dto.UserDtos;

/// <summary>
/// Lightweight projection of UserRole used to populate role dropdowns
/// on the frontend (e.g. the Admin user-registration form).
/// </summary>
public class RoleOptionDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
}
