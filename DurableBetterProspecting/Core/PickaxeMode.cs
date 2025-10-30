namespace DurableBetterProspecting.Core;

internal record PickaxeMode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Icon Icon { get; init; }
    public required SampleShape SampleShape { get; init; }
    public required SampleType SampleType { get; init; }
    public required int SampleRadius { get; init; }
    public required int DurabilityCost { get; init; }
    public required bool Enabled { get; init; }
}
