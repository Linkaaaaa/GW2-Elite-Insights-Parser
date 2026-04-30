using System.Numerics;
using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class EmoteEvent : AnimatedCastEvent
{
    public readonly long EmoteID;
    public readonly EmoteGUIDEvent EmoteGUIDEvent = EmoteGUIDEvent.DummyEmoteGUID;

    internal EmoteEvent(CombatItem? startItem, AgentData agentData, SkillData skillData, CombatItem? endItem, 
        long maxEnd, EvtcVersionEvent evtcVersion, IReadOnlyDictionary<long, EmoteGUIDEvent> emoteGUIDict) : base(startItem, agentData, skillData, endItem, maxEnd, evtcVersion)
    {
        if (startItem != null)
        {
            EmoteID = evtcVersion.Build < ArcDPSBuilds.AnimationAsStateChanges ? startItem.Pad : startItem.OverstackValue;
            if (emoteGUIDict.TryGetValue(EmoteID, out var emoteGUIDEvent))
            {
                EmoteGUIDEvent = emoteGUIDEvent;
            }
        }
    }

}
