using ProtoBuf;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class ConfigPacket
{
    #region General

    public bool OrderReadings;
    public string OrderReadingsDirection = ModConfig.OrderAscending;

    #endregion General

    #region Density Mode

    public bool DensityModeEnabled;
    public bool DensityModeSimplified;
    public int DensityModeDurabilityCost;

    #endregion Density Mode

    #region Node Mode

    public bool NodeModeEnabled;
    public int NodeModeDurabilityCost;

    #endregion Node Mode

    #region Rock Mode

    public bool RockModeEnabled;
    public int RockModeDurabilityCost;
    public int RockModeSize;

    #endregion Rock Mode

    #region Distance Mode

    public bool DistanceModeEnabled;
    public int DistanceModeSmallDurabilityCost;
    public int DistanceModeSmallSize;
    public int DistanceModeMediumDurabilityCost;
    public int DistanceModeMediumSize;
    public int DistanceModeLargeDurabilityCost;
    public int DistanceModeLargeSize;

    #endregion Distance Mode

    #region Area Mode

    public bool AreaModeEnabled;
    public int AreaModeSmallDurabilityCost;
    public int AreaModeSmallSize;
    public int AreaModeMediumDurabilityCost;
    public int AreaModeMediumSize;
    public int AreaModeLargeDurabilityCost;
    public int AreaModeLargeSize;

    #endregion Area Mode

    public static ConfigPacket FromConfig(ModConfig config)
    {
        return new ConfigPacket
        {
            // General
            OrderReadings = config.OrderReadings,
            OrderReadingsDirection = config.OrderReadingsDirection,

            // Density Mode
            DensityModeEnabled = config.DensityModeEnabled,
            DensityModeSimplified = config.DensityModeSimplified,
            DensityModeDurabilityCost = config.DensityModeDurabilityCost,

            // Node Mode
            NodeModeEnabled = config.NodeModeEnabled,
            NodeModeDurabilityCost = config.NodeModeDurabilityCost,

            // Rock Mode
            RockModeEnabled = config.RockModeEnabled,
            RockModeDurabilityCost = config.RockModeDurabilityCost,
            RockModeSize = config.RockModeSize,

            // Distance Mode
            DistanceModeEnabled = config.DistanceModeEnabled,
            DistanceModeSmallDurabilityCost = config.DistanceModeSmallDurabilityCost,
            DistanceModeSmallSize = config.DistanceModeSmallSize,
            DistanceModeMediumDurabilityCost = config.DistanceModeMediumDurabilityCost,
            DistanceModeMediumSize = config.DistanceModeMediumSize,
            DistanceModeLargeDurabilityCost = config.DistanceModeLargeDurabilityCost,
            DistanceModeLargeSize = config.DistanceModeLargeSize,

            // Area Mode
            AreaModeEnabled = config.AreaModeEnabled,
            AreaModeSmallDurabilityCost = config.AreaModeSmallDurabilityCost,
            AreaModeSmallSize = config.AreaModeSmallSize,
            AreaModeMediumDurabilityCost = config.AreaModeMediumDurabilityCost,
            AreaModeMediumSize = config.AreaModeMediumSize,
            AreaModeLargeDurabilityCost = config.AreaModeLargeDurabilityCost,
            AreaModeLargeSize = config.AreaModeLargeSize
        };
    }
}
