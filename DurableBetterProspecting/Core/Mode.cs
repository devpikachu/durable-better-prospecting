namespace DurableBetterProspecting.Core;

public record Mode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Icon Icon { get; init; }
    public required Func<bool> Enabled { get; init; }

    public static Mode Create(string id, string name, Icon icon, Func<bool> enabled)
    {
        return new Mode
        {
            Id = id,
            Name = name,
            Icon = icon,
            Enabled = enabled
        };
    }
}
