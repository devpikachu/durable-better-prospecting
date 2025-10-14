using System.Reflection;
using Common.Mod.Common.Config;
using DurableBetterProspecting.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

public class MarkerManager
{
    private static readonly MethodInfo ResendWaypointsMethod =
        typeof(WaypointMapLayer).GetMethod("ResendWaypoints", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo RebuildMapComponentsMethod =
        typeof(WaypointMapLayer).GetMethod("RebuildMapComponents", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly ILogger _logger;
    private readonly IConfigSystem _configSystem;

    private readonly WorldMapManager? _mapManager;

    private DurableBetterProspectingCommonConfig? _commonConfig;

    public MarkerManager(ICoreAPI api, ILogger logger, IConfigSystem configSystem)
    {
        _logger = logger;
        _configSystem = configSystem;

        DurableBetterProspectingSystem.Instance!.ServerRegisterMessageTypes += OnServerRegisterMessageTypes;
        DurableBetterProspectingSystem.Instance.ClientRegisterMessageTypes += OnClientRegisterMessageTypes;

        if (api is not ICoreServerAPI serverApi)
        {
            return;
        }

        _mapManager = serverApi.ModLoader.GetModSystem<WorldMapManager>();
        _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
        _configSystem.Updated += type =>
        {
            if (type is RootConfigType.Common)
            {
                _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
            }
        };
    }

    private void OnServerRegisterMessageTypes(IServerNetworkChannel channel)
    {
        channel
            .RegisterMessageType<MarkingPacket>()
            .SetMessageHandler<MarkingPacket>(ProcessMarking);
    }

    private void OnClientRegisterMessageTypes(IClientNetworkChannel channel)
    {
        channel
            .RegisterMessageType<MarkingPacket>()
            .SetMessageHandler<MarkingPacket>(_ => { });
    }

    private void ProcessMarking(IServerPlayer player, MarkingPacket packet)
    {
        _logger.Verbose($"Processing marking packet for {player.PlayerName} ({player.PlayerUID})");
    }
}
