using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Common.Mod.Common.Config;
using Common.Mod.Exceptions;
using DurableBetterProspecting.Core;
using DurableBetterProspecting.Network;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

/// <summary>
/// Manages the creation of map markers
/// <br/><br/>
/// <b>Side:</b> Server
/// </summary>
public class MarkerManager
{
    private static readonly MethodInfo ResendWaypointsMethod =
        typeof(WaypointMapLayer).GetMethod("ResendWaypoints", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly ILogger _logger;
    private readonly IConfigSystem _configSystem;

    private readonly WorldMapManager? _mapManager;

    private DurableBetterProspectingCommonConfig? _commonConfig;

    public MarkerManager(ICoreAPI api, EnumAppSide side, ILogger logger, IServerNetworkChannel channel, IConfigSystem configSystem)
    {
        if (side is not EnumAppSide.Server)
        {
            throw new InvalidSideException(side);
        }

        _logger = logger.Named("MarkerManager");
        _configSystem = configSystem;

        channel.SetMessageHandler<MarkerPacket>(ProcessMarker);

        _mapManager = api.ModLoader.GetModSystem<WorldMapManager>();

        _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
        _configSystem.Updated += type =>
        {
            if (type is RootConfigType.Common)
            {
                _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
            }
        };
    }

    private void ProcessMarker(IServerPlayer player, MarkerPacket packet)
    {
        _logger.Verbose("Received packet for player {0} (ID {1})", player.PlayerName, player.PlayerUID);
        var stopwatch = Stopwatch.StartNew();

        if (!_commonConfig!.Marker.Allowed)
        {
            _logger.Verbose("Marker creation is disallowed by the server");

            stopwatch.Stop();
            _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

            return;
        }

        if (_mapManager!.MapLayers.FirstOrDefault(l => l is WaypointMapLayer) is not WaypointMapLayer mapLayer)
        {
            _logger.Error("Could not find waypoint map layer");

            stopwatch.Stop();
            _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

            return;
        }

        foreach (var waypoint in mapLayer.Waypoints.Where(w => w.OwningPlayerUid == player.PlayerUID))
        {
            var deltaX = Math.Abs(waypoint.Position.X - packet.Position.X);
            var deltaZ = Math.Abs(waypoint.Position.Z - packet.Position.Z);
            var proximity = (int)Math.Max(deltaX, deltaZ);
            var threshold = _commonConfig.Marker.Threshold;

            if (proximity < threshold && waypoint.Title.StartsWith(Constants.MarkerPrefix))
            {
                _logger.Verbose("Skipping marker creation due to the proximity of {0} blocks being below the threshold of {1} blocks", proximity, threshold);

                stopwatch.Stop();
                _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

                return;
            }
        }

        var newWaypoint = new Waypoint
        {
            Position = new Vec3d(packet.Position.X, packet.Position.Y, packet.Position.Z),
            Title = packet.Text,
            Text = string.Empty,
            Color = Color.Orange.ToArgb(),
            Icon = "circle",
            ShowInWorld = false,
            Pinned = false,
            OwningPlayerUid = player.PlayerUID,
            Temporary = false
        };
        mapLayer.Waypoints.Add(newWaypoint);

        stopwatch.Stop();
        _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

        ResendWaypointsMethod.Invoke(mapLayer, [player]);
    }
}
