namespace DurableBetterProspecting.Core;

[Flags]
internal enum ReadingDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    North = 4,
    South = 8,
    East = 16,
    West = 32
}
