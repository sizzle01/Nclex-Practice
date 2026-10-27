using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.MultipleChoice;
using Nclex.Domain.ItemTypes.Sata;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class ItemTypeRegistryTests
{
    private readonly ItemTypeRegistry _registry = new([new MultipleChoiceModule(), new SataModule()]);

    [Fact]
    public void Finds_a_registered_module_by_type()
    {
        _registry.Get(ItemType.Sata).ShouldBeOfType<SataModule>();
        _registry.IsAvailable(ItemType.MultipleChoice).ShouldBeTrue();
    }

    [Fact]
    public void A_type_without_a_module_is_unavailable()
    {
        _registry.IsAvailable(ItemType.SelectN).ShouldBeFalse();
        _registry.TryGet(ItemType.SelectN, out _).ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => _registry.Get(ItemType.SelectN));
    }

    [Fact]
    public void The_catalog_describes_every_item_type_once()
    {
        ItemTypeCatalog.All.Select(d => d.Type).ShouldBe(Enum.GetValues<ItemType>(), ignoreOrder: true);
    }
}
