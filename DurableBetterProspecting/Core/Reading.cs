using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DurableBetterProspecting.Core;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class Reading
{
    [ProtoMember(1)]
    public required int Distance { get; set; }

    [ProtoMember(2)]
    public required int Quantity { get; set; }

    [ProtoMember(3)]
    public required Direction? Direction { get; set; }

    [ProtoMember(4)]
    public required string BlockId { get; init; }

    [ProtoMember(5)]
    public required string HandbookLink { get; init; }

    public static Reading Create(int distance, int quantity, Direction? direction, Block block)
    {
        var blockId = string.Empty;

        if (block.Variant.TryGetValue("rock", out var rockType))
        {
            blockId = $"rock-{rockType}";
        }

        if (block.BlockMaterial is EnumBlockMaterial.Ore && block.Variant.TryGetValue("type", out var oreType))
        {
            blockId = $"ore-{oreType}";
        }

        if (string.IsNullOrWhiteSpace(blockId))
        {
            throw new InvalidOperationException();
        }

        var handbookLink = $"handbook://{GuiHandbookItemStackPage.PageCodeForStack(new ItemStack(block))}";

        return Create(distance, quantity, direction, blockId, handbookLink);
    }

    private static Reading Create(int distance, int quantity, Direction? direction, string blockId, string handbookLink)
    {
        return new Reading
        {
            Distance = distance,
            Quantity = quantity,
            Direction = direction,
            BlockId = blockId,
            HandbookLink = handbookLink
        };
    }
}
