﻿
using System;
using System.Numerics;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using GW2EIEvtcParser.ParserHelpers;
using static GW2EIEvtcParser.EncounterLogic.EncounterImages;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicPhaseUtils;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicUtils;
using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic;

internal class DecimaTheStormsinger : MountBalrior
{
    public DecimaTheStormsinger(int triggerID) : base(triggerID)
    {
        MechanicList.AddRange(new List<Mechanic>()
        {
            new PlayerDstHitMechanic(ChorusOfThunderDamage, "Chorus of Thunder", new MechanicPlotlySetting(Symbols.Circle, Colors.LightOrange), "ChorThun.H", "Hit by Chorus of Thunder (Spreads AoE / Conduit AoE)", "Chorus of Thunder Hit", 0),
            new PlayerDstHitMechanic(SeismicCrashDamage, "Seismic Crash", new MechanicPlotlySetting(Symbols.Hourglass, Colors.White), "SeisCrash.H", "Hit by Seismic Crash (Concentric Rings)", "Seismic Crash Hit", 0),
            new PlayerDstHitMechanic(SeismicCrashDamage, "Seismic Crash", new MechanicPlotlySetting(Symbols.Hourglass, Colors.DarkWhite), "SeisCrash.CC", "CC by Seismic Crash (Concentric Rings)", "Seismic Crash CC", 0).UsingChecker((hde, log) => !hde.To.HasBuff(log, Stability, hde.Time, ServerDelayConstant)),
            new PlayerDstHitMechanic(Earthrend, "Earthrend", new MechanicPlotlySetting(Symbols.CircleOpen, Colors.Blue), "Earthrend.H", "Hit by Earthrend (Outer Doughnut)", "Earthrend Hit", 0),
            new PlayerDstHitMechanic(Earthrend, "Earthrend", new MechanicPlotlySetting(Symbols.CircleOpen, Colors.DarkBlue), "Earthrend.CC", "CC by Earthrend (Outer Doughnut)", "Earthrend CC", 0).UsingChecker((hde, log) => !hde.To.HasBuff(log, Stability, hde.Time, ServerDelayConstant)),
            new PlayerDstHitMechanic(Fluxlance, "Fluxlance", new MechanicPlotlySetting(Symbols.StarSquare, Colors.LightOrange), "Fluxlance.H", "Hit by Fluxlance (Single Orange Arrow)", "Fluxlance Hit", 0),
            new PlayerDstHitMechanic(FluxlanceFusillade, "Fluxlance Fusillade", new MechanicPlotlySetting(Symbols.StarDiamond, Colors.LightOrange), "FluxFusi.H", "Hit by Fluxlance Fusillade (Sequential Orange Arrows)", "Fluxlance Fusillade Hit", 0),
            new PlayerDstHitMechanic([FluxlanceSalvo1, FluxlanceSalvo2, FluxlanceSalvo3, FluxlanceSalvo4, FluxlanceSalvo5], "Fluxlance Salvo", new MechanicPlotlySetting(Symbols.StarDiamondOpen, Colors.LightOrange), "FluxSalvo.H", "Hit by Fluxlance Salvo (Simultaneous Orange Arrows)", "Fluxlance Salvo Hit", 0),
            new PlayerDstHitMechanic([Fluxlance, FluxlanceFusillade, FluxlanceSalvo1, FluxlanceSalvo2, FluxlanceSalvo3, FluxlanceSalvo4, FluxlanceSalvo5], "Fluxlance", new MechanicPlotlySetting(Symbols.DiamondWide, Colors.DarkMagenta), "FluxInc.H", "Hit by Fluxlance with Harmonic Sensitivity", "Fluxlance with Harmonic Sensitivity Hit", 0),
            new PlayerDstHitMechanic(SparkingAuraTier1, "Sparking Aura", new MechanicPlotlySetting(Symbols.CircleX, Colors.Green), "SparkAura1.H", "Sparking Aura (Absorbed Tier 1 Green Damage)", "Absorbed Tier 1 Green", 0),
            new PlayerDstHitMechanic(SparkingAuraTier2, "Sparking Aura", new MechanicPlotlySetting(Symbols.CircleX, Colors.LightMilitaryGreen), "SparkAura2.H", "Sparking Aura (Absorbed Tier 2 Green Damage)", "Absorbed Tier 2 Green", 0),
            new PlayerDstHitMechanic(SparkingAuraTier3, "Sparking Aura", new MechanicPlotlySetting(Symbols.CircleX, Colors.DarkGreen), "SparkAura3.H", "Sparking Aura (Absorbed Tier 3 Green Damage)", "Absorbed Tier 3 Green", 0),
            new PlayerDstHitMechanic([SparkingAuraTier1, SparkingAuraTier2, SparkingAuraTier3], "Sparking Aura", new MechanicPlotlySetting(Symbols.CircleX, Colors.MilitaryGreen), "SparkAuraInc.H", "Hit by Sparking Aura with Galvanic Sensitivity", "Sparking Aura with Galvanic Sensitivity Hit", 0),
            new PlayerDstHitMechanic(FulgentFence, "Fulgent Fence", new MechanicPlotlySetting(Symbols.Octagon, Colors.Purple), "FulFence.H", "Hit by Fulgent Fence (Barriers between Conduits)", "Fulgence Fence Hit", 0),
            new PlayerDstHitMechanic(ReverberatingImpact, "Reverberating Impact", new MechanicPlotlySetting(Symbols.StarOpen, Colors.LightBlue), "RevImpact.H", "Hit by Reverberating Impact (Hit a Conduit)", "Reverberating Impact Hit", 0),
            new PlayerDstHitMechanic([FulgentAuraTier1, FulgentAuraTier2, FulgentAuraTier3], "Fulgent Aura", new MechanicPlotlySetting(Symbols.CircleXOpen, Colors.Purple), "FulAura.H", "Hit by Fulgent Aura (Conduit AoE)", "Fulgent Aura Hit", 0),
            new PlayerDstSkillMechanic(Earthrend, "Earthrend", new MechanicPlotlySetting(Symbols.CircleCrossOpen, Colors.Red), "Earthrend.D", "Earthrend Death (Hitbox)", "Earthrend Death", 0).UsingChecker((hde, log) => hde.To.IsDead(log, hde.Time)),
            new PlayerDstSkillMechanic(SeismicCrashHitboxDamage, "Seismic Crash", new MechanicPlotlySetting(Symbols.CircleCross, Colors.Red), "SeisCrash.D", "Seismic Crash Death (Hitbox)", "Seismic Crash Death", 0).UsingChecker((hde, log) => hde.To.IsDead(log, hde.Time)),
            new PlayerDstBuffApplyMechanic([TargetOrder1JW, TargetOrder2JW, TargetOrder3JW, TargetOrder4JW, TargetOrder5JW], "Target Order", new MechanicPlotlySetting(Symbols.StarTriangleDown, Colors.LightOrange), "FluxOrder.T", "Targeted by Fluxlance (Target Order)", "Fluxlance Target (Sequential)", 0),
            new PlayerDstBuffApplyMechanic(FluxlanceTargetBuff1, "Fluxlance", new MechanicPlotlySetting(Symbols.StarTriangleDown, Colors.Orange), "Fluxlance.T", "Targeted by Fluxlance", "Fluxlance Target", 0),
            new PlayerDstBuffApplyMechanic(FluxlanceRedArrowTargetBuff, "Fluxlance", new MechanicPlotlySetting(Symbols.StarTriangleDown, Colors.Red), "FluxRed.T", "Targeted by Fluxlance (Red Arrow)", "Fluxlance (Red Arrow)", 0),
            new PlayerDstEffectMechanic(EffectGUIDs.DecimaChorusOfThunderAoE, "Chorus of Thunder", new MechanicPlotlySetting(Symbols.Circle, Colors.LightGrey), "ChorThun.T", "Targeted by Chorus of Thunder (Spreads)", "Chorus of Thunder Target", 0),
            new EnemyDstBuffApplyMechanic(ChargeDecima, "Charge", new MechanicPlotlySetting(Symbols.BowtieOpen, Colors.DarkMagenta), "Charge", "Charge Stacks", "Charge Stack", 0),
            new EnemyDstBuffApplyMechanic(Exposed31589, "Exposed", new MechanicPlotlySetting(Symbols.BowtieOpen, Colors.LightPurple), "Exposed", "Got Exposed (Broke Breakbar)", "Exposed", 0),
        });
        Extension = "decima";
        Icon = EncounterIconDecima;
        EncounterCategoryInformation.InSubCategoryOrder = 1;
        EncounterID |= 0x000002;
    }
    protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
    {
        return new CombatReplayMap(CombatReplayDecimaTheStormsinger,
                        (1602, 1602),
                        (-12668, 10500, -7900, 15268));
    }

