using System.Text.Json.Serialization;

namespace Nclex.Domain.Entities;

/// <summary>
/// Which NCLEX exam a candidate is preparing for (database enum <c>exam_track</c>).
/// Unlike other enums, its values are upper case in both the database and the API ("RN", "PN").
/// </summary>
public enum ExamTrack
{
    /// <summary>Registered Nurse.</summary>
    [JsonStringEnumMemberName("RN")]
    RN,

    /// <summary>Practical Nurse.</summary>
    [JsonStringEnumMemberName("PN")]
    PN,
}
