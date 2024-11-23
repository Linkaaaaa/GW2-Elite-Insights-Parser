﻿using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIBuilders.HtmlModels.HTMLCharts;

internal class MechanicChartDataDto
{
    public readonly string Symbol;
    public readonly int Size;
    public readonly string Color;
    public readonly List<List<List<(double time, string? name)>>> Points;
    public readonly bool Visible;

    private MechanicChartDataDto(ParsedEvtcLog log, Mechanic mech)
    {
        Color = mech.PlotlySetting.Color;
        Symbol = mech.PlotlySetting.Symbol;
        Size = mech.PlotlySetting.Size;
        Visible = !mech.ShowOnTable;
        Points = BuildMechanicGraphPointData(log, log.MechanicData.GetMechanicLogs(log, mech, log.FightData.FightStart, log.FightData.FightEnd), mech.IsEnemyMechanic);
    }

    private static List<List<(double time, string? name)>> GetMechanicChartPoints(IReadOnlyList<MechanicEvent> mechanicLogs, PhaseData phase, ParsedEvtcLog log, bool enemyMechanic)
    {
        var res = new List<List<(double time, string? name)>>();
        if (!enemyMechanic)
        {
            var playerIndex = new Dictionary<SingleActor, int>(log.Friendlies.Count);
            for (int p = 0; p < log.Friendlies.Count; p++)
            {
                playerIndex.Add(log.Friendlies[p], p);
                res.Add([]);
            }
            foreach (MechanicEvent ml in mechanicLogs.Where(x => phase.InInterval(x.Time)))
            {
                double time = (ml.Time - phase.Start) / 1000.0;
                if (playerIndex.TryGetValue(ml.Actor, out int p))
                {
                    res[p].Add((time, null));
                }
            }
        }
        else
        {
            var targetIndex = new Dictionary<SingleActor, int>(phase.AllTargets.Count);
            for (int p = 0; p < phase.AllTargets.Count; p++)
            {
                targetIndex.Add(phase.AllTargets[p], p);
                res.Add([]);
            }
            res.Add([]);
            foreach (MechanicEvent ml in mechanicLogs.Where(x => phase.InInterval(x.Time)))
            {
                double time = (ml.Time - phase.Start) / 1000.0;
                if (targetIndex.TryGetValue(ml.Actor, out int p))
                {
                    res[p].Add((time, null));
                }
                else
                {
                    res[^1].Add((time, ml.Actor.Character ));
                }
            }
        }
        return res;
    }

    private static List<List<List<(double time, string? name)>>> BuildMechanicGraphPointData(ParsedEvtcLog log, IReadOnlyList<MechanicEvent> mechanicLogs, bool enemyMechanic)
    {
        var phases = log.FightData.GetPhases(log);
        var list = new List<List<List<(double time, string? name)>>>(phases.Count);
        foreach (PhaseData phase in phases)
        {
            list.Add(GetMechanicChartPoints(mechanicLogs, phase, log, enemyMechanic));
        }
        return list;
    }

    public static List<MechanicChartDataDto> BuildMechanicsChartData(ParsedEvtcLog log)
    {
        var mechs = log.MechanicData.GetPresentMechanics(log, log.FightData.FightStart, log.FightData.FightEnd);
        var mechanicsChart = new List<MechanicChartDataDto>(mechs.Count);
        foreach (Mechanic mech in mechs)
        {
            mechanicsChart.Add(new MechanicChartDataDto(log, mech));
        }
        return mechanicsChart;
    }
}
