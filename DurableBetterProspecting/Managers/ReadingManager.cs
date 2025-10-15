using System.Diagnostics;
using System.Text;
using Common.Mod.Common.Config;
using Common.Mod.Common.Core;
using Common.Mod.Exceptions;
using DurableBetterProspecting.Core;
using DurableBetterProspecting.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

/// <summary>
/// Manages sorting and printing readings.
/// <br/><br/>
/// <b>Side:</b> Client
/// </summary>
internal class ReadingManager
{
    private readonly ICoreClientAPI _api;
    private readonly ILogger _logger;
    private readonly IClientNetworkChannel _channel;
    private readonly ITranslations _translations;
    private readonly IConfigSystem _configSystem;

    private DurableBetterProspectingClientConfig _clientConfig;

    public ReadingManager(
        ICoreAPI api,
        EnumAppSide side,
        ILogger logger,
        IClientNetworkChannel channel,
        ITranslations translations,
        IConfigSystem configSystem
    )
    {
        if (side is not EnumAppSide.Client)
        {
            throw new InvalidSideException(side);
        }

        _api = (api as ICoreClientAPI)!;
        _logger = logger.Named(nameof(ReadingManager));
        _channel = channel;
        _translations = translations;
        _configSystem = configSystem;

        _channel.SetMessageHandler<ReadingPacket>(ProcessReading);

        _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
        _configSystem.Updated += type =>
        {
            if (type is RootConfigType.Client)
            {
                _clientConfig = _configSystem.GetClient<DurableBetterProspectingClientConfig>();
            }
        };
    }

    private void ProcessReading(ReadingPacket packet)
    {
        _logger.Verbose("Received packet with {0} readings", packet.Readings.Length);
        var stopwatch = Stopwatch.StartNew();

        var messageBuilder = new StringBuilder();
        var markerBuilder = new StringBuilder();

        // Message: X sample taken in a Y blocks area
        {
            var modeString = packet.Mode switch
            {
                Constants.RockModeId => _translations.Get("reading--mode-rock"),
                Constants.ColumnModeId => _translations.Get("reading--mode-column"),
                Constants.DistanceShortModeId or Constants.DistanceMediumModeId or Constants.DistanceLongModeId => _translations.Get("reading--mode-distance"),
                Constants.QuantityShortModeId or Constants.QuantityMediumModeId or Constants.QuantityLongModeId => _translations.Get("reading--mode-quantity"),
                _ => throw new ArgumentOutOfRangeException()
            };

            var sampleTakenString = _translations.Get("reading--sample-taken", modeString, packet.Size);
            messageBuilder.AppendLine(sampleTakenString);
        }

        var rocksString = _translations.Get("reading--rocks");
        var oresString = _translations.Get("reading--ores");

        // Message: No rocks/ores found
        {
            if (packet.Readings.Length == 0)
            {
                var sampleEmptyString = _translations.Get("reading--sample-empty", packet.Mode is Constants.RockModeId ? rocksString : oresString);

                messageBuilder.AppendLine(sampleEmptyString);
                _api.ShowChatMessage(messageBuilder.ToString());

                stopwatch.Stop();
                _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

                return;
            }
        }

        // Sort readings
        var readings = packet.Readings.AsEnumerable();
        if (_clientConfig.Ordering.Enabled)
        {
            var ascending = _clientConfig.Ordering.Direction is OrderingDirection.Ascending;

            if (packet.Mode is Constants.RockModeId or Constants.DistanceShortModeId or Constants.DistanceMediumModeId or Constants.DistanceLongModeId)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.Distance)
                    : readings.OrderByDescending(r => r.Distance);
            }

