﻿using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicPhaseUtils;
using static GW2EIEvtcParser.ParserHelpers.EncounterImages;
using static GW2EIEvtcParser.SkillIDs;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIEvtcParser.EncounterLogic;

internal class Mordremoth : StoryInstance
{
    public Mordremoth(int triggerID) : base(triggerID)
    {
        Extension = "mordr";
        Icon = EncounterIconMordremoth;
        EncounterCategoryInformation.InSubCategoryOrder = 0;
        EncounterID |= 0x000201;
    }

    internal override IReadOnlyList<TargetID> GetTrashMobsIDs()
    {
        return
        [
            TargetID.SmotheringShadow,
        ];
    }

    protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
    {
        return new CombatReplayMap(CombatReplayMordremoth,
                        (899, 1172),
                        (-9059, 10171, -6183, 13149));
    }

    internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
    {
        List<PhaseData> phases = GetInitialPhase(log);
        SingleActor mainTarget = Targets.FirstOrDefault(x => x.IsSpecies(TargetID.Mordremoth)) ?? throw new MissingKeyActorsException("Mordremoth not found");
        phases[0].AddTarget(mainTarget, log);
        if (!requirePhases)
        {
            return phases;
        }
        // Invul check
        phases.AddRange(GetPhasesByInvul(log, Determined762, mainTarget, false, true));
        for (int i = 1; i < phases.Count; i++)
        {
            PhaseData phase = phases[i];
            phase.AddParentPhase(phases[0]);
            phase.Name = "Phase " + i;
            phase.AddTarget(mainTarget, log);
        }
        return phases;
    }

    internal override IReadOnlyList<TargetID>  GetTargetsIDs()
    {
        return
        [
            TargetID.Mordremoth,
            TargetID.BlightedRytlock,
            //TargetID.BlightedCanach,
            TargetID.BlightedBraham,
            TargetID.BlightedMarjory,
            TargetID.BlightedCaithe,
            TargetID.BlightedForgal,
            TargetID.BlightedSieran,
            //TargetID.BlightedTybalt,
            //TargetID.BlightedPaleTree,
            //TargetID.BlightedTrahearne,
            //TargetID.BlightedEir,
        ];
    }

    internal override void CheckSuccess(CombatData combatData, AgentData agentData, FightData fightData, IReadOnlyCollection<AgentItem> playerAgents)
    {
        SingleActor mordremoth = Targets.FirstOrDefault(x => x.IsSpecies(TargetID.Mordremoth)) ?? throw new MissingKeyActorsException("Mordremoth not found");
        BuffApplyEvent? buffApply = combatData.GetBuffDataByIDByDst(Determined895, mordremoth.AgentItem).OfType<BuffApplyEvent>().LastOrDefault();
        if (buffApply != null)
        {
            fightData.SetSuccess(true, mordremoth.LastAware);
        } 
        else
        {
            fightData.SetSuccess(false, mordremoth.LastAware);
        }
    }

    internal override FightData.EncounterMode GetEncounterMode(CombatData combatData, AgentData agentData, FightData fightData)
    {
        SingleActor mordremoth = Targets.FirstOrDefault(x => x.IsSpecies(TargetID.Mordremoth)) ?? throw new MissingKeyActorsException("Mordremoth not found");
        return (mordremoth.GetHealth(combatData) > 9e6) ? FightData.EncounterMode.CM : FightData.EncounterMode.Story;
    }

    internal override IReadOnlyList<TargetID>  GetFriendlyNPCIDs()
    {
        return
        [
            TargetID.Canach,
            TargetID.Braham,
            TargetID.Caithe,
        ];
    }
}
