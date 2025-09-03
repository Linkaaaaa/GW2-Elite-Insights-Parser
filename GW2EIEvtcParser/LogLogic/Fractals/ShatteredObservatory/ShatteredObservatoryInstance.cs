﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Schema;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.LogLogic.LogLogicPhaseUtils;
using static GW2EIEvtcParser.LogLogic.LogLogicTimeUtils;
using static GW2EIEvtcParser.LogLogic.LogLogicUtils;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.ParserHelpers.LogImages;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIEvtcParser.LogLogic;

internal class ShatteredObservatoryInstance : ShatteredObservatory
{
    private readonly Skorvald _skorvald;
    private readonly Artsariiv _artsariiv;
    private readonly Arkk _arkk;

    private readonly IReadOnlyList<ShatteredObservatory> _subLogics;
    public ShatteredObservatoryInstance(int triggerID) : base(triggerID)
    {
        LogID = LogIDs.LogMasks.Unsupported;
        Icon = InstanceIconShatteredObservatory;
        Extension = "shtrdobs";

        _skorvald = new Skorvald((int)TargetID.Skorvald);
        _artsariiv = new Artsariiv((int)TargetID.Artsariiv);
        _arkk = new Arkk((int)TargetID.Arkk);
        _subLogics = [_skorvald, _artsariiv, _arkk];

        MechanicList.Add(_skorvald.Mechanics);
        MechanicList.Add(_artsariiv.Mechanics);
        MechanicList.Add(_arkk.Mechanics);
    }

