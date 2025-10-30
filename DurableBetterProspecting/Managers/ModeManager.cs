using System.Diagnostics;
using Common.Mod.Common.Config;
using Common.Mod.Common.Core;
using DurableBetterProspecting.Core;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

/// <summary>
/// Manages prospecting pickaxe modes.
/// <br/><br/>
/// <b>Side:</b> Universal
/// </summary>
internal class ModeManager
{
    private readonly ICoreAPI _api;
    private readonly ILogger _logger;
    private readonly ITranslations _translations;
    private readonly IConfigSystem _configSystem;

    private PickaxeMode DensityMode { get; set; }
    private PickaxeMode NodeMode { get; set; }
    private PickaxeMode RockMode { get; set; }
    private PickaxeMode ColumnMode { get; set; }
    private PickaxeMode DistanceShortMode { get; set; }
    private PickaxeMode DistanceMediumMode { get; set; }
    private PickaxeMode DistanceLongMode { get; set; }
    private PickaxeMode QuantityShortMode { get; set; }
    private PickaxeMode QuantityMediumMode { get; set; }
    private PickaxeMode QuantityLongMode { get; set; }

    private bool _assetsLoaded;
    private DurableBetterProspectingCommonConfig _commonConfig;
    private PickaxeMode[] _modes = [];
    private SkillItem[] _skillItems = [];

#pragma warning disable CS8618
    public ModeManager(ICoreAPI api, ISystem system, ILogger logger, ITranslations translations, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger.Named(nameof(ModeManager));
        _translations = translations;
        _configSystem = configSystem;

        system.OnAssetsLoaded += () =>
        {
            _assetsLoaded = true;
            CreateModes();
        };

        _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
        _configSystem.Updated += type =>
        {
            if (type is not RootConfigType.Common)
            {
                return;
            }

            _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
            CreateModes();
        };
    }
#pragma warning restore CS8618

    public SkillItem[] GetSkillItems() => _skillItems;

    public PickaxeMode? GetMode(int skillIndex)
    {
        if (_skillItems.Length == 0)
        {
            return null;
        }

        var skill = _skillItems[skillIndex];
        return _modes.FirstOrDefault(mode => mode.Id == skill.Code.Path);
    }

