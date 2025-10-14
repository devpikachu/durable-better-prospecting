using Common.Mod.Common.Config;
using Common.Mod.Core;
using DryIoc;
using DurableBetterProspecting.Items;
using DurableBetterProspecting.Managers;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

[UsedImplicitly]
public class DurableBetterProspectingSystem : System<DurableBetterProspectingSystem>
{
    public override string ModId() => "durablebetterprospecting";
    public override string ModVersion() => "12.34.56";
    public override string ModName() => "Durable Better Prospecting";

    public override bool ShouldLoad(EnumAppSide forSide) => true;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Container.Register<MarkerManager>(Reuse.Singleton);
        Container.Register<ModeManager>(Reuse.Singleton);
        Container.Register<ReadingManager>(Reuse.Singleton);

        // Make sure that managers are instantiated
        {
            // ReSharper disable once UnusedVariable
            var markerManager = Container.Resolve<MarkerManager>();
            // ReSharper disable once UnusedVariable
            var readingManager = Container.Resolve<ReadingManager>();
        }
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.RegisterItemClass(ItemProspectingPick.ItemRegistryId, typeof(ItemProspectingPick));
    }

    protected override void RegisterConfigs(IConfigSystem configSystem)
    {
        configSystem.Register<DurableBetterProspectingCommonConfig>();
        configSystem.Register<DurableBetterProspectingClientConfig>();
    }
}
