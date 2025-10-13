using System.Diagnostics;
using Common.Mod.Common.Config;
using DurableBetterProspecting.Core;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using ILogger = Common.Mod.Common.Core.ILogger;

namespace DurableBetterProspecting.Managers;

public class ModeManager
{
    public const string NodeModeSampleSizeKey = "propickNodeSearchRadius";

    private const string SkillItemsCacheKey = "proPickToolModes";

    private readonly ICoreAPI _api;
    private readonly ILogger _logger;
    private readonly IConfigSystem _configSystem;

    public Mode DensityMode { get; private set; }
    public Mode NodeMode { get; private set; }
    public Mode RockMode { get; private set; }
    public Mode ColumnMode { get; private set; }
    public Mode DistanceShortMode { get; private set; }
    public Mode DistanceMediumMode { get; private set; }
    public Mode DistanceLongMode { get; private set; }
    public Mode QuantityShortMode { get; private set; }
    public Mode QuantityMediumMode { get; private set; }
    public Mode QuantityLongMode { get; private set; }

    private Mode[] _modes = [];
    private SkillItem[] _skillItems = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ModeManager(ICoreAPI api, ILogger logger, IConfigSystem configSystem)
    {
        _api = api;
        _logger = logger;
        _configSystem = configSystem;

        _configSystem.Updated += CreateModes;

        CreateModes();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public SkillItem[] GetSkillItems() => _skillItems;

    public Mode GetMode(int skillIndex)
    {
        var skill = _skillItems[skillIndex];
        return _modes.First(mode => mode.Id == skill.Code.Path);
    }

    private void CreateModes(RootConfigType type = RootConfigType.Common)
    {
        if (type is not RootConfigType.Common)
        {
            return;
        }

        _logger.Verbose("(Re)Creating prospecting pickaxe modes");
        var stopwatch = Stopwatch.StartNew();

        var commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();

        // TODO: Icons

        DensityMode = Mode.Create(
            id: "density",
            name: "Density Mode (Long range, chance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        NodeMode = Mode.Create(
            id: "node",
            name: "Node Mode (Short range, quantity based)",
            icon: Icon.Create("game", "rocks"),
            enabled: commonConfig.NodeMode.Enabled && _api.World.Config.GetString(NodeModeSampleSizeKey).ToInt() > 0
        );

        RockMode = Mode.Create(
            id: "rock",
            name: "Rock Mode (Medium range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        ColumnMode = Mode.Create(
            id: "column",
            name: "Column Mode (Long range, presence based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        DistanceShortMode = Mode.Create(
            id: "distance_short",
            name: "Distance Mode (Short range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        DistanceMediumMode = Mode.Create(
            id: "distance_medium",
            name: "Distance Mode (Medium range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        DistanceLongMode = Mode.Create(
            id: "distance_long",
            name: "Distance Mode (Long range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        QuantityShortMode = Mode.Create(
            id: "quantity_short",
            name: "Quantity Mode (Short range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        QuantityMediumMode = Mode.Create(
            id: "quantity_medium",
            name: "Quantity Mode (Medium range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

        QuantityLongMode = Mode.Create(
            id: "quantity_long",
            name: "Quantity Mode (Long range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: commonConfig.DensityMode.Enabled
        );

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

        ObjectCacheUtil.Delete(_api, SkillItemsCacheKey);
        _skillItems = ObjectCacheUtil.GetOrCreate(_api, SkillItemsCacheKey, () =>
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
