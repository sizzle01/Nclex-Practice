using Microsoft.Extensions.DependencyInjection;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Tests.ItemTypes;

public sealed class ItemTypeRegistrationTests
{
    [Fact]
    public void Every_mvp_item_type_has_a_registered_module()
    {
        using var services = new ServiceCollection().AddContentImport().BuildServiceProvider();
        var registry = services.GetRequiredService<ItemTypeRegistry>();

        registry.MissingModules(Phase.Mvp).ShouldBeEmpty();
        Should.NotThrow(services.EnsureItemTypeModules);
    }

    [Fact]
    public void Each_item_type_is_registered_once()
    {
        using var services = new ServiceCollection().AddContentImport().BuildServiceProvider();

        var types = services.GetServices<IItemTypeModule>().Select(m => m.Type).ToList();

        types.ShouldBeUnique();
    }

    [Fact]
    public void Phase_2_types_are_not_registered_yet()
    {
        using var services = new ServiceCollection().AddContentImport().BuildServiceProvider();

        services.GetServices<IItemTypeModule>().ShouldAllBe(m => m.Phase == Phase.Mvp);
    }
}
