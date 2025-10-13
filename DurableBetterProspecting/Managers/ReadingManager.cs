using System.Text;
using Common.Mod.Common.Config;
using DurableBetterProspecting.Core;
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

    private DurableBetterProspectingClientConfig? _clientConfig;

    public ReadingManager(ICoreAPI api, ILogger logger, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger;
        _configSystem = configSystem;

        if (api is ICoreClientAPI)
        {
            _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
            _configSystem.Updated += type =>
            {
                if (type is RootConfigType.Client)
                {
                    _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
                }
            };
        }
    }

    public void ProcessReading(ReadingPacket packet)
    {
        if (_api is not ICoreClientAPI clientApi)
        {
            _logger.Warning("Received reading packet on the server. This is probably a bug.");
            return;
        }

        _logger.Verbose("Received reading packet with {0} readings", packet.Readings?.Length ?? 0);

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendFormat("{0} sample taken within an area of {1} block(s)\n", packet.Mode, packet.SampleSize);

        if ((packet.Readings?.Length ?? 0) == 0)
        {
            messageBuilder.AppendLine("No ore/rock found");
            clientApi.ShowChatMessage(messageBuilder.ToString());
            return;
        }

        var readings = packet.Readings!.AsEnumerable();
        if (_clientConfig!.Ordering.Enabled)
        {
            var ascending = _clientConfig.Ordering.Direction is OrderingDirection.Ascending;

            if (packet.Mode is SampleMode.Rock or SampleMode.Distance)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.Distance)
                    : readings.OrderByDescending(r => r.Distance);
            }

            if (packet.Mode is SampleMode.Column)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.BlockId)
                    : readings.OrderByDescending(r => r.BlockId);
            }

            if (packet.Mode is SampleMode.Quantity)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.Quantity)
                    : readings.OrderByDescending(r => r.Quantity);
            }
        }

        messageBuilder.AppendLine("Found the following ore/rock:");
        foreach (var reading in readings)
        {
            messageBuilder.AppendFormat("<a href=\"{0}\">{1}</a>\n", reading.HandbookLink, reading.BlockId);
        }

        clientApi.ShowChatMessage(messageBuilder.ToString());
    }
}
