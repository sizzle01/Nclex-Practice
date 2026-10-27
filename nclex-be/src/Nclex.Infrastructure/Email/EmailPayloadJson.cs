using System.Text.Json;

namespace Nclex.Infrastructure.Email;

/// <summary>
/// One set of JSON options for <c>email_outbox.payload</c>, used both when queuing (API) and when
/// rendering (worker), so the two hosts always agree on the format.
/// </summary>
internal static class EmailPayloadJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Write(object payload) => JsonSerializer.Serialize(payload, payload.GetType(), Options);

    public static T Read<T>(string payload) =>
        JsonSerializer.Deserialize<T>(payload, Options)
        ?? throw new InvalidOperationException($"The outbox payload is not a {typeof(T).Name}.");
}