    internal override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log, CombatReplayDecorationContainer arenaDecorations)
    {
        var crMap = new CombatReplayMap((800, 800), (-24576, -24576, 24576, 24576));
        arenaDecorations.Add(new ArenaDecoration((log.LogData.LogStart, log.LogData.LogEnd), CombatReplayShatteredObservatory, crMap));
        foreach (var subLogic in _subLogics)
        {
            subLogic.GetCombatMapInternal(log, arenaDecorations);
        }
        return crMap;
    }
    internal override string GetLogicName(CombatData combatData, AgentData agentData)
    {
        return "Shattered Observatory";
    }
    internal override void CheckSuccess(CombatData combatData, AgentData agentData, LogData logData, IReadOnlyCollection<AgentItem> playerAgents)
    {
        var chest = agentData.GetGadgetsByID(_arkk.ChestID).FirstOrDefault();
        if (chest != null)
        {
            logData.SetSuccess(true, chest.FirstAware);
            return;
        }
        base.CheckSuccess(combatData, agentData, logData, playerAgents);
    }

    private List<EncounterPhaseData> HandleSkorvaldPhases(IReadOnlyDictionary<int, List<SingleActor>> targetsByIDs, ParsedEvtcLog log, List<PhaseData> phases)
    {
        var encounterPhases = new List<EncounterPhaseData>();
        var mainPhase = phases[0];
        if (targetsByIDs.TryGetValue((int)TargetID.Skorvald, out var skorvalds))
        {
            var anomalies = Targets.Where(x => x.IsAnySpecies(Skorvald.FluxAnomalies));
            var cmAnomalies = anomalies.Where(x => x.IsAnySpecies([TargetID.FluxAnomalyCM1, TargetID.FluxAnomalyCM2, TargetID.FluxAnomalyCM3, TargetID.FluxAnomalyCM4]));
            foreach (var skorvald in skorvalds)
            {
                var firstNonZeroHPUpdate = log.CombatData.GetHealthUpdateEvents(skorvald.AgentItem).FirstOrDefault(x => x.HealthPercent > 0);
                if (firstNonZeroHPUpdate == null)
                {
                    continue;
                }
                var enterCombat = log.CombatData.GetEnterCombatEvents(skorvald.AgentItem).FirstOrDefault();
                long start = Math.Min(enterCombat != null ? enterCombat.Time : long.MaxValue, firstNonZeroHPUpdate.Time);
                bool success = false;
                long end = skorvald.LastAware;
                var death = log.CombatData.GetDeadEvents(skorvald.AgentItem).FirstOrDefault();
                if (death != null)
                {
                    success = true;
                    end = death.Time;
                } 
                else
                {
                    var lastDamageTaken = skorvald.GetDamageTakenEvents(null, log).LastOrDefault(x => x.CreditedFrom.IsPlayer);
                    if (lastDamageTaken != null)
                    {
                        var invul895Apply = log.CombatData.GetBuffApplyDataByIDByDst(SkillIDs.Determined895, skorvald.AgentItem).Where(x => x.Time > lastDamageTaken.Time - 500).LastOrDefault();
                        if (invul895Apply != null)
                        {
                            end = invul895Apply.Time;
                            success = true;
                        }
                    }
                }
                var isCM = cmAnomalies.Any(x => skorvald.InAwareTimes(x.FirstAware));
                AddInstanceEncounterPhase(log, phases, encounterPhases, [skorvald], anomalies, [], mainPhase, "Skorvald", start, end, success, _skorvald, isCM ? LogData.LogMode.CM : LogData.LogMode.Normal);
            }
        }
        NumericallyRenameEncounterPhases(encounterPhases);
        return encounterPhases;
    }


    internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
    {
        List<PhaseData> phases = GetInitialPhase(log);
        var targetsByIDs = Targets.GroupBy(x => x.ID).ToDictionary(x => x.Key, x => x.ToList());
        {
            var skorvaldPhases = HandleSkorvaldPhases(targetsByIDs, log, phases);
            foreach (var skorvaldPhase in skorvaldPhases)
            {
                var skorvald = skorvaldPhase.Targets.Keys.First(x => x.IsSpecies(TargetID.Skorvald));
                phases.AddRange(Skorvald.ComputePhases(log, skorvald, Targets, skorvaldPhase, requirePhases));
            }
        }
        return phases;
    }

    internal override List<InstantCastFinder> GetInstantCastFinders()
    {
        List<InstantCastFinder> finders = [
            .. _skorvald.GetInstantCastFinders(),
            .. _artsariiv.GetInstantCastFinders(),
            .. _arkk.GetInstantCastFinders()
        ];
        return finders;
    }

    internal override IReadOnlyList<TargetID> GetTrashMobsIDs()
    {
        List<TargetID> trashes = [
            .. _skorvald.GetTrashMobsIDs(),
            .. _artsariiv.GetTrashMobsIDs(),
            .. _arkk.GetTrashMobsIDs()
        ];
        return trashes.Distinct().ToList();
    }
    internal override IReadOnlyList<TargetID> GetTargetsIDs()
    {
        List<TargetID> targets = [
            .. _skorvald.GetTargetsIDs(),
            .. _artsariiv.GetTargetsIDs(),
            .. _arkk.GetTargetsIDs()
        ];
        return targets.Distinct().ToList();
    }

    internal override IReadOnlyList<TargetID> GetFriendlyNPCIDs()
    {
        List<TargetID> friendlies = [
            .. _skorvald.GetFriendlyNPCIDs(),
            .. _artsariiv.GetFriendlyNPCIDs(),
            .. _arkk.GetFriendlyNPCIDs()
        ];
        return friendlies.Distinct().ToList();
    }
    protected override HashSet<int> IgnoreForAutoNumericalRenaming()
    {
        return [
            ..Skorvald.FluxAnomalies.Select(x => (int)x)
        ];
    }

    internal override void EIEvtcParse(ulong gw2Build, EvtcVersionEvent evtcVersion, LogData logData, AgentData agentData, List<CombatItem> combatData, IReadOnlyDictionary<uint, ExtensionHandler> extensions)
    {
        Skorvald.DetectUnknownAnomalies(agentData, combatData);
        base.EIEvtcParse(gw2Build, evtcVersion, logData, agentData, combatData, extensions);
        Skorvald.RenameAnomalies(Targets);
    }

    internal override List<BuffEvent> SpecialBuffEventProcess(CombatData combatData, SkillData skillData)
    {
        var res = new List<BuffEvent>();
        foreach (var subLogic in _subLogics)
        {
            res.AddRange(subLogic.SpecialBuffEventProcess(combatData, skillData));
        }
        return res;
    }

    internal override List<CastEvent> SpecialCastEventProcess(CombatData combatData, SkillData skillData)
    {
        var res = new List<CastEvent>();
        foreach (var subLogic in _subLogics)
        {
            res.AddRange(subLogic.SpecialCastEventProcess(combatData, skillData));
        }
        return res;
    }

    internal override List<HealthDamageEvent> SpecialDamageEventProcess(CombatData combatData, AgentData agentData, SkillData skillData)
    {
        var res = new List<HealthDamageEvent>();
        foreach (var subLogic in _subLogics)
        {
            res.AddRange(subLogic.SpecialDamageEventProcess(combatData, agentData, skillData));
        }
        return res;
    }

    // TODO: handle duplicates due multiple base method calls in Combat Replay methods
    internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
    {
        base.ComputeNPCCombatReplayActors(target, log, replay);
        foreach (var logic in _subLogics)
        {
            logic.ComputeNPCCombatReplayActors(target, log, replay);
        }
    }

    internal override void ComputePlayerCombatReplayActors(PlayerActor p, ParsedEvtcLog log, CombatReplay replay)
    {
        base.ComputePlayerCombatReplayActors(p, log, replay);
        foreach (var logic in _subLogics)
        {
            logic.ComputePlayerCombatReplayActors(p, log, replay);
        }
    }

    internal override void ComputeEnvironmentCombatReplayDecorations(ParsedEvtcLog log, CombatReplayDecorationContainer environmentDecorations)
    {
        base.ComputeEnvironmentCombatReplayDecorations(log, environmentDecorations);
        foreach (var logic in _subLogics)
        {
            logic.ComputeEnvironmentCombatReplayDecorations(log, environmentDecorations);
        }
    }

    internal override Dictionary<TargetID, int> GetTargetsSortIDs()
    {
        var sortIDs = new Dictionary<TargetID, int>();
        int offset = 0;
        foreach (var logic in _subLogics)
        {
            offset = AddSortIDWithOffset(sortIDs, logic.GetTargetsSortIDs(), offset);
        }
        return sortIDs;
    }
}
