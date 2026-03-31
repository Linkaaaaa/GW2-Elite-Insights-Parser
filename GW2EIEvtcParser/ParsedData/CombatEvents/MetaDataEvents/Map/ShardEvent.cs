using GW2EIGW2API;
using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class ShardEvent : MetaDataEvent
{
    public readonly ulong ShardID;
    public readonly ushort UpperShardID;

    public readonly RegionEnum Region = RegionEnum.Unknown;


    private static RegionEnum GetRegion(ulong shardID)
    {
        // TODO: china
        if (shardID < 2000 && shardID > 1000)
        {
            return RegionEnum.US;
        } 
        else if (shardID > 2000 && shardID < 3000)
        {
            return RegionEnum.EU;
        }
        return RegionEnum.Unknown;
    }
    internal ShardEvent(CombatItem evtcItem, MapIDEvent? mapEvent, GW2APIController apiController) : base(evtcItem)
    {
        ShardID = evtcItem.SrcAgent;
        UpperShardID = (ushort)(evtcItem.DstAgent & 0x000000FF);
        if (mapEvent != null)
        {
            var mapAPI = apiController.GetAPIMap(mapEvent.MapID);
            if (mapAPI != null && mapAPI.Type == "Instance")
            {
                Region = GetRegion(ShardID);
            }
        }
    }

}
