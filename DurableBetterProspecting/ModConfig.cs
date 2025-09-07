// ReSharper disable MemberCanBePrivate.Global

using System;
using ConfigLib;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

public class ModConfig
{
    private const string FileName = "DurableBetterProspecting.json";
    private const string ConfigLibModId = "configlib";
    private static ConfigLibModSystem? _configSystem;

    public static ModConfig Loaded { get; } = new();

    #region Density Mode

    private int _densityModeDurabilityCost = 1;

    public bool DensityModeEnabled { get; set; } = true;

    public int DensityModeDurabilityCost
    {
        get => _densityModeDurabilityCost;
        set => _densityModeDurabilityCost = Math.Max(1, value);
    }

    #endregion Density Mode

    #region Rock Mode

    private int _rockModeDurabilityCost = 1;
    private int _rockModeSize = 32;

    public bool RockModeEnabled { get; set; } = true;

    public int RockModeDurabilityCost
    {
        get => _rockModeDurabilityCost;
        set => _rockModeDurabilityCost = Math.Max(1, value);
    }

    public int RockModeSize
    {
        get => _rockModeSize;
        set => _rockModeSize = Math.Max(2, value);
    }

    #endregion Rock Mode

    #region Distance Mode

    private int _distanceModeSmallDurabilityCost = 1;
    private int _distanceModeSmallSize = 16;
    private int _distanceModeMediumDurabilityCost = 2;
    private int _distanceModeMediumSize = 32;
    private int _distanceModeLargeDurabilityCost = 4;
    private int _distanceModeLargeSize = 64;

    public bool DistanceModeEnabled { get; set; } = true;

    public int DistanceModeSmallDurabilityCost
    {
        get => _distanceModeSmallDurabilityCost;
        set => _distanceModeSmallDurabilityCost = Math.Max(1, value);
    }

    public int DistanceModeSmallSize
    {
        get => _distanceModeSmallSize;
        set => _distanceModeSmallSize = Math.Max(2, value);
    }

    public int DistanceModeMediumDurabilityCost
    {
        get => _distanceModeMediumDurabilityCost;
        set => _distanceModeMediumDurabilityCost = Math.Max(1, value);
    }

    public int DistanceModeMediumSize
    {
        get => _distanceModeMediumSize;
        set => _distanceModeMediumSize = Math.Max(2, value);
    }

    public int DistanceModeLargeDurabilityCost
    {
        get => _distanceModeLargeDurabilityCost;
        set => _distanceModeLargeDurabilityCost = Math.Max(1, value);
    }

    public int DistanceModeLargeSize
    {
        get => _distanceModeLargeSize;
        set => _distanceModeLargeSize = Math.Max(2, value);
    }

    #endregion Distance Mode

    #region Area Mode

    private int _areaModeSmallDurabilityCost = 1;
    private int _areaModeSmallSize = 16;
    private int _areaModeMediumDurabilityCost = 2;
    private int _areaModeMediumSize = 32;
    private int _areaModeLargeDurabilityCost = 4;
    private int _areaModeLargeSize = 64;

    public bool AreaModeEnabled { get; set; } = true;

    public int AreaModeSmallDurabilityCost
    {
        get => _areaModeSmallDurabilityCost;
        set => _areaModeSmallDurabilityCost = Math.Max(1, value);
    }

    public int AreaModeSmallSize
    {
        get => _areaModeSmallSize;
        set => _areaModeSmallSize = Math.Max(2, value);
    }

    public int AreaModeMediumDurabilityCost
    {
        get => _areaModeMediumDurabilityCost;
        set => _areaModeMediumDurabilityCost = Math.Max(1, value);
    }

    public int AreaModeMediumSize
    {
        get => _areaModeMediumSize;
        set => _areaModeMediumSize = Math.Max(2, value);
    }

    public int AreaModeLargeDurabilityCost
    {
        get => _areaModeLargeDurabilityCost;
        set => _areaModeLargeDurabilityCost = Math.Max(1, value);
    }

    public int AreaModeLargeSize
    {
        get => _areaModeLargeSize;
        set => _areaModeLargeSize = Math.Max(2, value);
    }

    #endregion Area Mode

    public static void LoadAndSave(ICoreAPI api)
    {
        var config = api.LoadModConfig<ModConfig>(FileName);

        if (config != null)
        {
            // Density Mode
            Loaded.DensityModeEnabled = config.DensityModeEnabled;
            Loaded.DensityModeDurabilityCost = config.DensityModeDurabilityCost;

            // Rock Mode
            Loaded.RockModeEnabled = config.RockModeEnabled;
            Loaded.RockModeDurabilityCost = config.RockModeDurabilityCost;
            Loaded.RockModeSize = config.RockModeSize;

            // Distance Mode
            Loaded.DistanceModeEnabled = config.DistanceModeEnabled;
            Loaded.DistanceModeSmallDurabilityCost = config.DistanceModeSmallDurabilityCost;
            Loaded.DistanceModeSmallSize = config.DistanceModeSmallSize;
            Loaded.DistanceModeMediumDurabilityCost = config.DistanceModeMediumDurabilityCost;
            Loaded.DistanceModeMediumSize = config.DistanceModeMediumSize;
            Loaded.DistanceModeLargeDurabilityCost = config.DistanceModeLargeDurabilityCost;
            Loaded.DistanceModeLargeSize = config.DistanceModeLargeSize;

            // Area Mode
            Loaded.AreaModeEnabled = config.AreaModeEnabled;
            Loaded.AreaModeSmallDurabilityCost = config.AreaModeSmallDurabilityCost;
            Loaded.AreaModeSmallSize = config.AreaModeSmallSize;
            Loaded.AreaModeMediumDurabilityCost = config.AreaModeMediumDurabilityCost;
            Loaded.AreaModeMediumSize = config.AreaModeMediumSize;
            Loaded.AreaModeLargeDurabilityCost = config.AreaModeLargeDurabilityCost;
            Loaded.AreaModeLargeSize = config.AreaModeLargeSize;
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
