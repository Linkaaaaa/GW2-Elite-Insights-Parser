﻿using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EIData.Buff;

namespace GW2EIEvtcParser.EIData;

public class FinalDefenses
{
    //public long allHealReceived;
    public readonly int DamageTaken;
    public readonly int ConditionDamageTaken;
    public readonly int PowerDamageTaken;
    public readonly int LifeLeechDamageTaken;
    public readonly int StrikeDamageTaken;
    public readonly int DownedDamageTaken;
    public readonly double BreakbarDamageTaken;
    public readonly int BlockedCount;
    public readonly int MissedCount;
    public readonly int EvadedCount;
    public readonly int DodgeCount;
    public readonly int InvulnedCount;
    public readonly int DamageBarrier;
    public readonly int InterruptedCount;
    public readonly int BoonStrips;
    public readonly double BoonStripsTime;
    public readonly int ConditionCleanses;
    public readonly double ConditionCleansesTime;
    public readonly int ReceivedCrowdControl;
    public readonly double ReceivedCrowdControlDuration;

    private static (int, double) GetStripData(IReadOnlyList<Buff> buffs, ParsedEvtcLog log, long start, long end, SingleActor actor, SingleActor? from, bool excludeSelf)
    {
        double stripTime = 0;
        int strip = 0;
        foreach (Buff buff in buffs)
        {
            double currentBoonStripTime = 0;
            IReadOnlyList<BuffRemoveAllEvent> removeAllArray = log.CombatData.GetBuffRemoveAllData(buff.ID);
            foreach (BuffRemoveAllEvent brae in removeAllArray)
            {
                if (brae.Time >= start && brae.Time <= end && brae.To == actor.AgentItem)
                {
                    if (from != null && brae.CreditedBy != from.AgentItem || brae.CreditedBy == ParserHelper._unknownAgent || (excludeSelf && brae.CreditedBy == actor.AgentItem))
                    {
                        continue;
                    }
                    currentBoonStripTime = Math.Max(currentBoonStripTime + brae.RemovedDuration, log.FightData.FightDuration);
                    strip++;
                }
            }
            stripTime += currentBoonStripTime;
        }
        stripTime = Math.Round(stripTime / 1000.0, ParserHelper.TimeDigit);
        return (strip, stripTime);
    }

    internal FinalDefenses(ParsedEvtcLog log, long start, long end, SingleActor actor, SingleActor? from)
    {
        var damageLogs = actor.GetDamageTakenEvents(from, log, start, end);
        foreach (HealthDamageEvent damageEvent in damageLogs)
        {
            DamageTaken += damageEvent.HealthDamage;
            if (damageEvent is NonDirectHealthDamageEvent ndhd)
            {
                if (damageEvent.ConditionDamageBased(log))
                {
                    ConditionDamageTaken += damageEvent.HealthDamage;
                }
                else
                {
                    PowerDamageTaken += damageEvent.HealthDamage;
                    if (ndhd.IsLifeLeech)
                    {
                        LifeLeechDamageTaken += damageEvent.HealthDamage;
                    }
                }
            }
            else
            {
                StrikeDamageTaken += damageEvent.HealthDamage;
                PowerDamageTaken += damageEvent.HealthDamage;
            }
            DamageBarrier += damageEvent.ShieldDamage;
            if (damageEvent.IsBlocked)
            {
                BlockedCount++;
            }
            if (damageEvent.IsBlind)
            {
                MissedCount++;
            }
            if (damageEvent.IsAbsorbed)
            {
                InvulnedCount++;
            }
            if (damageEvent.IsEvaded)
            {
                EvadedCount++;
            }
            if (damageEvent.HasInterrupted)
            {
                InterruptedCount++;
            }
            if (damageEvent.AgainstDowned)
            {
                DownedDamageTaken += damageEvent.HealthDamage;
            }
        }
        var ccs = actor.GetIncomingCrowdControlEvents(from, log, start, end);
        foreach (CrowdControlEvent cc in ccs)
        {
            ReceivedCrowdControl++;
            ReceivedCrowdControlDuration += cc.Duration;
        }
        DodgeCount = actor.GetCastEvents(log, start, end).Count(x => x.Skill.IsDodge(log.SkillData));
        BreakbarDamageTaken = Math.Round(actor.GetBreakbarDamageTakenEvents(from, log, start, end).Sum(x => x.BreakbarDamage), 1);
        (BoonStrips, BoonStripsTime) = GetStripData(log.Buffs.BuffsByClassification[BuffClassification.Boon], log, start, end, actor, from, true);
        (ConditionCleanses, ConditionCleansesTime) = GetStripData(log.Buffs.BuffsByClassification[BuffClassification.Condition], log, start, end, actor, from, false);
    }
}
