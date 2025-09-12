using System;
using DurableBetterProspecting.Items;
using DurableBetterProspecting.Network;
using JetBrains.Annotations;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DurableBetterProspecting;

[UsedImplicitly]
public class ModSystem : Vintagestory.API.Common.ModSystem
{
    public const string ModId = "durablebetterprospecting";

    private ICoreAPI? _api;
    private IServerNetworkChannel? _channel;

    public override void StartPre(ICoreAPI api)
    {
        _api = api;

        try
        {
            ModConfig.LoadAndSave(_api);
        }
        catch (Exception ex)
        {
            Mod.Logger.Error($"Could not load config for {ModId}! Loading defaults instead.");
            Mod.Logger.Error(ex);
        }
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("ItemProspectingPick", typeof(ItemDurableBetterProspectingPick));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        _channel = api.Network.RegisterChannel(ModId)
            .RegisterMessageType<ConfigPacket>()
            .SetMessageHandler<ConfigPacket>((_, _) => { });

        api.Event.PlayerJoin += OnPlayerJoin;
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Network.RegisterChannel(ModId)
            .RegisterMessageType<ConfigPacket>()
            .SetMessageHandler<ConfigPacket>(ModConfig.SynchronizeConfig);
    }

    public override bool ShouldLoad(EnumAppSide forSide) => true;

    public override void Dispose()
    {
        if (_api is not ICoreServerAPI serverApi)
        {
            return;
        }

        serverApi.Event.PlayerJoin -= OnPlayerJoin;
    }

    private void OnPlayerJoin(IServerPlayer player)
    {
        _channel!.SendPacket(ConfigPacket.FromConfig(ModConfig.Loaded), player);
    }
}
