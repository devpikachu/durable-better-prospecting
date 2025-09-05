namespace DurableBetterProspecting;

public class DurableBetterProspectingConfig
{
    public int DensityModeDurabilityCost { get; init; } = 1;

    public int DistanceModeDurabilityCost { get; init; } = 1;
    public float DistanceModeDurabilityCostMultiplier { get; init; } = 2.0f;
    public int DistanceModeSmallSize { get; init; } = 32;
    public int DistanceModeLargeSize { get; init; } = 256;

    public int RockModeDurabilityCost { get; init; } = 1;
    public int RockModeSize { get; init; } = 128;

    public int AreaModeDurabilityCost { get; init; } = 1;
    public float AreaModeDurabilityCostMultiplier { get; init; } = 1.5f;
    public int AreaModeSmallSize { get; init; } = 16;
    public int AreaModeMediumSize { get; init; } = 32;
    public int AreaModeLargeSize { get; init; } = 64;
}