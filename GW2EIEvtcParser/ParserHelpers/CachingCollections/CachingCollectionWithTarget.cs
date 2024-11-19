﻿using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIEvtcParser;

public class CachingCollectionWithTarget<T>(ParsedEvtcLog log)
    : CachingCollectionCustom<SingleActor, T>(log, _nullActor, log.FightData.Logic.Hostiles.Count)
{
    private static readonly NPC _nullActor = new(new AgentItem());
}
