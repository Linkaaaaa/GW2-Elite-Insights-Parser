﻿using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;
using static GW2EIEvtcParser.EncounterLogic.EncounterCategory;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicPhaseUtils;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicTimeUtils;
using static GW2EIEvtcParser.ParserHelpers.EncounterImages;
using static GW2EIEvtcParser.SkillIDs;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIEvtcParser.EncounterLogic;

internal class MountBalriorConvergenceInstance : ConvergenceLogic
{
    public MountBalriorConvergenceInstance(int triggerID) : base(triggerID)
    {
        EncounterCategoryInformation.SubCategory = SubFightCategory.MountBalriorConvergence;
        EncounterID |= EncounterIDs.ConvergenceMasks.MountBalriorMask;
        Icon = InstanceIconMountBalrior;
        Extension = "mntbalrconv";
    }

    internal override string GetLogicName(CombatData combatData, AgentData agentData)
    {
        return "Convergence: Mount Balrior";
    }

    protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
    {
        return new CombatReplayMap(CombatReplayMountBalrior, 
            (1280, 1280),
            (-15454, -22004, 20326, 20076));
    }

    internal override IReadOnlyList<TargetID> GetTargetsIDs()
    {
        return
        [
            TargetID.GreerTheBlightbringerConv,
            TargetID.GreeTheBingerConv,
            TargetID.ReegTheBlighterConv,
            TargetID.DecimaTheStormsingerConv,
            TargetID.UraTheSteamshriekerConv,
        ];
    }

    internal override FightData.EncounterMode GetEncounterMode(CombatData combatData, AgentData agentData, FightData fightData)
    {
        return combatData.GetBuffApplyData(UnstableAttunementJW).Any(x => x.To.IsPlayer) ? FightData.EncounterMode.CM : FightData.EncounterMode.Normal;
    }

    internal override FightData.InstancePrivacyMode GetInstancePrivacyMode(CombatData combatData, AgentData agentData, FightData fightData)
    {
        return combatData.GetMapIDEvents().Any(x => x.MapID == MapIDs.MountBalriorPublicConvergence) ? FightData.InstancePrivacyMode.Public : FightData.InstancePrivacyMode.Private;
    }

    internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
    {
        var phases = GetInitialPhase(log);
        phases[0].AddTargets(Targets.Where(x => x.IsAnySpecies([TargetID.DecimaTheStormsingerConv, TargetID.GreerTheBlightbringerConv, TargetID.UraTheSteamshriekerConv])), log);
        var target = Targets.FirstOrDefault(x => x.IsAnySpecies([TargetID.DecimaTheStormsingerConv, TargetID.GreerTheBlightbringerConv, TargetID.UraTheSteamshriekerConv]));
        if (target == null)
        {
            return phases;
        }
        IReadOnlyList<Segment> hpUpdates = target.GetHealthUpdates(log);

        // Full Fight Phase
        string phaseName = "";
        switch (target.ID)
        {
            case (int)TargetID.DecimaTheStormsingerConv:
                phaseName = "Full Decima";
                break;
            case (int)TargetID.GreerTheBlightbringerConv:
                phaseName = "Full Greer";
                break;
            case (int)TargetID.UraTheSteamshriekerConv:
                phaseName = "Full Ura";
                break;
        }
        var fullPhase = new PhaseData(Math.Max(log.FightData.FightStart, target.FirstAware), Math.Min(target.LastAware, log.FightData.FightEnd), phaseName).WithParentPhase(phases[0]);
        fullPhase.AddTarget(target, log);
        phases.Add(fullPhase);
        if (!requirePhases)
        {
            return phases;
        }

        // Sub Phases
        Segment start = hpUpdates.FirstOrDefault(x => x.Value <= 100.0 && x.Value != 0 && x.Start != 0);
        Segment end75 = hpUpdates.FirstOrDefault(x => x.Value < 75.0 && x.Value != 0);
        Segment start75 = hpUpdates.FirstOrDefault(x => x.Value < 75.0 && x.Value != 0 && x.Start > end75.End);
        Segment end50 = hpUpdates.FirstOrDefault(x => x.Value < 50.0 && x.Value != 0);
        Segment start50 = hpUpdates.FirstOrDefault(x => x.Value < 50.0 && x.Value != 0 && x.Start > end50.End);
        Segment end25 = hpUpdates.FirstOrDefault(x => x.Value < 25.0 && x.Value != 0);
        Segment final = hpUpdates.FirstOrDefault(x => x.Value < 25.0 && x.Start > end25.End);

        // 100-75, Warclaw, 75-50, Warclaw, 50-25, Warclaw, 25-0
        var phase1 = new PhaseData(start.Start, Math.Min(end75.Start, log.FightData.FightEnd), "Phase 1").WithParentPhase(fullPhase);
        var phase2 = new PhaseData(start75.Start, Math.Min(end50.Start, log.FightData.FightEnd), "Phase 2").WithParentPhase(fullPhase);
        var phase3 = new PhaseData(start50.Start, Math.Min(end25.Start, log.FightData.FightEnd), "Phase 3").WithParentPhase(fullPhase);
        var phase4 = new PhaseData(final.Start, Math.Min(target.AgentItem.LastAware, log.FightData.FightEnd), "Phase 4").WithParentPhase(fullPhase);
        var warclaw1 = new PhaseData(end75.Start, Math.Min(start75.Start, log.FightData.FightEnd), "Warclaw 1").WithParentPhase(fullPhase);
        var warclaw2 = new PhaseData(end50.Start, Math.Min(start50.Start, log.FightData.FightEnd), "Warclaw 2").WithParentPhase(fullPhase);
        var warclaw3 = new PhaseData(end25.Start, Math.Min(final.Start, log.FightData.FightEnd), "Warclaw 3").WithParentPhase(fullPhase);

        phase1.AddTarget(target, log);
        phase2.AddTarget(target, log);
        phase3.AddTarget(target, log);
        phase4.AddTarget(target, log);
        warclaw1.AddTarget(target, log);
        warclaw2.AddTarget(target, log);
        warclaw3.AddTarget(target, log);

        phases.AddRange([phase1, phase2, phase3, phase4, warclaw1, warclaw2, warclaw3]);

        return phases;
    }

    internal override void EIEvtcParse(ulong gw2Build, EvtcVersionEvent evtcVersion, FightData fightData, AgentData agentData, List<CombatItem> combatData, IReadOnlyDictionary<uint, ExtensionHandler> extensions)
    {
        base.EIEvtcParse(gw2Build, evtcVersion, fightData, agentData, combatData, extensions);

        SingleActor? ura = Targets.FirstOrDefault(x => x.IsSpecies(TargetID.UraTheSteamshriekerConv));
        ura?.OverrideName("Ura, the Steamshrieker");
    }
}
