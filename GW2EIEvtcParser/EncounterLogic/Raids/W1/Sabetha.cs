﻿using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Exceptions;
using GW2EIEvtcParser.ParsedData;
using GW2EIEvtcParser.ParserHelpers;
using static GW2EIEvtcParser.ParserHelpers.EncounterImages;
using static GW2EIEvtcParser.EncounterLogic.EncounterLogicPhaseUtils;
using static GW2EIEvtcParser.SkillIDs;

namespace GW2EIEvtcParser.EncounterLogic;

internal class Sabetha : SpiritVale
{
    public Sabetha(int triggerID) : base(triggerID)
    {
        MechanicList.AddRange(new List<Mechanic>
        {
        new PlayerDstBuffApplyMechanic(ShellShocked, "Shell-Shocked", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.DarkGreen), "Launched","Shell-Shocked (launched up to cannons)", "Shell-Shocked",0),
        new PlayerDstBuffApplyMechanic(SapperBombBuff, "Sapper Bomb", new MechanicPlotlySetting(Symbols.Circle,Colors.DarkGreen), "Sap Bomb","Got a Sapper Bomb", "Sapper Bomb",0),
        new PlayerDstBuffApplyMechanic(TimeBomb, "Time Bomb", new MechanicPlotlySetting(Symbols.Circle,Colors.LightOrange), "Timed Bomb","Got a Timed Bomb (Expanding circle)", "Timed Bomb",0),
        /*new PlayerBoonApplyMechanic(31324, "Time Bomb Hit", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.LightOrange), "Timed Bomb Hit","Got hit by Timed Bomb (Expanding circle)", "Timed Bomb Hit",0,
            (ba, log) =>
            {
                List<AbstractBuffEvent> timedBombRemoved = log.CombatData.GetBoonData(31485).Where(x => x.To == ba.To && x is BuffRemoveAllEvent && Math.Abs(ba.Time - x.Time) <= 50).ToList();
                if (timedBombRemoved.Count > 0)
                {
                    return false;
                }
                return true;
           }
        }),
        new PlayerBoonApplyMechanic(34152, "Time Bomb Hit", new MechanicPlotlySetting(Symbols.CircleOpen,Colors.LightOrange), "Timed Bomb Hit","Got hit by Timed Bomb (Expanding circle)", "Timed Bomb Hit",0,
            (ba, log) =>
            {
                List<AbstractBuffEvent> timedBombRemoved = log.CombatData.GetBoonData(31485).Where(x => x.To == ba.To && x is BuffRemoveAllEvent && Math.Abs(ba.Time - x.Time) <= 50).ToList();
                if (timedBombRemoved.Count > 0)
                {
                    return false;
                }
                return true;
           }
        }),*/
        new PlayerDstSkillMechanic(Firestorm, "Firestorm", new MechanicPlotlySetting(Symbols.Square,Colors.Red), "Flamewall","Firestorm (killed by Flamewall)", "Flamewall",0).UsingChecker((de, log) => de.HasKilled),
        new PlayerDstHitMechanic(FlakShot, "Flak Shot", new MechanicPlotlySetting(Symbols.HexagramOpen,Colors.LightOrange), "Flak","Flak Shot (Fire Patches)", "Flak Shot",0),
        new PlayerDstHitMechanic(CannonBarrage, "Cannon Barrage", new MechanicPlotlySetting(Symbols.Circle,Colors.Yellow), "Cannon","Cannon Barrage (stood in AoE)", "Cannon Shot",0),
        new PlayerDstHitMechanic(FlameBlast, "Flame Blast", new MechanicPlotlySetting(Symbols.TriangleLeftOpen,Colors.Yellow), "Karde Flame","Flame Blast (Karde's Flamethrower)", "Flamethrower (Karde)",0),
        new PlayerDstHitMechanic(BanditKick, "Kick", new MechanicPlotlySetting(Symbols.TriangleRight,Colors.Magenta), "Kick","Kicked by Bandit", "Bandit Kick",0).UsingChecker((de, log) => !de.To.HasBuff(log, Stability, de.Time - ParserHelper.ServerDelayConstant)),
        new EnemyCastStartMechanic(PlatformQuake, "Platform Quake", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.DarkTeal), "CC","Platform Quake (Breakbar)","Breakbar",0),
        new EnemyCastEndMechanic(PlatformQuake, "Platform Quake", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.DarkGreen), "CCed","Platform Quake (Breakbar broken) ", "CCed",0).UsingChecker((ce, log) => ce.ActualDuration <= 4400),
        new EnemyCastEndMechanic(PlatformQuake, "Platform Quake", new MechanicPlotlySetting(Symbols.DiamondTall,Colors.Red), "CC Fail","Platform Quake (Breakbar failed) ", "CC Fail",0).UsingChecker( (ce,log) =>  ce.ActualDuration > 4400),
        // Hit by Time Bomb could be implemented by checking if a person is affected by ID 31324 (1st Time Bomb) or 34152 (2nd Time Bomb, only below 50% boss HP) without being attributed a bomb (ID: 31485) 3000ms before (+-50ms). I think the actual heavy hit isn't logged because it may be percentage based. Nothing can be found in the logs.
        });
        Extension = "sab";
        Icon = EncounterIconSabetha;
        EncounterCategoryInformation.InSubCategoryOrder = 3;
        EncounterID |= 0x000003;
    }

    protected override CombatReplayMap GetCombatMapInternal(ParsedEvtcLog log)
    {
        return new CombatReplayMap(CombatReplaySabetha,
                        (1000, 990),
                        (-8587, -162, -1601, 6753)/*,
                        (-15360, -36864, 15360, 39936),
                        (3456, 11012, 4736, 14212)*/);
    }

    internal override List<PhaseData> GetPhases(ParsedEvtcLog log, bool requirePhases)
    {
        List<PhaseData> phases = GetInitialPhase(log);
        SingleActor sabetha = Targets.FirstOrDefault(x => x.IsSpecies(ArcDPSEnums.TargetID.Sabetha)) ?? throw new MissingKeyActorsException("Sabetha not found");
        phases[0].AddTarget(sabetha);
        var miniBossIds = new List<int>
        {
            (int) ArcDPSEnums.TrashID.Karde, // reverse order for mini boss phase detection
            (int) ArcDPSEnums.TrashID.Knuckles,
            (int) ArcDPSEnums.TrashID.Kernan,
        };
        phases[0].AddTargets(Targets.Where(x => x.IsAnySpecies(miniBossIds)), PhaseData.TargetPriority.Blocking);
        if (!requirePhases)
        {
            return phases;
        }
        // Invul check
        phases.AddRange(GetPhasesByInvul(log, Invulnerability757, sabetha, true, true));
        for (int i = 1; i < phases.Count; i++)
        {
            PhaseData phase = phases[i];
            if (i % 2 == 0)
            {
                int phaseID = i / 2;
                phase.Name = "Unknown";
                foreach (var miniBossId in miniBossIds)
                {
                    AddTargetsToPhaseAndFit(phase, [miniBossId], log);
                    if (phase.Targets.Count > 0)
                    {
                        SingleActor phaseTarget = phase.Targets.Keys.First();
                        if (PhaseNames.TryGetValue(phaseTarget.ID, out var phaseName))
                        {
                            phase.Name = phaseName;
                        }
                        break; // we found our main target
                    }
                }
                AddTargetsToPhase(phase, miniBossIds, PhaseData.TargetPriority.NonBlocking);
            }
            else
            {
                int phaseID = (i + 1) / 2;
                phase.Name = "Phase " + phaseID;
                phase.AddTarget(sabetha);
                AddTargetsToPhase(phase, miniBossIds, PhaseData.TargetPriority.NonBlocking);
            }
        }
        return phases;
    }

    protected override ReadOnlySpan<int> GetTargetsIDs()
    {
        return
        [
            (int)ArcDPSEnums.TargetID.Sabetha,
            (int)ArcDPSEnums.TrashID.Kernan,
            (int)ArcDPSEnums.TrashID.Knuckles,
            (int)ArcDPSEnums.TrashID.Karde,
        ];
    }

    internal override void ComputeNPCCombatReplayActors(NPC target, ParsedEvtcLog log, CombatReplay replay)
    {
        var cls = target.GetCastEvents(log, log.FightData.FightStart, log.FightData.FightEnd);
        switch (target.ID)
        {
            case (int)ArcDPSEnums.TargetID.Sabetha:
                var flameWall = cls.Where(x => x.SkillId == Firestorm);
                foreach (CastEvent c in flameWall)
                {
                    int start = (int)c.Time;
                    int preCastTime = 2800;
                    int duration = 10000;
                    uint width = 1300; uint height = 60;
                    if (target.TryGetCurrentFacingDirection(log, start, out var facing))
                    {
                        var positionConnector = (AgentConnector)new AgentConnector(target).WithOffset(new(width / 2, 0, 0), true);
                        replay.Decorations.Add(new RectangleDecoration(width, height, (start, start + preCastTime), Colors.Orange, 0.2, positionConnector).UsingRotationConnector(new AngleConnector(facing)));
                        replay.Decorations.Add(new RectangleDecoration(width, height, (start + preCastTime, start + preCastTime + duration), Colors.Red, 0.5, positionConnector).UsingRotationConnector(new AngleConnector(facing, 360)));
                    }
                }
                break;

            case (int)ArcDPSEnums.TrashID.Kernan:
                var bulletHail = cls.Where(x => x.SkillId == BulletHail);
                foreach (CastEvent c in bulletHail)
                {
                    int start = (int)c.Time;
                    int firstConeStart = start;
                    int secondConeStart = start + 800;
                    int thirdConeStart = start + 1600;
                    int firstConeEnd = firstConeStart + 400;
                    int secondConeEnd = secondConeStart + 400;
                    int thirdConeEnd = thirdConeStart + 400;
                    uint radius = 1500;
                    if (target.TryGetCurrentFacingDirection(log, start, out var facing))
                    {
                        var connector = new AgentConnector(target);
                        var rotationConnector = new AngleConnector(facing);
                        replay.Decorations.Add(new PieDecoration(radius, 28, (firstConeStart, firstConeEnd), Colors.Yellow, 0.3, connector).UsingRotationConnector(rotationConnector));
                        replay.Decorations.Add(new PieDecoration(radius, 54, (secondConeStart, secondConeEnd), Colors.Yellow, 0.3, connector).UsingRotationConnector(rotationConnector));
                        replay.Decorations.Add(new PieDecoration(radius, 81, (thirdConeStart, thirdConeEnd), Colors.Yellow, 0.3, connector).UsingRotationConnector(rotationConnector));
                    }
                }
                break;

            case (int)ArcDPSEnums.TrashID.Knuckles:
                var breakbar = cls.Where(x => x.SkillId == PlatformQuake);
                foreach (CastEvent c in breakbar)
                {
                    replay.Decorations.Add(new CircleDecoration(180, ((int)c.Time, (int)c.EndTime), Colors.LightBlue, 0.3, new AgentConnector(target)));
                }
                break;

            case (int)ArcDPSEnums.TrashID.Karde:
                var flameBlast = cls.Where(x => x.SkillId == FlameBlast);
                foreach (CastEvent c in flameBlast)
                {
                    int start = (int)c.Time;
                    int end = start + 4000;
                    uint radius = 600;
                    if (target.TryGetCurrentFacingDirection(log, start, out var facing))
                    {
                        replay.Decorations.Add(new PieDecoration(radius, 60, (start, end), Colors.Yellow, 0.5, new AgentConnector(target)).UsingRotationConnector(new AngleConnector(facing)));
                    }
                }
                break;

            default:
                break;
        }
    }

    protected override List<ArcDPSEnums.TrashID> GetTrashMobsIDs()
    {
        return
        [
            ArcDPSEnums.TrashID.BanditSapper,
            ArcDPSEnums.TrashID.BanditThug,
            ArcDPSEnums.TrashID.BanditArsonist
        ];
    }

    internal override void ComputePlayerCombatReplayActors(PlayerActor p, ParsedEvtcLog log, CombatReplay replay)
    {
        base.ComputePlayerCombatReplayActors(p, log, replay);
        // timed bombs
        var timedBombs = log.CombatData.GetBuffDataByIDByDst(TimeBomb, p.AgentItem).Where(x => x is BuffApplyEvent);
        foreach (BuffEvent c in timedBombs)
        {
            int start = (int)c.Time;
            int end = start + 3000;
            replay.Decorations.AddWithFilledWithGrowing(new CircleDecoration(280, (start, end), Colors.LightOrange, 0.5, new AgentConnector(p)).UsingFilled(false), true, end);
        }
        // Sapper bombs
        var sapperBombs = p.GetBuffStatus(log, SapperBombBuff, log.FightData.FightStart, log.FightData.FightEnd).Where(x => x.Value > 0);
        foreach (var seg in sapperBombs)
        {
            replay.Decorations.AddWithFilledWithGrowing(new CircleDecoration(180, seg, "rgba(200, 255, 100, 0.5)", new AgentConnector(p)).UsingFilled(false), true, seg.Start + 5000);
            replay.Decorations.AddOverheadIcon(seg, p, ParserIcons.BombOverhead);
        }
    }

    protected override ReadOnlySpan<int> GetUniqueNPCIDs()
    {
        return
        [
            (int)ArcDPSEnums.TargetID.Sabetha,
            (int)ArcDPSEnums.TrashID.Kernan,
            (int)ArcDPSEnums.TrashID.Karde,
            (int)ArcDPSEnums.TrashID.Knuckles,
        ];
    }

    private readonly IReadOnlyDictionary<int, string> PhaseNames = new Dictionary<int, string>()
    {
        { (int)ArcDPSEnums.TrashID.Kernan, "Kernan" },
        { (int)ArcDPSEnums.TrashID.Karde, "Karde" },
        { (int)ArcDPSEnums.TrashID.Knuckles, "Knuckles" }
    };
}
