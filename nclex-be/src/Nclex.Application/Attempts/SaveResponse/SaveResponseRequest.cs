using System.Text.Json;

namespace Nclex.Application.Attempts.SaveResponse;

/// <summary>Body of PUT /attempts/{id}/items/{position}/response (api.md section 4).</summary>
/// <param name="Response">The answer in the item type's response shape; null clears it.</param>
/// <param name="ClientSeq">Goes up with every write from the player; older writes are ignored.</param>
public sealed record SaveResponseRequest(JsonElement? Response, int ClientSeq);
