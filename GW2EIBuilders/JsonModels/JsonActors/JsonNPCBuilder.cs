﻿using System.Collections.Generic;
using System.Linq;
using GW2EIBuilders.JsonModels.JsonActorUtilities;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;
using GW2EIJSON;
using static GW2EIJSON.JsonBuffsUptime;

namespace GW2EIBuilders.JsonModels.JsonActors
{
    /// <summary>
    /// Class representing an NPC
    /// </summary>
    internal static class JsonNPCBuilder
    {

        public static JsonNPC BuildJsonNPC(AbstractSingleActor npc, ParsedEvtcLog log, RawFormatSettings settings, Dictionary<long, SkillItem> skillMap, Dictionary<long, Buff> buffMap)
        {
            var jsonNPC = new JsonNPC();
            JsonActorBuilder.FillJsonActor(jsonNPC, npc, log, settings, skillMap, buffMap);
            IReadOnlyList<PhaseData> phases = log.FightData.GetPhases(log);
            //
            jsonNPC.Id = npc.ID;
            IReadOnlyList<HealthUpdateEvent> hpUpdates = log.CombatData.GetHealthUpdateEvents(npc.AgentItem);
            IReadOnlyList<BarrierUpdateEvent> barrierUpdates = log.CombatData.GetBarrierUpdateEvents(npc.AgentItem);
            jsonNPC.FirstAware = (int)npc.FirstAware;
            jsonNPC.LastAware = (int)npc.LastAware;
            jsonNPC.EnemyPlayer = npc is PlayerNonSquad;
            double hpLeft = 100.0;
            double barrierLeft = 0.0;
            if (log.FightData.Success)
            {
                hpLeft = 0;
            }
            else
            {
                if (hpUpdates.Count > 0)
                {
                    hpLeft = hpUpdates.Last().HealthPercent;
                }
                if (barrierUpdates.Count > 0)
                {
                    barrierLeft = barrierUpdates.Last().BarrierPercent;
                }
            }
            jsonNPC.HealthPercentBurned = 100.0 - hpLeft;
            jsonNPC.BarrierPercent = barrierLeft;
            jsonNPC.FinalHealth = npc.GetCurrentHealth(log, hpLeft);
            jsonNPC.FinalBarrier = npc.GetCurrentBarrier(log, barrierLeft, log.FightData.FightEnd);
            //
            jsonNPC.Buffs = GetNPCJsonBuffsUptime(npc, log, settings, buffMap);
            // Breakbar
            if (settings.RawFormatTimelineArrays)
            {
                jsonNPC.BreakbarPercents = npc.GetBreakbarPercentUpdates(log).Select(x => new double[2] { x.Start, x.Value }).ToList();
            }
            return jsonNPC;
        }

        private static List<JsonBuffsUptime> GetNPCJsonBuffsUptime(AbstractSingleActor npc, ParsedEvtcLog log, RawFormatSettings settings, Dictionary<long, Buff> buffMap)
        {
            var res = new List<JsonBuffsUptime>();
            IReadOnlyList<PhaseData> phases = log.FightData.GetPhases(log);
            var buffs = phases.Select(x => npc.GetBuffs(ParserHelper.BuffEnum.Self, log, x.Start, x.End)).ToList();
            var buffDictionaries = phases.Select(x => npc.GetBuffsDictionary(log, x.Start, x.End)).ToList();
            foreach (KeyValuePair<long, FinalActorBuffs> pair in buffs[0])
            {
                Buff buff = log.Buffs.BuffsByIds[pair.Key];
                if (buff.Classification == Buff.BuffClassification.Hidden)
                {
                    continue;
                }
                var data = new List<JsonBuffsUptimeData>();
                for (int i = 0; i < phases.Count; i++)
                {
                    if (buffs[i].TryGetValue(pair.Key, out FinalActorBuffs val))
                    {
                        JsonBuffsUptimeData value = JsonBuffsUptimeBuilder.BuildJsonBuffsUptimeData(val, buffDictionaries[i][pair.Key]);
                        data.Add(value);
                    }
                    else
                    {
                        var value = new JsonBuffsUptimeData();
                        data.Add(value);
                    }
                }
                res.Add(JsonBuffsUptimeBuilder.BuildJsonBuffsUptime(npc, pair.Key, log, settings, data, buffMap));
            }
            return res;
        }
    }
}
