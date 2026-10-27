namespace Nclex.Domain.ItemTypes;

/// <summary>
/// Finds the module for an item type. Every module is registered in DI, and this collects them
/// (interfaces.md section 3).
/// </summary>
public sealed class ItemTypeRegistry(IEnumerable<IItemTypeModule> modules)
{
    private readonly Dictionary<ItemType, IItemTypeModule> _byType = modules.ToDictionary(m => m.Type);

    /// <summary>True when the type has a module, so items of that type can be authored and served.</summary>
    public bool IsAvailable(ItemType type) => _byType.ContainsKey(type);

    public bool TryGet(ItemType type, out IItemTypeModule module) => _byType.TryGetValue(type, out module!);

    /// <summary>The module for <paramref name="type"/>; throws when the type has none yet.</summary>
    public IItemTypeModule Get(ItemType type) =>
        _byType.TryGetValue(type, out var module)
            ? module
            : throw new InvalidOperationException($"No item-type module is registered for {type}.");

    /// <summary>Item types of <paramref name="enabledPhase"/>, or an earlier phase, that have no module.</summary>
    public IReadOnlyList<ItemType> MissingModules(Phase enabledPhase) =>
        [.. ItemTypeCatalog.All.Where(d => d.Phase <= enabledPhase && !IsAvailable(d.Type)).Select(d => d.Type)];

    /// <summary>
    /// The startup check from interfaces.md section 3: every item type whose phase is enabled must have
    /// a module. Throws, so a forgotten registration stops the host from starting instead of failing
    /// the first candidate who meets that item type.
    /// </summary>
    public void EnsureEveryTypeHasAModule(Phase enabledPhase)
    {
        var missing = MissingModules(enabledPhase);
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"No item-type module is registered for these {enabledPhase} item types: {string.Join(", ", missing)}.");
        }
    }
}
