﻿using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EIData.DamageModifiersUtils;
using static GW2EIEvtcParser.ParserHelper;

namespace GW2EIEvtcParser.EIData;

internal class BuffOnActorDamageModifier : DamageModifierDescriptor
{
    internal delegate double DamageGainAdjuster(HealthDamageEvent dl, ParsedEvtcLog log);

    internal BuffsTracker Tracker;
    internal DamageGainAdjuster? GainAdjuster;

    internal BuffOnActorDamageModifier(long buffID, string name, string tooltip, DamageSource damageSource, double gainPerStack, DamageType srctype, DamageType compareType, Source src, GainComputer gainComputer, string icon, DamageModifierMode mode) : base(0, name, tooltip, damageSource, gainPerStack, srctype, compareType, src, icon, gainComputer, mode)
    {
        Tracker = new BuffsTrackerSingle(buffID);
    }

    internal BuffOnActorDamageModifier(HashSet<long> buffIDs, string name, string tooltip, DamageSource damageSource, double gainPerStack, DamageType srctype, DamageType compareType, Source src, GainComputer gainComputer, string icon, DamageModifierMode mode) : base(0, name, tooltip, damageSource, gainPerStack, srctype, compareType, src, icon, gainComputer, mode)
    {
        Tracker = new BuffsTrackerMulti(buffIDs);
    }

    internal virtual DamageModifierDescriptor UsingGainAdjuster(DamageGainAdjuster gainAdjuster)
    {
        GainAdjuster = gainAdjuster;
        return this;
    }

    private double ComputeAdjustedGain(HealthDamageEvent dl, ParsedEvtcLog log)
    {
        return GainAdjuster != null ? GainAdjuster(dl, log) * GainPerStack : GainPerStack;
    }

    protected override bool ComputeGain(IReadOnlyDictionary<long, BuffsGraphModel> bgms, HealthDamageEvent dl, ParsedEvtcLog log, out double gain)
    {
        int stack = Tracker.GetStack(bgms, dl.Time);
        gain = GainComputer.ComputeGain(ComputeAdjustedGain(dl, log), stack);
        return gain != 0;
    }

    protected static bool Skip(BuffsTracker tracker, IReadOnlyDictionary<long, BuffsGraphModel> bgms, GainComputer gainComputer)
    {
        return (!tracker.Has(bgms) && gainComputer != ByAbsence) || (tracker.Has(bgms) && gainComputer == ByAbsence);
    }

    internal override List<DamageModifierEvent> ComputeDamageModifier(SingleActor actor, ParsedEvtcLog log, DamageModifier damageModifier)
    {
        IReadOnlyDictionary<long, BuffsGraphModel> bgms = actor.GetBuffGraphs(log);
        if (Skip(Tracker, bgms, GainComputer))
        {
            return [];
        }
        var res = new List<DamageModifierEvent>();
        var typeHits = damageModifier.GetHitDamageEvents(actor, log, null, log.FightData.FightStart, log.FightData.FightEnd);
        foreach (HealthDamageEvent evt in typeHits)
        {
            if (ComputeGain(bgms, evt, log, out double gain) && CheckCondition(evt, log))
            {
                res.Add(new DamageModifierEvent(evt, damageModifier, gain * evt.HealthDamage));
            }
        }
        return res;
    }
}
