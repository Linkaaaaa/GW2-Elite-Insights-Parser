﻿using GW2EIBuilders.HtmlModels.HTMLCharts;
using GW2EIBuilders.HtmlModels.HTMLStats;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using static GW2EIEvtcParser.ParserHelper;

namespace GW2EIBuilders.HtmlModels;

using GameplayStatDataItem = List<double>;
/*(
    double timeWasted, // 0
    int wasted, // 1
    double timeSaved, // 2
    int saved, // 3
    int swap, // 4
    double distToStack, // 5
    double distToCom, // 6
    double castUptime, // 7
    double castUptimeNoAA // 8
);
*/

using OffensiveStatDataItem = List<double>;
/*(
    int directDamageCount, // 0
    int critableDirectDamageCount,
    int criticalCount,
    int criticalDamage,
    int flanking,
    int glance, // 5
    int missed,
    int interrupts,
    int invulned,
    int evaded,
    int blocked, // 10
    int connectedDirectDamageCount,
    int killed,
    int downed,
    int againstMoving,
    int connectedDamageCount, // 15
    int totalDamageCount,
    int downContribution,
    int connectedDamage,
    int connectedDirectDamage,
    int connectedPowerCount, // 20
    int connectedPowerAbove90HPCount,
    int connectedConditionCount,
    int connectedConditionAbove90HPCount,
    int againstDownedCount,
    int againstDownedDamage, // 25
    int totalDamage,
    int appliedCrowdControl,
    double appliedCrowdControlDuration
);*/

using DPSStatDataItem = List<double>;
/*(
    int damage, // 0
    int powerDamage,
    int conditionDamage,
    double breakbarDamage
);*/

using DefensiveStatDataItem = object[];
/*(
    int damageTaken, // 0
    int damageBarrier,
    int missedCount,
    int interruptCount,
    int invulnedCount,
    int evadedCound, // 5
    int blockedCount,
    int dodgeCount,
    int conditionCleanses,
    double conditionCleansesTime,
    int boonStrips, // 10,
    double boonStripsTime,
    int downCount,
    string downTooltip,
    int deadCount,
    string deadTooltip, // 15
    int downedDamageTaken,
    int receivedCrowdControl,
    double receivedCrowdControlDuration
);*/

using SupportStatDataItem = List<double>;
/*(
    int conditionCleanse, // 0
    double conditionCleanseTime,
    int conditionCleanseSelf,
    double conditionCleanseSelfTime,
    int boonStrips,
    double boonStripsTime, // 5
    int ressurects,
    double ressurectsTime,
    int stunBreak,
    double removedStunDuration
);*/

//TODO(Rennorb) @perf: IF we wanted more performance we could try to just get rid of this json data step all together.
// It should be doable to just merge it with existing structures, as to not need to copy everything..
// If this is reasonably possible it should give time savings around 20-30%
internal class PhaseDto
{
    public string? Name;
    public long Duration;
    public double Start;
    public double End;
    public List<int> Targets;
    public List<bool> SecondaryTargets;
    public bool BreakbarPhase;

    public List<DPSStatDataItem> DpsStats;
    public List<List<DPSStatDataItem>> DpsStatsTargets;
    public List<List<OffensiveStatDataItem>> OffensiveStatsTargets;
    public List<OffensiveStatDataItem> OffensiveStats;
    public List<GameplayStatDataItem> GameplayStats;
    public List<DefensiveStatDataItem> DefStats;
    public List<SupportStatDataItem> SupportStats;

    public BuffsContainerDto BuffsStatContainer;
    public BuffVolumesContainerDto BuffVolumesStatContainer;

    public List<DamageModData> DmgModifiersCommon;
    public List<DamageModData> DmgModifiersItem;
    public List<DamageModData> DmgModifiersPers;


    public List<DamageModData> DmgIncModifiersCommon;
    public List<DamageModData> DmgIncModifiersItem;
    public List<DamageModData> DmgIncModifiersPers;


