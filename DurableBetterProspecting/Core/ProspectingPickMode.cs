namespace DurableBetterProspecting.Core;

public record ProspectingPickMode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Icon Icon { get; init; }
}
