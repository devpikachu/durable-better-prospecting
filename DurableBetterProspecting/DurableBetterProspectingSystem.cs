using Common.Mod.Common.Config;
using DurableBetterProspecting.Items;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

[UsedImplicitly]
public class DurableBetterProspectingSystem : Common.Mod.Core.System
{
    public static DurableBetterProspectingSystem? Instance { get; private set; }

    public override string ModId() => "durablebetterprospecting";
    public override string ModVersion() => "12.34.56";
    public override string ModName() => "Durable Better Prospecting";

    public override bool ShouldLoad(EnumAppSide forSide) => true;

    public DurableBetterProspectingSystem()
    {
        Instance = this;
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass(ItemProspectingPick.ItemRegistryId, typeof(ItemProspectingPick));
    }

    protected override void RegisterConfigs(IConfigSystem configSystem)
    {
        configSystem.Register<DurableBetterProspectingCommonConfig>();
        configSystem.Register<DurableBetterProspectingClientConfig>();
    }
}
