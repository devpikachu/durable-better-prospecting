using Common.Mod.Common.Config;
using JetBrains.Annotations;

namespace DurableBetterProspecting;

[UsedImplicitly]
public class DurableBetterProspectingSystem : Common.Mod.Core.System
{
    public override string ModId() => "durablebetterprospecting";
    public override string ModVersion() => "12.34.56";
    public override string ModName() => "Durable Better Prospecting";

    protected override void RegisterConfigs(IConfigSystem configSystem)
    {
        configSystem.Register<DurableBetterProspectingCommonConfig>();
        configSystem.Register<DurableBetterProspectingClientConfig>();
    }
}
