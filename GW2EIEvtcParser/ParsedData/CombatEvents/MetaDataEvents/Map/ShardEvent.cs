using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class ShardEvent : MetaDataEvent
{
    public readonly ulong ShardID;
    public readonly ushort UpperShardID;

    public readonly RegionEnum Region;


    private static RegionEnum GetRegion(ulong shardID)
    {
        // TODO: china
        if (shardID < 2000)
        {
            return RegionEnum.US;
        }
        return RegionEnum.EU;
    }
    internal ShardEvent(CombatItem evtcItem) : base(evtcItem)
    {
        ShardID = evtcItem.SrcAgent;
        UpperShardID = (ushort)(evtcItem.DstAgent & 0x000000FF);
        Region = GetRegion(ShardID);
    }

}
