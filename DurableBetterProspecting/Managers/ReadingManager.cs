using Common.Mod.Common.Config;
using DurableBetterProspecting.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

public class ReadingManager
{
    private readonly ICoreAPI _api;
    private readonly ILogger _logger;
    private readonly IConfigSystem _configSystem;

    public ReadingManager(ICoreAPI api, ILogger logger, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger;
        _configSystem = configSystem;
    }

    public void ProcessReading(ReadingPacket packet)
    {
        if (_api is not ICoreClientAPI clientApi)
        {
            _logger.Warning("Received reading packet on the server. This is probably a bug.");
            return;
        }

        _logger.Verbose("Received reading packet with {0} readings", packet.Readings.Length);
        clientApi.World.Player.ShowChatNotification($"Received reading packet with {packet.Readings.Length} readings");
    }
}
