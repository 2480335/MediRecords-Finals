using System;
using System.Text.Json.Serialization;

namespace MediRecords.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImagingOrderStudyType
{
    Xray = 1,
    CT,
    MRI,
    UltraSound
}
