using Common.Mod.Common.Config;
using Common.Mod.Core;
using DryIoc;
using DurableBetterProspecting.Items;
using DurableBetterProspecting.Managers;
using DurableBetterProspecting.Network;
using JetBrains.Annotations;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

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
        Container.Register<LegacyConfigManager>(Reuse.Singleton);
        Container.Register<ModeManager>(Reuse.Singleton);

        // Make sure that legacy configuration is migrated if it exists
        {
            var legacyConfigManager = Container.Resolve<LegacyConfigManager>();
            legacyConfigManager.Migrate();
        }

        // Make sure that managers are instantiated
        {
            // ReSharper disable UnusedVariable
            var modeManager = Container.Resolve<ModeManager>();
            // ReSharper restore UnusedVariable
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

    protected override void ServerRegisterNetworkMessages(IServerNetworkChannel channel)
    {
        base.ServerRegisterNetworkMessages(channel);
        channel.RegisterMessageType<ReadingPacket>();
        channel.RegisterMessageType<MarkerPacket>();
    }

    protected override void ClientRegisterNetworkMessages(IClientNetworkChannel channel)
    {
        base.ClientRegisterNetworkMessages(channel);
        channel.RegisterMessageType<ReadingPacket>();
        channel.RegisterMessageType<MarkerPacket>();
    }

    protected override void ServerStartPre(ICoreServerAPI api)
    {
        base.ServerStartPre(api);
        Container.Register<MarkerManager>(Reuse.Singleton);

        // Make sure that managers are instantiated
        {
            // ReSharper disable UnusedVariable
            var markerManager = Container.Resolve<MarkerManager>();
            // ReSharper restore UnusedVariable
        }
    }

    protected override void ClientStartPre(ICoreClientAPI api)
    {
        base.ClientStartPre(api);
        Container.Register<ReadingManager>(Reuse.Singleton);

        // Make sure that managers are instantiated
        {
            // ReSharper disable UnusedVariable
            var readingManager = Container.Resolve<ReadingManager>();
            // ReSharper restore UnusedVariable
        }
    }
}
