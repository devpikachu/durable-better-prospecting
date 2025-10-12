namespace DurableBetterProspecting.Core;

public record Mode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Icon Icon { get; init; }
    public required bool Enabled { get; init; }

    public static Mode Create(string id, string name, Icon icon, bool enabled)
    {
        return new Mode
        {
            Id = id,
            Name = name,
            Icon = icon,
            Enabled = enabled
        };
    }

    public virtual bool Equals(Mode? other)
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