    protected override ReadOnlySpan<int> GetTargetsIDs()
    {
        return
        [
            (int)ArcDPSEnums.TargetID.Decima,
        ];
    }

    protected override List<ArcDPSEnums.TrashID> GetTrashMobsIDs()
    {
        return
        [
            ArcDPSEnums.TrashID.GreenOrb1Player,
            ArcDPSEnums.TrashID.GreenOrb2Players,
            ArcDPSEnums.TrashID.GreenOrb3Players,
            ArcDPSEnums.TrashID.EnlightenedConduit,
            ArcDPSEnums.TrashID.EnlightenedConduitGadget,
            ArcDPSEnums.TrashID.DecimaBeamStart,
            ArcDPSEnums.TrashID.DecimaBeamEnd,
        ];
    }

    internal override List<InstantCastFinder> GetInstantCastFinders()
    {
        return
        [
            new DamageCastFinder(ThrummingPresenceBuff, ThrummingPresenceDamage),
        ];
    }

    internal override void EIEvtcParse(ulong gw2Build, EvtcVersionEvent evtcVersion, FightData fightData, AgentData agentData, List<CombatItem> combatData, IReadOnlyDictionary<uint, ExtensionHandler> extensions)
    {
        var conduitsGadgets = combatData
            .Where(x => x.IsStateChange == ArcDPSEnums.StateChange.MaxHealthUpdate && MaxHealthUpdateEvent.GetMaxHealth(x) == 15276)
            .Select(x => agentData.GetAgent(x.SrcAgent, x.Time))
            .Where(x => x.Type == AgentItem.AgentType.Gadget && x.HitboxWidth == 100 && x.HitboxHeight == 200)
            .Distinct();
        foreach (var conduit in conduitsGadgets)
        {
            conduit.OverrideID(ArcDPSEnums.TrashID.EnlightenedConduitGadget);
            conduit.OverrideType(AgentItem.AgentType.NPC);
        }
        agentData.Refresh();
        ComputeFightTargets(agentData, combatData, extensions);
    }

    internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
    {
        List<PhaseData> phases = GetInitialPhase(log);
        SingleActor decima = Targets.FirstOrDefault(x => x.IsSpecies(ArcDPSEnums.TargetID.Decima)) ?? throw new MissingKeyActorsException("Decima not found");
        phases[0].AddTarget(decima);
        if (!requirePhases)
        {
            return phases;
        }
        // Invul check
        phases.AddRange(GetPhasesByInvul(log, NovaShield, decima, true, true));
        for (int i = 1; i < phases.Count; i++)
        {
            PhaseData phase = phases[i];
            if (i % 2 == 0)
            {
                phase.Name = "Split " + (i) / 2;
                phase.AddTarget(decima);
            }
            else
            {
                phase.Name = "Phase " + (i + 1) / 2;
                phase.AddTarget(decima);
            }
        }
        return phases;
    }


    internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
    {
        var lifespan = ((int)replay.TimeOffsets.start, (int)replay.TimeOffsets.end);
        switch (target.ID)
        {
            case (int)ArcDPSEnums.TargetID.Decima:
                var casts = target.GetCastEvents(log, log.FightData.FightStart, log.FightData.FightEnd).ToList();

                // Thrumming Presence - Red Ring around Decima
                var thrummingSegments = target.GetBuffStatus(log, ThrummingPresenceBuff, log.FightData.FightStart, log.FightData.FightEnd).Where(x => x.Value > 0);
                foreach (var segment in thrummingSegments)
                {
                    replay.Decorations.Add(new CircleDecoration(700, segment.TimeSpan, Colors.Red, 0.2, new AgentConnector(target)).UsingFilled(false));
                }

                // Add the Charge Bar on top right of the replay
                var chargeSegments = target.GetBuffStatus(log, ChargeDecima, log.FightData.FightStart, log.FightData.FightEnd);
                ReadOnlySpan<(Color, double)> colors =
                [
                    (Colors.Red, 0.6),
                    (Colors.Black, 0.4),
                    (Colors.LightRed, 0.8),
                ];
                replay.AddDynamicBar(target, chargeSegments, 10, 1800, -2000, 1000, 100, 0, colors);

                // Mainshock - Pizza Indicator
                if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaMainshockIndicator, out var mainshockSlices))
                {
                    foreach (EffectEvent effect in mainshockSlices)
                    {
                        long duration = 2300;
                        long growing = effect.Time + duration;
                        (long start, long end) lifespan2 = effect.ComputeLifespan(log, duration);
                        var rotation = new AngleConnector(effect.Rotation.Z + 90);
                        var slice = (PieDecoration)new PieDecoration(1200, 32, lifespan2, Colors.LightOrange, 0.4, new PositionConnector(effect.Position)).UsingRotationConnector(rotation);
                        replay.AddDecorationWithBorder(slice, Colors.LightOrange, 0.6);
                    }
                }

