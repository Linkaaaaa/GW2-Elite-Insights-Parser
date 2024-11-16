﻿using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EIData.DamageModifiersUtils;
using static GW2EIEvtcParser.ParserHelper;

namespace GW2EIEvtcParser.EIData;

internal class DamageLogDamageModifier : DamageModifierDescriptor
{

    internal DamageLogDamageModifier(string name, string tooltip, DamageSource damageSource, double gainPerStack, DamageType srctype, DamageType compareType, Source src, string icon, DamageLogChecker checker, DamageModifierMode mode) : base(name, tooltip, damageSource, gainPerStack, srctype, compareType, src, icon, ByPresence, mode)
    {
        base.UsingChecker(checker);
    }

    protected override bool ComputeGain(IReadOnlyDictionary<long, BuffsGraphModel>? bgms, HealthDamageEvent? dl, ParsedEvtcLog log, out double gain)
    {
        gain = GainComputer.ComputeGain(GainPerStack, 1);
        return true;
    }

    internal override List<DamageModifierEvent> ComputeDamageModifier(SingleActor actor, ParsedEvtcLog log, DamageModifier damageModifier)
    {
        var res = new List<DamageModifierEvent>();
        if (ComputeGain(null, null, log, out double gain))
        {

            var typeHits = damageModifier.GetHitDamageEvents(actor, log, null, log.FightData.FightStart, log.FightData.FightEnd);
            foreach (HealthDamageEvent evt in typeHits)
            {
                if (CheckCondition(evt, log))
                {
                    res.Add(new DamageModifierEvent(evt, damageModifier, gain * evt.HealthDamage));
                }
            }
        }
        return res;
    }
}
