using GW2EIGW2API;
using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class ShardEvent : MetaDataEvent
{
    public readonly ulong ShardID;
    public readonly ulong UpperShardID;
    public readonly uint UserWorldID0;
    public readonly uint UserWorldID1;

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
        UpperShardID = evtcItem.DstAgent;
        UserWorldID0 = (uint)evtcItem.Value;
        UserWorldID1 = (uint)evtcItem.BuffDmg;
        if (UserWorldID0 > 0)
        {
            Region = GetRegion(UserWorldID0);
        } 
        else if (mapEvent != null)
        {
            var mapAPI = apiController.GetAPIMap(mapEvent.MapID);
            if (mapAPI != null && mapAPI.Type == "Instance")
            {
                Region = GetRegion(ShardID);
            }
        }
    }

}
