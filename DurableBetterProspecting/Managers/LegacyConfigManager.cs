using System.Diagnostics;
using System.Text.Json;
using Common.Mod.Common.Config;
using DurableBetterProspecting.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

internal class LegacyConfigManager
{
    private readonly EnumAppSide _side;
    private readonly ILogger _logger;
    private readonly IConfigSystem _configSystem;

    private readonly JsonSerializerOptions _jsonOptions;

    public LegacyConfigManager(EnumAppSide side, ILogger logger, IConfigSystem configSystem)
    {
        _side = side;
        _logger = logger.Named(nameof(LegacyConfigManager));
        _configSystem = configSystem;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public void Migrate()
    {
        var path = Path.Combine(GamePaths.DataPath, "ModConfig", "DurableBetterProspecting.json");
        if (!File.Exists(path))
        {
            return;
        }

        _logger.Info("Migrating legacy configuration to new format");
        var stopwatch = Stopwatch.StartNew();

        LegacyConfig? legacyConfig;
        try
        {
            var json = File.ReadAllText(path);
            legacyConfig = JsonSerializer.Deserialize<LegacyConfig>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to deserialize legacy configuration");
            return;
        }

        // Common
        if (_side is EnumAppSide.Server)
        {
            _configSystem.MutateCommon<DurableBetterProspectingCommonConfig>(commonConfig =>
            {
                commonConfig.DensityMode.Enabled = legacyConfig!.DensityModeEnabled;
                commonConfig.DensityMode.Simplified = legacyConfig.DensityModeSimplified;
                commonConfig.DensityMode.DurabilityCost = legacyConfig.DensityModeDurabilityCost;

                commonConfig.NodeMode.Enabled = legacyConfig.NodeModeEnabled;
                commonConfig.NodeMode.DurabilityCost = legacyConfig.NodeModeDurabilityCost;

                commonConfig.RockMode.Enabled = legacyConfig.RockModeEnabled;
                commonConfig.RockMode.DurabilityCost = legacyConfig.RockModeDurabilityCost;
                commonConfig.RockMode.SampleSize = legacyConfig.RockModeSize;

                commonConfig.DistanceMode.Enabled = legacyConfig.DistanceModeEnabled;
                commonConfig.DistanceMode.DurabilityCostShort = legacyConfig.DistanceModeSmallDurabilityCost;
                commonConfig.DistanceMode.SampleSizeShort = legacyConfig.DistanceModeSmallSize;
                commonConfig.DistanceMode.DurabilityCostMedium = legacyConfig.DistanceModeMediumDurabilityCost;
                commonConfig.DistanceMode.SampleSizeMedium = legacyConfig.DistanceModeMediumSize;
                commonConfig.DistanceMode.DurabilityCostLong = legacyConfig.DistanceModeLargeDurabilityCost;
                commonConfig.DistanceMode.SampleSizeLong = legacyConfig.DistanceModeLargeSize;

                commonConfig.QuantityMode.Enabled = legacyConfig.AreaModeEnabled;
                commonConfig.QuantityMode.DurabilityCostShort = legacyConfig.AreaModeSmallDurabilityCost;
                commonConfig.QuantityMode.SampleSizeShort = legacyConfig.AreaModeSmallSize;
                commonConfig.QuantityMode.DurabilityCostMedium = legacyConfig.AreaModeMediumDurabilityCost;
                commonConfig.QuantityMode.SampleSizeMedium = legacyConfig.AreaModeMediumSize;
                commonConfig.QuantityMode.DurabilityCostLong = legacyConfig.AreaModeLargeDurabilityCost;
                commonConfig.QuantityMode.SampleSizeLong = legacyConfig.AreaModeLargeSize;

                return commonConfig;
            });
        }

        // Client
        if (_side is EnumAppSide.Client)
        {
            _configSystem.MutateClient<DurableBetterProspectingClientConfig>(clientConfig =>
            {
                clientConfig.Ordering.Enabled = legacyConfig!.OrderReadings;
                clientConfig.Ordering.Direction =
                    legacyConfig.OrderReadingsDirection == "Ascending" ? OrderingDirection.Ascending : OrderingDirection.Descending;

                return clientConfig;
            });
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to delete legacy configuration file. You'll likely want to delete it manually");
            _logger.Error(ex, "Path: {0}", path);

            stopwatch.Stop();
            _logger.Info("Migrated legacy configuration with errors in {0} ms", stopwatch.ElapsedMilliseconds);

            return;
        }

        stopwatch.Stop();
        _logger.Info("Successfully migrated legacy configuration in {0} ms", stopwatch.ElapsedMilliseconds);
    }
}
