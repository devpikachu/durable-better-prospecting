namespace DurableBetterProspecting.Config;

internal class LegacyConfig
{
    public required bool OrderReadings { get; init; }
    public required string OrderReadingsDirection { get; init; }
    public required bool DensityModeEnabled { get; init; }
    public required bool DensityModeSimplified { get; init; }
    public required int DensityModeDurabilityCost { get; init; }
    public required bool NodeModeEnabled { get; init; }
    public required int NodeModeDurabilityCost { get; init; }
    public required bool RockModeEnabled { get; init; }
    public required int RockModeDurabilityCost { get; init; }
    public required int RockModeSize { get; init; }
    public required bool DistanceModeEnabled { get; init; }
    public required int DistanceModeSmallDurabilityCost { get; init; }
    public required int DistanceModeSmallSize { get; init; }
    public required int DistanceModeMediumDurabilityCost { get; init; }
    public required int DistanceModeMediumSize { get; init; }
    public required int DistanceModeLargeDurabilityCost { get; init; }
    public required int DistanceModeLargeSize { get; init; }
    public required bool AreaModeEnabled { get; init; }
    public required int AreaModeSmallDurabilityCost { get; init; }
    public required int AreaModeSmallSize { get; init; }
    public required int AreaModeMediumDurabilityCost { get; init; }
    public required int AreaModeMediumSize { get; init; }
    public required int AreaModeLargeDurabilityCost { get; init; }
    public required int AreaModeLargeSize { get; init; }
}