                // For some reason the effects all start at the same time
                // We sequence them using the skill cast
                var foreshock = casts.Where(x => x.SkillId == Foreshock);
                foreach (var cast in foreshock)
                {
                    (long start, long end) = (cast.Time, cast.Time + cast.ActualDuration + 3000); // 3s padding as safety
                    long nextStartTime = 0;

                    // Decima's Left Side
                    if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaForeshockLeft, out var foreshockLeft))
                    {
                        foreach (EffectEvent effect in foreshockLeft.Where(x => x.Time >= start && x.Time < end))
                        {
                            (long start, long end) lifespanLeft = effect.ComputeLifespan(log, 1967);
                            nextStartTime = lifespanLeft.end;
                            var rotation = new AngleConnector(effect.Rotation.Z + 90);
                            var leftHalf = (PieDecoration)new PieDecoration(1185, 180, lifespanLeft, Colors.LightOrange, 0.4, new PositionConnector(effect.Position)).UsingRotationConnector(rotation);
                            replay.AddDecorationWithBorder(leftHalf, Colors.LightOrange, 0.6);
                        }
                    }

                    // Decima's Right Side
                    if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaForeshockRight, out var foreshockRight))
                    {
                        foreach (EffectEvent effect in foreshockRight.Where(x => x.Time >= start && x.Time < end))
                        {
                            (long start, long end) lifespanRight = effect.ComputeLifespan(log, 3000);
                            lifespanRight.start = nextStartTime - 700; // Trying to match in game timings
                            nextStartTime = lifespanRight.end;
                            var rotation = new AngleConnector(effect.Rotation.Z + 90);
                            var rightHalf = (PieDecoration)new PieDecoration(1185, 180, lifespanRight, Colors.LightOrange, 0.4, new PositionConnector(effect.Position)).UsingRotationConnector(rotation);
                            replay.AddDecorationWithBorder(rightHalf, Colors.LightOrange, 0.6);
                        }
                    }

                    // Decima's Frontal
                    if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaForeshockFrontal, out var foreshockFrontal))
                    {
                        foreach (EffectEvent effect in foreshockFrontal.Where(x => x.Time >= start && x.Time < end))
                        {
                            (long start, long end) lifespanFrontal = effect.ComputeLifespan(log, 5100);
                            lifespanFrontal.start = nextStartTime;
                            var frontalCircle = new CircleDecoration(600, lifespanFrontal, Colors.LightOrange, 0.4, new PositionConnector(effect.Position));
                            replay.AddDecorationWithBorder(frontalCircle, Colors.LightOrange, 0.6);
                        }
                    }
                }

                // Earthrend - Outer Sliced Doughnut - 8 Slices
                if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaEarthrendDoughnutSlice, out var earthrend))
                {
                    // Since we don't have a decoration shaped like this, we regroup the 8 effects and use Decima position as the center for a doughnut sliced by lines.
                    var filtered = new List<EffectEvent>();
                    foreach (var effect in earthrend)
                    {
                        if (filtered.FirstOrDefault(x => Math.Abs(x.Time - effect.Time) < ServerDelayConstant) != null)
                        {
                            continue;
                        }
                        filtered.Add(effect);
                    }

                    foreach (EffectEvent effect in filtered)
                    {
                        uint inner = 1200;
                        uint outer = 3000;
                        int lineAngle = 45;
                        var offset = new Vector3(0, inner + (outer - inner) / 2, 0);
                        (long start, long end) lifespanRing = effect.ComputeLifespan(log, 2800);

                        if (target.TryGetCurrentFacingDirection(log, effect.Time, out Vector3 facing, 100))
                        {
                            for (int i = 0; i < 360; i += lineAngle)
                            {
                                var rotation = facing.GetRoundedZRotationDeg() + i;
                                var line = new RectangleDecoration(10, outer - inner, lifespanRing, Colors.LightOrange, 0.6, new AgentConnector(target).WithOffset(offset, true)).UsingRotationConnector(new AngleConnector(rotation));
                                replay.Decorations.Add(line);
                            }
                        }

                        var doughnut = new DoughnutDecoration(inner, outer, lifespanRing, Colors.LightOrange, 0.2, new AgentConnector(target));
                        replay.AddDecorationWithBorder(doughnut, Colors.LightOrange, 0.6);
                    }
                }

                // Seismic Crash - Jump with rings
                if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaSeismicCrashRings, out var seismicCrash))
                {
                    foreach (var effect in seismicCrash)
                    {
                        (long start, long end) lifespanCrash = effect.ComputeLifespan(log, 3000);
                        replay.AddContrenticRings(300, 140, lifespanCrash, effect.Position, Colors.LightOrange, 0.30f, 6, false);
                    }
                }

                // Jump Death Zone
                if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaJumpAoEUnderneath, out var deathZone))
                {
                    foreach (var effect in deathZone)
                    {
                        // Logged effect has 2 durations depending on attack - 3000 and 2500
                        (long start, long end) lifespanDeathzone = effect.ComputeLifespan(log, effect.Duration);
                        var zone = new CircleDecoration(300, lifespanDeathzone, Colors.Red, 0.2, new PositionConnector(effect.Position));
                        replay.AddDecorationWithGrowing(zone, effect.Time + effect.Duration);
                    }
                }

                // Aftershock - Moving AoEs - 4 Cascades 
                if (log.CombatData.TryGetEffectEventsBySrcWithGUID(target.AgentItem, EffectGUIDs.DecimaAftershockAoE, out var aftershock))
                {
                    // All the AoEs take roughly 11-12 seconds to appear
                    // There are 10 AoEs of radius 200, then 10 of 240, 10 of 280, etc.

                    uint radius = 200;
                    float distance = 0;
                    EffectEvent first = aftershock.FirstOrDefault()!;
                    List<EffectEvent>? currentGroup = [];
                    List<List<EffectEvent>>? groups = [];
                    long groupStartTime = first.Time;

                    foreach (var effect in aftershock)
                    {
                        if (effect.Time <= groupStartTime + 12000)
                        {
                            currentGroup.Add(effect);
                        }
                        else
                        {
                            groups.Add(new List<EffectEvent>(currentGroup));
                            currentGroup.Clear();
                            currentGroup.Add(effect);
                            groupStartTime = effect.Time;
                        }
                    }

                    // Last group
                    groups.Add(new List<EffectEvent>(currentGroup));

                    // Because the x9th and the x0th can happen at the same timestamp, we need to check the distance of the from Decima.
                    // A simple increase every 10 can happen to increase the x9th instead of the following x0th.
                    if (target.TryGetCurrentPosition(log, first.Time, out Vector3 decimaPosition))
                    {
                        foreach (var group in groups)
                        {
                            foreach (var effect in group)
                            {
                                distance = (effect.Position - decimaPosition).XY().Length();
                                if (distance > 1074 && distance < 1076 || distance > 1759 && distance < 1761)
                                {
                                    radius = 200;
                                }
                                if (distance > 1324 && distance < 1326 || distance > 1528 && distance < 1530)
                                {
                                    radius = 240;
                                }
                                if (distance > 1574 && distance < 1576 || distance > 1297 && distance < 1299)
                                {
                                    radius = 280;
                                }
                                if (distance > 1824 && distance < 1826 || distance > 1066 && distance < 1068)
                                {
                                    radius = 320;
                                }
                                (long start, long end) lifespanAftershock = effect.ComputeLifespan(log, 1500);
                                var zone = (CircleDecoration)new CircleDecoration(radius, lifespanAftershock, Colors.Red, 0.2, new PositionConnector(effect.Position)).UsingFilled(false);
                                replay.Decorations.Add(zone);
                            }
                        }
                    }
                }
                break;
            case (int)ArcDPSEnums.TrashID.GreenOrb1Player:
                // Green Circle
                replay.Decorations.Add(new CircleDecoration(90, lifespan, Colors.DarkGreen, 0.2, new AgentConnector(target)));

                // Overhead Number
                replay.AddOverheadIcon(lifespan, target, ParserIcons.TargetOrder1Overhead);

                // Hp Bar
                ReadOnlySpan<(Color, double)> colorsOrb1 =
                [
                    (Colors.Green, 0.4),
                    (Colors.Black, 0.4),
                    (Colors.Green, 0.5),
                ];
                var hpUpdates1 = target.GetHealthUpdates(log);
                replay.AddDynamicBar(target, hpUpdates1, 100, 0, 20, 100, 20, 0, colorsOrb1);
                break;
            case (int)ArcDPSEnums.TrashID.GreenOrb2Players:
                // Green Circle
                replay.Decorations.Add(new CircleDecoration(185, lifespan, Colors.DarkGreen, 0.2, new AgentConnector(target)));

                // Overhead Number
                replay.AddOverheadIcon(lifespan, target, ParserIcons.TargetOrder2Overhead);

                // Hp Bar
                ReadOnlySpan<(Color, double)> colorsOrb2 =
                [
                    (Colors.Green, 0.4),
                    (Colors.Black, 0.4),
                    (Colors.Green, 0.5),
                ];
                var hpUpdates2 = target.GetHealthUpdates(log);
                replay.AddDynamicBar(target, hpUpdates2, 200, 0, 20, 200, 20, 0, colorsOrb2, 2);
                break;
            case (int)ArcDPSEnums.TrashID.GreenOrb3Players:
                // Green Circle
                replay.Decorations.Add(new CircleDecoration(285, lifespan, Colors.DarkGreen, 0.2, new AgentConnector(target)));

                // Overhead Number
                replay.AddOverheadIcon(lifespan, target, ParserIcons.TargetOrder3Overhead);

                // Hp Bar
                ReadOnlySpan<(Color, double)> colorsOrb3 =
                [
                    (Colors.Green, 0.4),
                    (Colors.Black, 0.4),
                    (Colors.Green, 0.5),
                ];
                var hpUpdates3 = target.GetHealthUpdates(log);
                replay.AddDynamicBar(target, hpUpdates3, 300, 0, 20, 300, 20, 0, colorsOrb3, 3);
                break;
            case (int)ArcDPSEnums.TrashID.EnlightenedConduit:
                // Chorus of Thunder / Discordant Thunder - Orange AoE
                AddThunderAoE(target, log, replay);

                // Focused Fluxlance - Green Arrow from Decima to the Conduit
                var greenArrow = GetFilteredList(log.CombatData, FluxlanceTargetBuff1, target, true, true).Where(x => x is BuffApplyEvent);
                foreach (var apply in greenArrow)
                {
                    replay.Decorations.Add(new LineDecoration((apply.Time, apply.Time + 5500), Colors.DarkGreen, 0.2, new AgentConnector(apply.To), new AgentConnector(apply.By)).WithThickess(80, true));
                    replay.Decorations.Add(new LineDecoration((apply.Time + 5500, apply.Time + 6500), Colors.DarkGreen, 0.5, new AgentConnector(apply.To), new AgentConnector(apply.By)).WithThickess(80, true));
                }

                // Warning indicator of walls spawning between Conduits.
                var wallsWarnings = GetFilteredList(log.CombatData, DecimaConduitWallWarningBuff, target, true, true);
                replay.AddTether(wallsWarnings, Colors.Red, 0.2, 30, true);

                // Walls connecting Conduits to each other.
                var walls = GetFilteredList(log.CombatData, DecimaConduitWallBuff, target, true, true);
                replay.AddTether(walls, Colors.Purple, 0.4, 60, true);
                break;
            case (int)ArcDPSEnums.TrashID.EnlightenedConduitGadget:
                // Fulgent Aura - Tier 1 Charge
                var tier1 = target.GetBuffStatus(log, EnlightenedConduitGadgetChargeTier1Buff, log.FightData.FightStart, log.FightData.FightEnd);
                foreach (var segment in tier1.Where(x => x.Value > 0))
                {
                    replay.AddDecorationWithBorder(new CircleDecoration(100, segment.TimeSpan, Colors.DarkPurple, 0.4, new AgentConnector(target)), Colors.Red, 0.4);
                    replay.AddOverheadIcon(segment.TimeSpan, target, ParserIcons.TargetOrder1Overhead);
                }

                // Fulgent Aura - Tier 2 Charge
                var tier2 = target.GetBuffStatus(log, EnlightenedConduitGadgetChargeTier2Buff, log.FightData.FightStart, log.FightData.FightEnd);
                foreach (var segment in tier2.Where(x => x.Value > 0))
                {
                    replay.AddDecorationWithBorder(new CircleDecoration(200, segment.TimeSpan, Colors.DarkPurple, 0.4, new AgentConnector(target)), Colors.Red, 0.4);
                    replay.AddOverheadIcon(segment.TimeSpan, target, ParserIcons.TargetOrder2Overhead);
                }

                // Fulgent Aura - Tier 3 Charge
                var tier3 = target.GetBuffStatus(log, EnlightenedConduitGadgetChargeTier3Buff, log.FightData.FightStart, log.FightData.FightEnd);
                foreach (var segment in tier3.Where(x => x.Value > 0))
                {
                    replay.AddDecorationWithBorder(new CircleDecoration(400, segment.TimeSpan, Colors.DarkPurple, 0.4, new AgentConnector(target)), Colors.Red, 0.4);
                    replay.AddOverheadIcon(segment.TimeSpan, target, ParserIcons.TargetOrder3Overhead);
                }
                break;
            case (int)ArcDPSEnums.TrashID.DecimaBeamStart:
                SingleActor decima = Targets.FirstOrDefault(x => x.IsSpecies(ArcDPSEnums.TargetID.Decima)) ?? throw new MissingKeyActorsException("Decima not found");
                var decimaConnector = new AgentConnector(decima);
                const uint beamLength = 2900;
                const uint orangeBeamWidth = 80;
                const uint redBeamWidth = 160;
                var orangeBeams = GetFilteredList(log.CombatData, DecimaBeamTargeting, target.AgentItem, true, true);
                AddBeamWarning(log, target, replay, decima, DecimaBeamLoading, orangeBeamWidth, beamLength, orangeBeams.OfType<BuffApplyEvent>(), Colors.LightOrange);
                replay.AddTetherWithCustomConnectors(log, orangeBeams, Colors.LightOrange, 0.5,
                    (log, agent, start, end) =>
                    {
                        if (agent.TryGetCurrentInterpolatedPosition(log, start, out var pos))
                        {
                            return new PositionConnector(pos);
                        }
                        return null;
                    },
                    (log, agent, start, end) =>
                    {
                        return decimaConnector;
                    },
                    orangeBeamWidth, true);
                var redBeams = GetFilteredList(log.CombatData, DecimaRedBeamTargeting, target.AgentItem, true, true);
                AddBeamWarning(log, target, replay, decima, DecimaRedBeamLoading, redBeamWidth, beamLength, redBeams.OfType<BuffApplyEvent>(), Colors.Red);
                replay.AddTetherWithCustomConnectors(log, redBeams, Colors.Red, 0.5,
                    (log, agent, start, end) =>
                    {
                        if (agent.TryGetCurrentInterpolatedPosition(log, start, out var pos))
                        {
                            return new PositionConnector(pos);
                        }
                        return null;
                    },
                    (log, agent, start, end) =>
                    {
                        return decimaConnector;
                    },
                    redBeamWidth, true);
                break;
            default:
                break;
        }
    }

    private static void AddBeamWarning(ParsedEvtcLog log, SingleActor target, CombatReplay replay, SingleActor attachActor, long buffID, uint beamWidth, uint beamLength, IEnumerable<BuffApplyEvent> beamFireds, Color color)
    {
        var beamWarnings = target.AgentItem.GetBuffStatus(log, buffID, log.FightData.FightStart, log.FightData.FightEnd);
        foreach (var beamWarning in beamWarnings)
        {
            if (beamWarning.Value > 0)
            {
                long start = beamWarning.Start;
                long end = beamFireds.FirstOrDefault(x => x.Time >= start)?.Time ?? beamWarning.End;
                var connector = (AgentConnector)new AgentConnector(attachActor).WithOffset(new(beamLength / 2, 0, 0), true);
                var rotationConnector = new AgentFacingConnector(target);
                replay.Decorations.Add(new RectangleDecoration(beamLength, beamWidth, (start, end), color, 0.2, connector).UsingRotationConnector(rotationConnector));
            }
        }
    }


    internal override void ComputePlayerCombatReplayActors(PlayerActor player, ParsedEvtcLog log, CombatReplay replay)
    {
        base.ComputePlayerCombatReplayActors(player, log, replay);

        // Target Overhead
        // In phase 2 you get the Fluxlance Target Buff but also Target Order, in game only Target Order is displayed overhead, so we filter those out.
        var p2Targets = player.GetBuffStatus(log, [TargetOrder1JW, TargetOrder2JW, TargetOrder3JW, TargetOrder4JW, TargetOrder5JW], log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0);
        var allTargets = player.GetBuffStatus(log, FluxlanceTargetBuff1, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0);
        var filtered = allTargets.Where(x => !p2Targets.Any(y => Math.Abs(x.Start - y.Start) < ServerDelayConstant));
        foreach (var segment in filtered)
        {
            replay.AddOverheadIcon(segment, player, ParserIcons.TargetOverhead);
        }

        // Target Order Overhead
        replay.AddOverheadIcons(player.GetBuffStatus(log, TargetOrder1JW, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0), player, ParserIcons.TargetOrder1Overhead);
        replay.AddOverheadIcons(player.GetBuffStatus(log, TargetOrder2JW, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0), player, ParserIcons.TargetOrder2Overhead);
        replay.AddOverheadIcons(player.GetBuffStatus(log, TargetOrder3JW, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0), player, ParserIcons.TargetOrder3Overhead);
        replay.AddOverheadIcons(player.GetBuffStatus(log, TargetOrder4JW, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0), player, ParserIcons.TargetOrder4Overhead);
        replay.AddOverheadIcons(player.GetBuffStatus(log, TargetOrder5JW, log.FightData.LogStart, log.FightData.LogEnd).Where(x => x.Value > 0), player, ParserIcons.TargetOrder5Overhead);

        // Chorus of Thunder / Discordant Thunder - Orange AoE
        AddThunderAoE(player, log, replay);
    }

    /// <summary>
    /// Chorus of Thunder / Discordant Thunder - Orange spread AoE on players or on Conduits.
    /// </summary>
    private static void AddThunderAoE(SingleActor actor, ParsedEvtcLog log, CombatReplay replay)
    {
        if (log.CombatData.TryGetEffectEventsByDstWithGUID(actor.AgentItem, EffectGUIDs.DecimaChorusOfThunderAoE, out var thunders))
        {
            foreach (var effect in thunders)
            {
                long duration = 5000;
                long growing = effect.Time + duration;
                (long start, long end) lifespan = effect.ComputeLifespan(log, duration);
                replay.AddDecorationWithGrowing(new CircleDecoration(285, lifespan, Colors.LightOrange, 0.2, new AgentConnector(actor)), growing);
            }
        }
    }
}
