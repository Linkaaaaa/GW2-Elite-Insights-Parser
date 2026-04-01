using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class ShardEvent : MetaDataEvent
{
    public readonly ulong ShardID;
    public readonly ulong UpperShardID;
    public readonly uint UserWorldID0;
    public readonly uint UserWorldID1;

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
        UpperShardID = evtcItem.DstAgent;
        UserWorldID0 = (uint)evtcItem.Value;
        UserWorldID1 = (uint)evtcItem.BuffDmg;
        Region = GetRegion(ShardID);
    }

}
