namespace DurableBetterProspecting.Core;

[Flags]
public enum Direction
{
    None = 0,
    Up = 1,
    Down = 2,
    North = 4,
    South = 8,
    East = 16,
    West = 32
}
