using DurableBetterProspecting.Core;
using ProtoBuf;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public record ReadingPacket
{
    [ProtoMember(1)]
    public required SampleMode Mode { get; init; }

    [ProtoMember(2)]
    public required int SampleSize { get; init; }

    [ProtoMember(3)]
    public required Reading[]? Readings { get; init; }
}
