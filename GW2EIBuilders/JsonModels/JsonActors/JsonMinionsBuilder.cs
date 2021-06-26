﻿using System;
using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using GW2EIJSON;
using Newtonsoft.Json;

namespace GW2EIBuilders.JsonModels
{
    /// <summary>
    /// Class corresponding to the regrouping of the same type of minions
    /// </summary>
    internal static class JsonMinionsBuilder
    {
        
        public static JsonMinions BuildJsonMinions(Minions minions, ParsedEvtcLog log, Dictionary<string, JsonLog.SkillDesc> skillDesc, Dictionary<string, JsonLog.BuffDesc> buffDesc)
        {
            var jsonMinions = new JsonMinions();
            IReadOnlyList<PhaseData> phases = log.FightData.GetNonDummyPhases(log);
            bool isEnemyMinion = !log.FriendlyAgents.Contains(minions.Master.AgentItem);
            //
            jsonMinions.Name = minions.Character;
            //
            var totalDamage = new List<int>();
            var totalShieldDamage = new List<int>();
            var totalBreakbarDamage = new List<double>();
            foreach (PhaseData phase in phases)
            {
                int tot = 0;
                int shdTot = 0;
                foreach (AbstractHealthDamageEvent de in minions.GetDamageEvents(null, log, phase.Start, phase.End))
                {
                    tot += de.HealthDamage;
                    shdTot = de.ShieldDamage;
                }
                totalDamage.Add(tot);
                totalShieldDamage.Add(shdTot);
                totalBreakbarDamage.Add(Math.Round(minions.GetBreakbarDamageEvents(null, log, phase.Start, phase.End).Sum(x => x.BreakbarDamage), 1));
            }
            jsonMinions.TotalDamage = totalDamage;
            jsonMinions.TotalShieldDamage = totalShieldDamage;
            jsonMinions.TotalBreakbarDamage = totalBreakbarDamage;
            if (!isEnemyMinion)
            {
                var totalTargetDamage = new IReadOnlyList<int>[log.FightData.Logic.Targets.Count];
                var totalTargetShieldDamage = new IReadOnlyList<int>[log.FightData.Logic.Targets.Count];
                var totalTargetBreakbarDamage = new IReadOnlyList<double>[log.FightData.Logic.Targets.Count];
                for (int i = 0; i < log.FightData.Logic.Targets.Count; i++)
                {
                    AbstractSingleActor tar = log.FightData.Logic.Targets[i];
                    var totalTarDamage = new List<int>();
                    var totalTarShieldDamage = new List<int>();
                    var totalTarBreakbarDamage = new List<double>();
                    foreach (PhaseData phase in phases)
                    {
                        int tot = 0;
                        int shdTot = 0;
                        foreach (AbstractHealthDamageEvent de in minions.GetDamageEvents(tar, log, phase.Start, phase.End))
                        {
                            tot += de.HealthDamage;
                            shdTot = de.ShieldDamage;
                        }
                        totalTarDamage.Add(tot);
                        totalTarShieldDamage.Add(shdTot);
                        totalTarBreakbarDamage.Add(Math.Round(minions.GetBreakbarDamageEvents(tar, log, phase.Start, phase.End).Sum(x => x.BreakbarDamage), 1));
                    }
                    totalTargetDamage[i] = totalTarDamage;
                    totalTargetShieldDamage[i] = totalTarShieldDamage;
                    totalTargetBreakbarDamage[i] = totalTarBreakbarDamage;
                }
                jsonMinions.TotalTargetShieldDamage = totalTargetShieldDamage;
                jsonMinions.TotalTargetDamage = totalTargetDamage;
                jsonMinions.TotalTargetBreakbarDamage = totalTargetBreakbarDamage;
            }
            //
            var skillByID = minions.GetIntersectingCastEvents(log, 0, log.FightData.FightEnd).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList());
            if (skillByID.Any())
            {
                jsonMinions.Rotation = JsonRotationBuilder.BuildJsonRotationList(log, skillByID, skillDesc);
            }
            //
            var totalDamageDist = new IReadOnlyList<JsonDamageDist>[phases.Count];
            for (int i = 0; i < phases.Count; i++)
            {
                PhaseData phase = phases[i];
                totalDamageDist[i] = JsonDamageDistBuilder.BuildJsonDamageDistList(minions.GetDamageEvents(null, log, phase.Start, phase.End).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList()), log, skillDesc, buffDesc);
            }
            jsonMinions.TotalDamageDist = totalDamageDist;
            if (!isEnemyMinion)
            {
                var targetDamageDist = new IReadOnlyList<JsonDamageDist>[log.FightData.Logic.Targets.Count][];
                for (int i = 0; i < log.FightData.Logic.Targets.Count; i++)
                {
                    AbstractSingleActor target = log.FightData.Logic.Targets[i];
                    targetDamageDist[i] = new IReadOnlyList<JsonDamageDist>[phases.Count];
                    for (int j = 0; j < phases.Count; j++)
                    {
                        PhaseData phase = phases[j];
                        targetDamageDist[i][j] = JsonDamageDistBuilder.BuildJsonDamageDistList(minions.GetDamageEvents(target, log, phase.Start, phase.End).GroupBy(x => x.SkillId).ToDictionary(x => x.Key, x => x.ToList()), log, skillDesc, buffDesc);
                    }
                }
                jsonMinions.TargetDamageDist = targetDamageDist;
            }
            return jsonMinions;
        }

    }
}