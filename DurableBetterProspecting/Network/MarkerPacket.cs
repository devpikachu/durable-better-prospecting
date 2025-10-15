using ProtoBuf;
using Vintagestory.API.MathTools;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public record MarkerPacket
{
    [ProtoMember(1)]
    public required Vec3i Position { get; init; }

    [ProtoMember(2)]
    public required string Text { get; init; }

    public static MarkerPacket Create(Vec3i position, string text)
    {
        return new MarkerPacket
        {
            Position = position,
            Text = text
        };
    }
}