            if (packet.Mode is Constants.ColumnModeId)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.BlockId)
                    : readings.OrderByDescending(r => r.BlockId);
            }

            if (packet.Mode is Constants.QuantityShortModeId or Constants.QuantityMediumModeId or Constants.QuantityLongModeId)
            {
                readings = ascending
                    ? readings.OrderBy(r => r.Quantity)
                    : readings.OrderByDescending(r => r.Quantity);
            }
        }

        // Message: Found the following rocks/ores
        {
            var sampleNotEmptyString = _translations.Get("reading--sample-not-empty", packet.Mode is Constants.RockModeId ? rocksString : oresString);
            messageBuilder.AppendLine(sampleNotEmptyString);
        }

        // Add each reading to the message / marker
        {
            var blocksAwayString = _translations.Get("reading--blocks-away");
            foreach (var reading in readings)
            {
                var nameString = Lang.GetL(Lang.CurrentLocale, reading.BlockId);

                switch (packet.Mode)
                {
                    case Constants.RockModeId:
                    {
                        markerBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2} {3}", reading.HandbookLink, nameString, reading.Distance, blocksAwayString);

                        if (reading.Direction is not null && reading.Direction is not ReadingDirection.None && _clientConfig.Direction)
                        {
                            markerBuilder.AppendFormat(" - {0}", TranslateDirection((ReadingDirection)reading.Direction));
                        }

                        markerBuilder.Append('\n');
                        break;
                    }

                    case Constants.ColumnModeId:
                        markerBuilder.AppendFormat("<a href=\"{0}\">{1}</a>\n", reading.HandbookLink, nameString);
                        break;

                    case Constants.DistanceShortModeId:
                    case Constants.DistanceMediumModeId:
                    case Constants.DistanceLongModeId:
                    {
                        markerBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2} {3}", reading.HandbookLink, nameString, reading.Distance, blocksAwayString);

                        if (reading.Direction is not null && reading.Direction is not ReadingDirection.None && _clientConfig.Direction)
                        {
                            markerBuilder.AppendFormat(" - {0}", TranslateDirection((ReadingDirection)reading.Direction));
                        }

                        markerBuilder.Append('\n');
                        break;
                    }

                    case Constants.QuantityShortModeId:
                    case Constants.QuantityMediumModeId:
                    case Constants.QuantityLongModeId:
                    {
                        markerBuilder.AppendFormat("<a href=\"{0}\">{1}</a>: {2}", reading.HandbookLink, nameString, TranslateQuantity(reading.Quantity));

                        if (reading.Direction is not null && reading.Direction is not ReadingDirection.None && _clientConfig.Direction)
                        {
                            markerBuilder.AppendFormat(" - {0}", TranslateDirection((ReadingDirection)reading.Direction));
                        }

                        markerBuilder.Append('\n');
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        var markerString = markerBuilder.ToString();
        messageBuilder.Append(markerString);

        // Send marker packet
        {
            if (packet.Position is not null && _clientConfig.Marker)
            {
                var markerPacket = MarkerPacket.Create(packet.Position, $"{Constants.MarkerPrefix}\n{markerString}");
                _channel.SendPacket(markerPacket);
            }
        }

        stopwatch.Stop();
        _logger.Verbose("Done processing packet in {0} ms", stopwatch.ElapsedMilliseconds);

        _api.ShowChatMessage(messageBuilder.ToString());
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

    private string TranslateDirection(ReadingDirection readingDirection)
    {
        var stringBuilder = new StringBuilder();
        var vertical = false;
        var horizontal = false;

        // Up/Down
        {
            if (readingDirection.HasFlag(ReadingDirection.Up))
            {
                stringBuilder.Append(_translations.Get("reading--up"));
                vertical = true;
            }

            if (readingDirection.HasFlag(ReadingDirection.Down))
            {
                stringBuilder.Append(_translations.Get("reading--down"));
                vertical = true;
            }
        }

        // North/South
        {
            if (readingDirection.HasFlag(ReadingDirection.North))
            {
                if (vertical)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--north"));
                horizontal = true;
            }

            if (readingDirection.HasFlag(ReadingDirection.South))
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
            if (readingDirection.HasFlag(ReadingDirection.East))
            {
                if (vertical && !horizontal)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(_translations.Get("reading--east"));
            }

            if (readingDirection.HasFlag(ReadingDirection.West))
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
