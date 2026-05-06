using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using GW2EIJSON;

namespace GW2EIBuilders.JsonModels;

internal static class JsonWvWMapDataBuilder
{
    public static JsonWvWMapData BuildJsonWvWMapData(ParsedEvtcLog log, WvWTeamsEvent wvwTeamsEvent,  HashSet<ulong> teampMap)
    {
        var jsonWvWMapData = new JsonWvWMapData
        {
            BlueShardID = wvwTeamsEvent.BlueShardID,
            RedShardID = wvwTeamsEvent.RedShardID,
            GreenShardID = wvwTeamsEvent.GreenShardID,

            BlueTeamID = wvwTeamsEvent.BlueTeamID,
            GreenTeamID = wvwTeamsEvent.GreenTeamID,
            RedTeamID = wvwTeamsEvent.RedTeamID,
        };

        teampMap.UnionWith([wvwTeamsEvent.BlueTeamID, wvwTeamsEvent.GreenTeamID, wvwTeamsEvent.RedTeamID]);

        return jsonWvWMapData;
    }

}