    public List<List<(int, int)>> MechanicStats;
    public List<List<(int, int)>> EnemyMechanicStats;
    public List<long> PlayerActiveTimes;

    public List<double>? MarkupLines;
    public List<AreaLabelDto>? MarkupAreas;
    public List<int>? SubPhases;

    public PhaseDto(PhaseData phase, IReadOnlyList<PhaseData> phases, ParsedEvtcLog log, IReadOnlyDictionary<Spec, IReadOnlyList<Buff>> persBuffDict,
        IReadOnlyList<OutgoingDamageModifier> commonOutDamageModifiers, IReadOnlyList<OutgoingDamageModifier> itemOutDamageModifiers, IReadOnlyDictionary<Spec, IReadOnlyList<OutgoingDamageModifier>> persOutDamageModDict,
        IReadOnlyList<IncomingDamageModifier> commonIncDamageModifiers, IReadOnlyList<IncomingDamageModifier> itemIncDamageModifiers, IReadOnlyDictionary<Spec, IReadOnlyList<IncomingDamageModifier>> persIncDamageModDict)
    {
        Name          = phase.Name;
        Duration      = phase.DurationInMS;
        Start         = phase.Start / 1000.0;
        End           = phase.End / 1000.0;
        BreakbarPhase = phase.BreakbarPhase;

        var allTargets = phase.AllTargets;
        Targets          = new(allTargets.Count);
        SecondaryTargets = new(allTargets.Count);
        foreach (SingleActor target in allTargets)
        {
            Targets.Add(log.FightData.Logic.Targets.IndexOf(target));
            SecondaryTargets.Add(phase.IsSecondaryTarget(target));
        }

        PlayerActiveTimes = new(log.Friendlies.Count);
        foreach (SingleActor actor in log.Friendlies)
        {
            PlayerActiveTimes.Add(actor.GetActiveDuration(log, phase.Start, phase.End));
        }

        // add phase markup
        
        if (!BreakbarPhase)
        {
            MarkupLines = new(phases.Count);
            MarkupAreas = new(phases.Count);
            for (int j = 1; j < phases.Count; j++)
            {
                PhaseData curPhase = phases[j];
                if (curPhase.Start < phase.Start || curPhase.End > phase.End ||
                    (curPhase.Start == phase.Start && curPhase.End == phase.End) || !curPhase.CanBeSubPhase)
                {
                    continue;
                }

                SubPhases ??= new List<int>(phases.Count);
                SubPhases.Add(j);

                long start = curPhase.Start - phase.Start;
                long end = curPhase.End - phase.Start;
                if (curPhase.DrawStart)
                {
                    MarkupLines.Add(start / 1000.0);
                }

                if (curPhase.DrawEnd)
                {
                    MarkupLines.Add(end / 1000.0);
                }

                var phaseArea = new AreaLabelDto
                {
                    Start = start / 1000.0,
                    End = end / 1000.0,
                    Label = curPhase.DrawLabel ? curPhase.Name : null,
                    Highlight = curPhase.DrawArea
                };

                MarkupAreas.Add(phaseArea);
            }
        }

        if (MarkupAreas?.Count == 0)
        {
            MarkupAreas = null;
        }

        if (MarkupLines?.Count == 0)
        {
            MarkupLines = null;
        }

        BuffsStatContainer       = new BuffsContainerDto(phase, log, persBuffDict);
        BuffVolumesStatContainer = new BuffVolumesContainerDto(phase, log, persBuffDict);
        
        DpsStats              = BuildDPSData(log, phase);
        DpsStatsTargets       = BuildDPSTargetsData(log, phase);
        OffensiveStatsTargets = BuildOffensiveStatsTargetsData(log, phase);
        OffensiveStats        = BuildOffensiveStatsData(log, phase);
        GameplayStats         = BuildGameplayStatsData(log, phase);
        DefStats              = BuildDefenseData(log, phase);
        SupportStats          = BuildSupportData(log, phase);
        
        DmgModifiersCommon    = DamageModData.BuildOutgoingDmgModifiersData(log, phase, commonOutDamageModifiers);
        DmgModifiersItem      = DamageModData.BuildOutgoingDmgModifiersData(log, phase, itemOutDamageModifiers);
        DmgModifiersPers      = DamageModData.BuildPersonalOutgoingDmgModifiersData(log, phase, persOutDamageModDict);
        DmgIncModifiersCommon = DamageModData.BuildIncomingDmgModifiersData(log, phase, commonIncDamageModifiers);
        DmgIncModifiersItem   = DamageModData.BuildIncomingDmgModifiersData(log, phase, itemIncDamageModifiers);
        DmgIncModifiersPers   = DamageModData.BuildPersonalIncomingDmgModifiersData(log, phase, persIncDamageModDict);
        MechanicStats         = MechanicDto.BuildPlayerMechanicData(log, phase);
        EnemyMechanicStats    = MechanicDto.BuildEnemyMechanicData(log, phase);
    }

