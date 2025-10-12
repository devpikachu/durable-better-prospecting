using Vintagestory.API.Common;

namespace DurableBetterProspecting.Core;

public record Reading
{
    public required int? Distance { get; init; }
    public required int? Quantity { get; init; }
    public required Block Block { get; init; }

    public static Reading CreateDistance(int distance, Block block)
    {
        return new Reading
        {
            Distance = distance,
            Quantity = null,
            Block = block
        };
    }

    public static Reading CreateQuantity(int quantity, Block block)
    {
        return new Reading
        {
            Distance = null,
            Quantity = quantity,
            Block = block
        };
    }
}
