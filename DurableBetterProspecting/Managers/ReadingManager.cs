using System.Text;
using Common.Mod.Common.Config;
using Common.Mod.Common.Core;
using DurableBetterProspecting.Core;
using DurableBetterProspecting.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

public class ReadingManager
{
    private readonly ICoreAPI _api;
    private readonly ILogger _logger;
    private readonly ITranslations _translations;
    private readonly IConfigSystem _configSystem;

    private DurableBetterProspectingClientConfig? _clientConfig;

    public ReadingManager(ICoreAPI api, ILogger logger, ITranslations translations, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger.Named(nameof(ReadingManager));
        _translations = translations;
        _configSystem = configSystem;

        DurableBetterProspectingSystem.Instance!.ServerRegisterMessageTypes += OnServerRegisterMessageTypes;
        DurableBetterProspectingSystem.Instance.ClientRegisterMessageTypes += OnClientRegisterMessageTypes;

        if (api is not ICoreClientAPI)
        {
            return;
        }

        _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
        _configSystem.Updated += type =>
        {
            if (type is RootConfigType.Client)
            {
                _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
            }
        };
    }

    private void OnServerRegisterMessageTypes(IServerNetworkChannel channel)
    {
        channel
            .RegisterMessageType<ReadingPacket>()
            .SetMessageHandler<ReadingPacket>((_, _) => { });
    }

    private void OnClientRegisterMessageTypes(IClientNetworkChannel channel)
    {
        channel
            .RegisterMessageType<ReadingPacket>()
            .SetMessageHandler<ReadingPacket>(ProcessReading);
    }

    private void ProcessReading(ReadingPacket packet)
    {
        if (_api is not ICoreClientAPI clientApi)
        {
            _logger.Warning("Received reading packet on the server. This is probably a bug.");
            return;
        }

        _logger.Verbose("Received reading packet with {0} readings", packet.Readings?.Length ?? 0);

        var messageBuilder = new StringBuilder();

        var blocksAwayString = _translations.Get("reading--blocks-away");
        var rocksString = _translations.Get("reading--rocks");
        var oresString = _translations.Get("reading--ores");
        var modeString = packet.Mode switch
        {
            SampleMode.Rock => _translations.Get("reading--mode-rock"),
            SampleMode.Column => _translations.Get("reading--mode-column"),
            SampleMode.Distance => _translations.Get("reading--mode-distance"),
            SampleMode.Quantity => _translations.Get("reading--mode-quantity"),
            _ => throw new ArgumentOutOfRangeException()
        };

        var sampleTakenString = _translations.Get("reading--sample-taken", modeString, packet.SampleSize);
        messageBuilder.AppendLine(sampleTakenString);

        if ((packet.Readings?.Length ?? 0) == 0)
        {
            var sampleEmptyString = _translations.Get("reading--sample-empty", packet.Mode is SampleMode.Rock ? rocksString : oresString);

            messageBuilder.AppendLine(sampleEmptyString);
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

        var sampleNotEmptyString = _translations.Get("reading--sample-not-empty", packet.Mode is SampleMode.Rock ? rocksString : oresString);
        messageBuilder.AppendLine(sampleNotEmptyString);

        foreach (var reading in readings)
        {
            var nameString = Lang.GetL(Lang.CurrentLocale, reading.BlockId);

            switch (packet.Mode)
            {
                case SampleMode.Rock:
                {
                    messageBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2} {3}", reading.HandbookLink, nameString, reading.Distance, blocksAwayString);

                    if (reading.Direction is not null && reading.Direction is not Direction.None && _clientConfig.ReadingDirection)
                    {
                        messageBuilder.AppendFormat(" - {0}", TranslateDirection((Direction)reading.Direction));
                    }

                    messageBuilder.Append('\n');
                    break;
                }

                case SampleMode.Column:
                    messageBuilder.AppendFormat("<a href=\"{0}\">{1}</a>\n", reading.HandbookLink, nameString);
                    break;

                case SampleMode.Distance:
                {
                    messageBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2} {3}", reading.HandbookLink, nameString, reading.Distance, blocksAwayString);

                    if (reading.Direction is not null && reading.Direction is not Direction.None && _clientConfig.ReadingDirection)
                    {
                        messageBuilder.AppendFormat(" - {0}", TranslateDirection((Direction)reading.Direction));
                    }

                    messageBuilder.Append('\n');
                    break;
                }

                case SampleMode.Quantity:
                {
                    var quantityString = TranslateQuantity(reading.Quantity);
                    messageBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2}", reading.HandbookLink, nameString, quantityString);

                    if (reading.Direction is not null && reading.Direction is not Direction.None && _clientConfig.ReadingDirection)
                    {
                        messageBuilder.AppendFormat(" - {0}", TranslateDirection((Direction)reading.Direction));
                    }

                    messageBuilder.Append('\n');
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        clientApi.ShowChatMessage(messageBuilder.ToString());
    }

    private string TranslateQuantity(int value)
    {
        return value switch
        {
            < 10 => _translations.Get("reading--amount-trace"),
            < 20 => _translations.Get("reading--amount-small"),
            < 40 => _translations.Get("reading--amount-medium"),
            < 80 => _translations.Get("reading--amount-large"),
            _ => value < 160
                ? _translations.Get("reading--amount-very-large")
                : _translations.Get("reading--amount-huge")
        };
    }

    private string TranslateDirection(Direction direction)
    {
        var stringBuilder = new StringBuilder();
        var vertical = false;
        var horizontal = false;

        // Up/Down
        {
            if (direction.HasFlag(Direction.Up))
            {
                stringBuilder.Append(_translations.Get("reading--up"));
                vertical = true;
            }

            if (direction.HasFlag(Direction.Down))
            {
                stringBuilder.Append(_translations.Get("reading--down"));
                vertical = true;
            }
        }

        // North/South
        {
            if (direction.HasFlag(Direction.North))
            {
                if (vertical)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--north"));
                horizontal = true;
            }

            if (direction.HasFlag(Direction.South))
            {
                if (vertical)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--south"));
                horizontal = true;
            }
        }

        // East/West
        {
            if (direction.HasFlag(Direction.East))
            {
                if (vertical && !horizontal)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--east"));
            }

            if (direction.HasFlag(Direction.West))
            {
                if (vertical && !horizontal)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--west"));
            }
        }

        return stringBuilder.ToString();
    }
}
