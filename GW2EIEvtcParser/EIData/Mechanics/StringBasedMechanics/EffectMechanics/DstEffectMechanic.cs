﻿using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser.EIData;


internal abstract class DstEffectMechanic : EffectMechanic
{

    protected override AgentItem GetAgentItem(EffectEvent effectEvt, AgentData agentData)
    {
        if (!effectEvt.IsAroundDst || effectEvt.Dst.IsUnamedSpecies())
        {
            return agentData.GetNPCsByID(ArcDPSEnums.TrashID.Environment).FirstOrDefault()!;
        }
        return effectEvt.Dst;
    }

    public DstEffectMechanic(GUID effectGUID, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : this([ effectGUID ], inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
    {
    }

    public DstEffectMechanic(ReadOnlySpan<GUID> effects, string inGameName, MechanicPlotlySetting plotlySetting, string shortName, string description, string fullName, int internalCoolDown) : base(effects, inGameName, plotlySetting, shortName, description, fullName, internalCoolDown)
    {
    }
}
