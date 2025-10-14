using ProtoBuf;
using Vintagestory.API.MathTools;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public record MarkingPacket
{
    [ProtoMember(1)]
    public required Vec3i Position { get; init; }

    [ProtoMember(2)]
    public required string Text { get; init; }

    public static MarkingPacket Create(Vec3i position, string text)
    {
        return new MarkingPacket
        {
            Position = position,
            Text = text
        };
    }
}
