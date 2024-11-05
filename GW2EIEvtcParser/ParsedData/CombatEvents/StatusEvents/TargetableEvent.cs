﻿namespace GW2EIEvtcParser.ParsedData;

public class TargetableEvent : AbstractStatusEvent
{
    public readonly bool Targetable;

    internal TargetableEvent(CombatItem evtcItem, AgentData agentData) : base(evtcItem, agentData)
    {
        Targetable = evtcItem.DstAgent == 1;
    }

}
