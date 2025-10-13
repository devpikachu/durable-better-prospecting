using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DurableBetterProspecting.Core;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public struct Reading
{
    [ProtoMember(1)]
    public required int Distance { get; set; }

    [ProtoMember(2)]
    public required int Quantity { get; set; }

    [ProtoMember(3)]
    public required string BlockId { get; init; }

    [ProtoMember(4)]
    public required string HandbookLink { get; init; }

    public static Reading Create(int distance, int quantity, Block block)
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

        return Create(distance, quantity, blockId, handbookLink);
    }

    private static Reading Create(int distance, int quantity, string blockId, string handbookLink)
    {
        return new Reading
        {
            Distance = distance,
            Quantity = quantity,
            BlockId = blockId,
            HandbookLink = handbookLink
        };
    }
}