    private void CreateModes()
    {
        if (!_assetsLoaded)
        {
            _logger.Debug("Assets not yet loaded, skipping modes creation");
            return;
        }

        _logger.Verbose("(Re)Creating prospecting pickaxe modes");
        var stopwatch = Stopwatch.StartNew();

        // Dispose of existing skill items, if any
        if (_skillItems.Length > 0)
        {
            foreach (var skillItem in _skillItems)
            {
                skillItem.Dispose();
            }
        }

        // Create modes
        DensityMode = new PickaxeMode
        {
            Id = Constants.DensityModeId,
            Name = _translations.Get("mode--density"),
            Icon = Icon.Create("game", "heatmap"),
            SampleShape = SampleShape.Vanilla,
            SampleType = SampleType.Vanilla,
            SampleSize = int.MaxValue,
            DurabilityCost = _commonConfig.DensityMode.DurabilityCost,
            Enabled = _commonConfig.DensityMode.Enabled
        };

        NodeMode = new PickaxeMode
        {
            Id = Constants.NodeModeId,
            Name = _translations.Get("mode--node"),
            Icon = Icon.Create("game", "rocks"),
            SampleShape = SampleShape.Vanilla,
            SampleType = SampleType.Vanilla,
            SampleSize = int.MaxValue,
            DurabilityCost = _commonConfig.NodeMode.DurabilityCost,
            Enabled = _commonConfig.NodeMode.Enabled && _api.World.Config.GetString(Constants.NodeSearchRadiusConfigKey).ToInt() > 0
        };

        RockMode = new PickaxeMode
        {
            Id = Constants.RockModeId,
            Name = _translations.Get("mode--rock"),
            Icon = Icon.Create("mode_rock"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Rock,
            SampleSize = _commonConfig.RockMode.SampleSize,
            DurabilityCost = _commonConfig.RockMode.DurabilityCost,
            Enabled = _commonConfig.RockMode.Enabled
        };

        ColumnMode = new PickaxeMode
        {
            Id = Constants.ColumnModeId,
            Name = _translations.Get("mode--column"),
            Icon = Icon.Create("mode_column"),
            SampleShape = SampleShape.Cuboid,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.ColumnMode.SampleSize,
            DurabilityCost = _commonConfig.ColumnMode.DurabilityCost,
            Enabled = _commonConfig.ColumnMode.Enabled
        };

        DistanceShortMode = new PickaxeMode
        {
            Id = Constants.DistanceShortModeId,
            Name = _translations.Get("mode--distance-short"),
            Icon = Icon.Create("mode_distance_short"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.DistanceMode.SampleSizeShort,
            DurabilityCost = _commonConfig.DistanceMode.DurabilityCostShort,
            Enabled = _commonConfig.DistanceMode.EnabledShort
        };

        DistanceMediumMode = new PickaxeMode
        {
            Id = Constants.DistanceMediumModeId,
            Name = _translations.Get("mode--distance-medium"),
            Icon = Icon.Create("mode_distance_medium"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.DistanceMode.SampleSizeMedium,
            DurabilityCost = _commonConfig.DistanceMode.DurabilityCostMedium,
            Enabled = _commonConfig.DistanceMode.EnabledMedium
        };

        DistanceLongMode = new PickaxeMode
        {
            Id = Constants.DistanceLongModeId,
            Name = _translations.Get("mode--distance-long"),
            Icon = Icon.Create("mode_distance_long"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.DistanceMode.SampleSizeLong,
            DurabilityCost = _commonConfig.DistanceMode.DurabilityCostLong,
            Enabled = _commonConfig.DistanceMode.EnabledLong
        };

        QuantityShortMode = new PickaxeMode
        {
            Id = Constants.QuantityShortModeId,
            Name = _translations.Get("mode--quantity-short"),
            Icon = Icon.Create("mode_quantity_short"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.QuantityMode.SampleSizeShort,
            DurabilityCost = _commonConfig.QuantityMode.DurabilityCostShort,
            Enabled = _commonConfig.QuantityMode.EnabledShort
        };

        QuantityMediumMode = new PickaxeMode
        {
            Id = Constants.QuantityMediumModeId,
            Name = _translations.Get("mode--quantity-medium"),
            Icon = Icon.Create("mode_quantity_medium"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.QuantityMode.SampleSizeMedium,
            DurabilityCost = _commonConfig.QuantityMode.DurabilityCostMedium,
            Enabled = _commonConfig.QuantityMode.EnabledMedium
        };

        QuantityLongMode = new PickaxeMode
        {
            Id = Constants.QuantityLongModeId,
            Name = _translations.Get("mode--quantity-long"),
            Icon = Icon.Create("mode_quantity_long"),
            SampleShape = SampleShape.Cube,
            SampleType = SampleType.Ore,
            SampleSize = _commonConfig.QuantityMode.SampleSizeLong,
            DurabilityCost = _commonConfig.QuantityMode.DurabilityCostLong,
            Enabled = _commonConfig.QuantityMode.EnabledLong
        };

        _modes =
        [
            DensityMode,
            NodeMode,
            RockMode,
            ColumnMode,
            DistanceShortMode,
            DistanceMediumMode,
            DistanceLongMode,
            QuantityShortMode,
            QuantityMediumMode,
            QuantityLongMode
        ];

        // Create skill items
        ObjectCacheUtil.Delete(_api, Constants.SkillItemsCacheKey);
        _skillItems = ObjectCacheUtil.GetOrCreate(_api, Constants.SkillItemsCacheKey, () =>
        {
            return _modes
                .Where(mode => mode.Enabled)
                .Select(mode =>
                {
                    var skillItem = new SkillItem
                    {
                        Code = mode.Id,
                        Name = mode.Name
                    };

                    if (_api is not ICoreClientAPI clientApi)
                    {
                        return skillItem;
                    }

                    skillItem.WithIcon(clientApi, mode.Icon.Load(clientApi));
                    skillItem.TexturePremultipliedAlpha = false;

                    return skillItem;
                })
                .ToArray();
        });

        stopwatch.Stop();
        _logger.Verbose($"Done in {stopwatch.ElapsedMilliseconds} ms");
    }
}
