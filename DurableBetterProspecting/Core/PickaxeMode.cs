namespace DurableBetterProspecting.Core;

internal record PickaxeMode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Icon Icon { get; init; }
    public required SampleShape SampleShape { get; init; }
    public required SampleType SampleType { get; init; }
    public required int SampleSize { get; init; }
    public required int DurabilityCost { get; init; }
    public required bool Enabled { get; init; }

    public virtual bool Equals(PickaxeMode? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || string.Equals(Id, other.Id, StringComparison.InvariantCulture);
    }

    public override int GetHashCode()
    {
        return StringComparer.InvariantCulture.GetHashCode(Id);
    }
}
