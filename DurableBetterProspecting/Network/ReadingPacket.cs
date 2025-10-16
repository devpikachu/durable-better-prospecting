using DurableBetterProspecting.Core;
using ProtoBuf;
using Vintagestory.API.MathTools;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
internal record ReadingPacket
{
    [ProtoMember(1)]
    public required string Mode { get; init; }

    [ProtoMember(2)]
    public required int Size { get; init; }

    [ProtoMember(3)]
    public required Vec3i? Position { get; init; }

    [ProtoMember(4)]
    public required Reading[]? Readings { get; init; }
}
