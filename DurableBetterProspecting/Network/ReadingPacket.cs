using DurableBetterProspecting.Core;
using ProtoBuf;

namespace DurableBetterProspecting.Network;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public record ReadingPacket
{
    [ProtoMember(1)]
    public required Reading[] Readings { get; init; }
}
