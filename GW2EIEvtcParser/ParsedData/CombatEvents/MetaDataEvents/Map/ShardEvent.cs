namespace GW2EIEvtcParser.ParsedData;

public class ShardEvent : MetaDataEvent
{
    public readonly ulong ShardID;
    public readonly ushort UpperShardID;

    internal ShardEvent(CombatItem evtcItem) : base(evtcItem)
    {
        ShardID = evtcItem.SrcAgent;
        UpperShardID = (ushort)(evtcItem.DstAgent & 0x000000FF);
    }

}