    // helper methods

    private static GameplayStatDataItem GetGameplayStatData(FinalGameplayStats stats)
    {
        return [
                stats.TimeWasted,
                stats.Wasted,
                stats.TimeSaved,
                stats.Saved,
                stats.SwapCount,
                Math.Round(stats.StackDist, 2),
                Math.Round(stats.DistToCom, 2),
                stats.SkillCastUptime,
                stats.SkillCastUptimeNoAA
        ];
    }

    private static OffensiveStatDataItem GetOffensiveStatData(FinalOffensiveStats stats)
    {
        return [
                stats.DirectDamageCount,
                stats.CritableDirectDamageCount,
                stats.CriticalCount,
                stats.CriticalDmg,
                stats.FlankingCount,
                stats.GlanceCount,
                stats.Missed,
                stats.Interrupts,
                stats.Invulned,
                stats.Evaded,
                stats.Blocked,
                stats.ConnectedDirectDamageCount,
                stats.Killed,
                stats.Downed,
                stats.AgainstMovingCount,
                stats.ConnectedDamageCount,
                stats.TotalDamageCount,
                stats.DownContribution,
                stats.ConnectedDmg,
                stats.ConnectedDirectDmg,
                stats.ConnectedPowerCount,
                stats.ConnectedPowerAbove90HPCount,
                stats.ConnectedConditionCount,
                stats.ConnectedConditionAbove90HPCount,
                stats.AgainstDownedCount,
                stats.AgainstDownedDamage,
                stats.TotalDmg,
                stats.AppliedCrowdControl,
                stats.AppliedCrowdControlDuration
            ];
    }

    private static DPSStatDataItem GetDPSStatData(FinalDPS dpsAll)
    {
        return [
                dpsAll.Damage,
                dpsAll.PowerDamage,
                dpsAll.CondiDamage,
                dpsAll.BreakbarDamage
            ];
    }

    private static SupportStatDataItem GetSupportStatData(FinalToPlayersSupport support)
    {
        return [
                support.CondiCleanse,
                support.CondiCleanseTime,
                support.CondiCleanseSelf,
                support.CondiCleanseTimeSelf,
                support.BoonStrips,
                support.BoonStripsTime,
                support.Resurrects,
                support.ResurrectTime,
                support.StunBreak,
                support.RemovedStunDuration
        ];
    }

