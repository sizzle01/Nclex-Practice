using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Nclex.Domain.ItemTypes;

/// <summary>
/// Shuffles a list in an order fixed by a seed string, e.g. "{attemptId}:{position}", so a candidate
/// sees the same option order every time they reopen an item, on any server (interfaces.md 3, Present).
/// </summary>
public static class SeededShuffle
{
    /// <summary>Returns a new list with the items of <paramref name="items"/> in seeded order.</summary>
    public static IReadOnlyList<T> Shuffle<T>(IReadOnlyList<T> items, string seed)
    {
        var result = items.ToArray();

        // Fisher-Yates. Each swap index comes from SHA-256 of the seed and the step number, because
        // string.GetHashCode and System.Random's seeding are not guaranteed to be the same across
        // processes or .NET versions, and a reloaded item must never change order.
        for (var i = result.Length - 1; i > 0; i--)
        {
            var j = (int)(RandomUInt(seed, i) % (uint)(i + 1));
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }

    private static uint RandomUInt(string seed, int step)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{seed}:{step}"));
        return BinaryPrimitives.ReadUInt32LittleEndian(hash);
    }
}
