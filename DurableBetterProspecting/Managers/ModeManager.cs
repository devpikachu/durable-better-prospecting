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

    public PickaxeMode DensityMode { get; private set; }
    public PickaxeMode NodeMode { get; private set; }
    public PickaxeMode RockMode { get; private set; }
    public PickaxeMode ColumnMode { get; private set; }
    public PickaxeMode DistanceShortMode { get; private set; }
    public PickaxeMode DistanceMediumMode { get; private set; }
    public PickaxeMode DistanceLongMode { get; private set; }
    public PickaxeMode QuantityShortMode { get; private set; }
    public PickaxeMode QuantityMediumMode { get; private set; }
    public PickaxeMode QuantityLongMode { get; private set; }

    private DurableBetterProspectingCommonConfig _commonConfig;
    private PickaxeMode[] _modes = [];
    private SkillItem[] _skillItems = [];

#pragma warning disable CS8618
    public ModeManager(ICoreAPI api, ILogger logger, ITranslations translations, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger.Named(nameof(ModeManager));
        _translations = translations;
        _configSystem = configSystem;

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

        CreateModes();
    }
#pragma warning restore CS8618

    public SkillItem[] GetSkillItems() => _skillItems;

    public PickaxeMode GetMode(int skillIndex)
    {
        var skill = _skillItems[skillIndex];
        return _modes.First(mode => mode.Id == skill.Code.Path);
    }

    private void CreateModes()
    {
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
            Enabled = _commonConfig.DensityMode.Enabled
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
