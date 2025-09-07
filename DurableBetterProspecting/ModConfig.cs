using ConfigLib;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

public class ModConfig
{
    private const string FileName = "DurableBetterProspecting.json";
    private const string ConfigLibModId = "configlib";
    private static ConfigLibModSystem? _configSystem;

    public static ModConfig Loaded { get; private set; } = new();

    public bool DensityModeEnabled { get; init; } = true;
    public int DensityModeDurabilityCost { get; init; } = 1;

    public bool RockModeEnabled { get; init; } = true;
    public int RockModeDurabilityCost { get; init; } = 1;
    public int RockModeSize { get; init; } = 128;

    public bool DistanceModeEnabled { get; init; } = true;
    public int DistanceModeSmallDurabilityCost { get; init; } = 1;
    public int DistanceModeSmallSize { get; init; } = 32;
    public int DistanceModeMediumDurabilityCost { get; init; } = 2;
    public int DistanceModeMediumSize { get; init; } = 64;
    public int DistanceModeLargeDurabilityCost { get; init; } = 2;
    public int DistanceModeLargeSize { get; init; } = 256;

    public bool AreaModeEnabled { get; init; } = true;
    public int AreaModeSmallDurabilityCost { get; init; } = 1;
    public int AreaModeSmallSize { get; init; } = 16;
    public int AreaModeMediumDurabilityCost { get; init; } = 2;
    public int AreaModeMediumSize { get; init; } = 32;
    public int AreaModeLargeDurabilityCost { get; init; } = 3;
    public int AreaModeLargeSize { get; init; } = 64;

    public static void LoadOrSaveDefault(ICoreAPI api)
    {
        var config = api.LoadModConfig<ModConfig>(FileName);
        if (config != null)
        {
            Loaded = config;
            return;
        }

        api.StoreModConfig(Loaded, FileName);
    }

    public static void RegisterListeners(ICoreAPI api)
    {
        if (!api.ModLoader.IsModEnabled(ConfigLibModId))
        {
            return;
        }

        _configSystem = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        _configSystem.SettingChanged += OnConfigChanged;
    }

    public static void UnregisterListeners()
    {
        if (_configSystem == null)
        {
            return;
        }

        _configSystem.SettingChanged -= OnConfigChanged;
    }

    private static void OnConfigChanged(string domain, IConfig config, ISetting setting)
    {
        if (domain != ModSystem.ModId)
        {
            return;
        }

        setting.AssignSettingValue(Loaded);
    }
}
