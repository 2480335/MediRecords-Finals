using System.Runtime.Serialization;

namespace MediRecords.Domain.Enums;

public enum UserRoleEnums
{
    [EnumMember(Value = "Admin")]
    Admin,
    [EnumMember(Value = "Physician")]
    Physician,
    [EnumMember(Value = "Nurse")]
    Nurse,
    [EnumMember(Value = "LabTech")]
    LabTech,
    [EnumMember(Value = "FrontDesk")]
    FrontDesk

    
}