    private static DefensiveStatDataItem GetDefenseStatData(FinalDefensesAll defenses, PhaseData phase)
    {
        int downCount = 0;
        string downTooltip = "0% Downed";
        if (defenses.DownCount > 0)
        {
            var downDuration = TimeSpan.FromMilliseconds(defenses.DownDuration);
            downCount = (defenses.DownCount);
            downTooltip = (downDuration.TotalSeconds + " seconds downed, " + Math.Round((downDuration.TotalMilliseconds / phase.DurationInMS) * 100, 1) + "% Downed");
        }
        int deadCount = 0;
        string deadTooltip = "100% Alive";
        if (defenses.DeadCount > 0)
        {
            var deathDuration = TimeSpan.FromMilliseconds(defenses.DeadDuration);
            deadCount = (defenses.DeadCount);
            deadTooltip = (deathDuration.TotalSeconds + " seconds dead, " + (100.0 - Math.Round((deathDuration.TotalMilliseconds / phase.DurationInMS) * 100, 1)) + "% Alive");
        }
        return [
                defenses.DamageTaken, 
                defenses.DamageBarrier,
                defenses.MissedCount,
                defenses.InterruptedCount,
                defenses.InvulnedCount,
                defenses.EvadedCount,
                defenses.BlockedCount,
                defenses.DodgeCount,
                defenses.ConditionCleanses,
                defenses.ConditionCleansesTime,
                defenses.BoonStrips,
                defenses.BoonStripsTime,
                downCount,
                downTooltip,
                deadCount,
                deadTooltip,
                defenses.DownedDamageTaken, 
                defenses.ReceivedCrowdControl,
                defenses.ReceivedCrowdControlDuration
            ];
    }
    public static List<DPSStatDataItem> BuildDPSData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<DPSStatDataItem>(log.Friendlies.Count);
        foreach (SingleActor actor in log.Friendlies)
        {
            FinalDPS dpsAll = actor.GetDPSStats(log, phase.Start, phase.End);
            list.Add(GetDPSStatData(dpsAll));
        }
        return list;
    }

    public static List<List<DPSStatDataItem>> BuildDPSTargetsData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<List<DPSStatDataItem>>(log.Friendlies.Count);

        foreach (SingleActor actor in log.Friendlies)
        {
            var playerData = new List<DPSStatDataItem>(phase.AllTargets.Count);

            foreach (SingleActor target in phase.AllTargets)
            {
                playerData.Add(GetDPSStatData(actor.GetDPSStats(target, log, phase.Start, phase.End)));
            }
            list.Add(playerData);
        }
        return list;
    }

    public static List<GameplayStatDataItem> BuildGameplayStatsData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<GameplayStatDataItem>(log.Friendlies.Count);
        foreach (SingleActor actor in log.Friendlies)
        {
            FinalGameplayStats stats = actor.GetGameplayStats(log, phase.Start, phase.End);
            list.Add(GetGameplayStatData(stats));
        }
        return list;
    }

    public static List<OffensiveStatDataItem> BuildOffensiveStatsData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<OffensiveStatDataItem>(log.Friendlies.Count);
        foreach (SingleActor actor in log.Friendlies)
        {
            FinalOffensiveStats stats = actor.GetOffensiveStats(null, log, phase.Start, phase.End);
            list.Add(GetOffensiveStatData(stats));
        }
        return list;
    }

    public static List<List<OffensiveStatDataItem>> BuildOffensiveStatsTargetsData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<List<OffensiveStatDataItem>>(log.Friendlies.Count);

        foreach (SingleActor actor in log.Friendlies)
        {
            var playerData = new List<OffensiveStatDataItem>(phase.AllTargets.Count);
            foreach (SingleActor target in phase.AllTargets)
            {
                FinalOffensiveStats statsTarget = actor.GetOffensiveStats(target, log, phase.Start, phase.End);
                playerData.Add(GetOffensiveStatData(statsTarget));
            }
            list.Add(playerData);
        }
        return list;
    }

    public static List<DefensiveStatDataItem> BuildDefenseData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<DefensiveStatDataItem>(log.Friendlies.Count);

        foreach (SingleActor actor in log.Friendlies)
        {
            FinalDefensesAll defenses = actor.GetDefenseStats(log, phase.Start, phase.End);
            list.Add(GetDefenseStatData(defenses, phase));
        }

        return list;
    }

    public static List<SupportStatDataItem> BuildSupportData(ParsedEvtcLog log, PhaseData phase)
    {
        var list = new List<SupportStatDataItem>(log.Friendlies.Count);

        foreach (SingleActor actor in log.Friendlies)
        {
            FinalToPlayersSupport support = actor.GetToPlayerSupportStats(log, phase.Start, phase.End);
            list.Add(GetSupportStatData(support));
        }
        return list;
    }
}
