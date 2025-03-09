using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser;

public static class ParserHelper
{
    internal delegate bool ExtraRedirection(CombatItem evt, AgentItem from, AgentItem to);
    internal static readonly AgentItem _unknownAgent = new();

    public const int CombatReplayPollingRate = 300;
    internal const uint CombatReplaySkillDefaultSizeInPixel = 22;
    internal const uint CombatReplaySkillDefaultSizeInWorld = 90;
    internal const uint CombatReplayOverheadDefaultSizeInPixel = 20;
    internal const uint CombatReplayOverheadProgressBarMinorSizeInPixel = 20;
    internal const uint CombatReplayOverheadProgressBarMajorSizeInPixel = 35;
    internal const float CombatReplayOverheadDefaultOpacity = 0.8f;

    public const int MinionLimit = 1500;

    //TODO(Rennorb) @cleanup: Rename this whole block. These are rounding precisions, maybe 'digits' if im being generous.
    internal const int BuffDigit = 3;
    internal const int DamageModGainDigit = 3;
    internal const int AccelerationDigit = 3;
    internal const int CombatReplayDataDigit = 3;
    public const int TimeDigit = 3;

    public const long ServerDelayConstant = 10;
    internal const long BuffSimulatorDelayConstant = 15;
    internal const long BuffSimulatorStackActiveDelayConstant = 50;
    internal const long WeaponSwapDelayConstant = 75;
    internal const long TimeThresholdConstant = 150;

    internal const long InchDistanceThreshold = 10;

    public const long MinimumInCombatDuration = 2200;

    internal const int PhaseTimeLimit = 1000;


    public enum Source
    {
        Common,
        Item, Gear,
        Necromancer, Reaper, Scourge, Harbinger,
        Elementalist, Tempest, Weaver, Catalyst,
        Mesmer, Chronomancer, Mirage, Virtuoso,
        Warrior, Berserker, Spellbreaker, Bladesworn,
        Revenant, Herald, Renegade, Vindicator,
        Guardian, Dragonhunter, Firebrand, Willbender,
        Thief, Daredevil, Deadeye, Specter,
        Ranger, Druid, Soulbeast, Untamed,
        Engineer, Scrapper, Holosmith, Mechanist,
        PetSpecific,
        FightSpecific,
        FractalInstability,
        Unknown
    };

    public enum Spec
    {
        Necromancer, Reaper, Scourge, Harbinger,
        Elementalist, Tempest, Weaver, Catalyst,
        Mesmer, Chronomancer, Mirage, Virtuoso,
        Warrior, Berserker, Spellbreaker, Bladesworn,
        Revenant, Herald, Renegade, Vindicator,
        Guardian, Dragonhunter, Firebrand, Willbender,
        Thief, Daredevil, Deadeye, Specter,
        Ranger, Druid, Soulbeast, Untamed,
        Engineer, Scrapper, Holosmith, Mechanist,
        NPC, Gadget,
        Unknown
    };

    public enum BuffEnum { Self, Group, OffGroup, Squad };

    [Flags]
    public enum DamageType
    {
        All = 0,
        Power = 1 << 0,
        Strike = 1 << 1,
        Condition = 1 << 2,
        StrikeAndCondition = Strike | Condition, // Common
        LifeLeech = 1 << 3,
        StrikeAndConditionAndLifeLeech = Strike | Condition | LifeLeech, // Common
    };
    public static string DamageTypeToString(this DamageType damageType)
    {
        if (damageType == DamageType.All)
        {
            return "All Damage";
        }
        string str = "";
        bool addComa = false;
        if ((damageType & DamageType.Power) > 0)
        {
            if (addComa)
            {
                str += ", ";
            }
            str += "Power";
            addComa = true;
        }
        if ((damageType & DamageType.Strike) > 0)
        {
            if (addComa)
            {
                str += ", ";
            }
            str += "Strike";
            addComa = true;
        }
        if ((damageType & DamageType.Condition) > 0)
        {
            if (addComa)
            {
                str += ", ";
            }
            str += "Condition";
            addComa = true;
        }
        if ((damageType & DamageType.LifeLeech) > 0)
        {
            if (addComa)
            {
                str += ", ";
            }
            str += "Life Leech";
            addComa = true;
        }
        str += " Damage";
        return str;
    }

    internal static Dictionary<long, List<T>> GroupByTime<T>(IEnumerable<T> list) where T : TimeCombatEvent
    {
        var groupByTime = new Dictionary<long, List<T>>();
        foreach (T c in list)
        {
            long key = groupByTime.Keys.FirstOrDefault(x => Math.Abs(x - c.Time) < ServerDelayConstant);
            if (key != 0)
            {
                groupByTime[key].Add(c);
            }
            else
            {
                groupByTime[c.Time] =
                        [
                            c
                        ];
            }
        }
        return groupByTime;
    }

    internal static T MaxBy<T, TComparable>(this IEnumerable<T> en, Func<T, TComparable> evaluate) where TComparable : IComparable<TComparable>
    {
        return en.Select(t => (value: t, eval: evaluate(t)))
            .Aggregate((max, next) => next.eval.CompareTo(max.eval) > 0 ? next : max).value;
    }

    internal static T MinBy<T, TComparable>(this IEnumerable<T> en, Func<T, TComparable> evaluate) where TComparable : IComparable<TComparable>
    {
        return en.Select(t => (value: t, eval: evaluate(t)))
            .Aggregate((max, next) => next.eval.CompareTo(max.eval) < 0 ? next : max).value;
    }

    internal static bool IsSupportedStateChange(StateChange state)
    {
        return state != StateChange.Unknown && state != StateChange.ReplInfo && state != StateChange.StatReset && state != StateChange.APIDelayed && state != StateChange.Idle;
    }

    /*
    public static string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }


    public static string FindPattern(string source, string regex)
    {
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        Match match = Regex.Match(source, regex);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
    */
    public static Exception GetFinalException(Exception ex)
    {
        Exception final = ex;
        while (final.InnerException != null)
        {
            final = final.InnerException;
        }
        return final;
    }

    /// <summary>
    /// Method used to redirect a subset of events from redirectFrom to to
    /// </summary>
    /// <param name="combatData"></param>
    /// <param name="extensions"></param>
    /// <param name="agentData"></param>
    /// <param name="redirectFrom">AgentItem the events need to be redirected from</param>
    /// <param name="stateCopyFroms">AgentItems from where last known states (hp, position, etc) will be copied from</param>
    /// <param name="to">AgentItem the events need to be redirected to</param>
    /// <param name="copyPositionalDataFromAttackTarget">If true, "to" will get the positional data from attack targets, if possible</param>
    /// <param name="extraRedirections">function to handle special conditions, given event either src or dst matches from</param>
    internal static void RedirectEventsAndCopyPreviousStates(List<CombatItem> combatData, IReadOnlyDictionary<uint, ExtensionHandler> extensions, AgentData agentData, AgentItem redirectFrom, List<AgentItem> stateCopyFroms, AgentItem to, bool copyPositionalDataFromAttackTarget, ExtraRedirection? extraRedirections = null)
    {
        // Redirect combat events
        foreach (CombatItem evt in combatData)
        {
            if (to.InAwareTimes(evt.Time))
            {
                var srcMatchesAgent = evt.SrcMatchesAgent(redirectFrom, extensions);
                var dstMatchesAgent = evt.DstMatchesAgent(redirectFrom, extensions);
                if (extraRedirections != null && !extraRedirections(evt, redirectFrom, to))
                {
                    continue;
                }
                if (srcMatchesAgent)
                {
                    evt.OverrideSrcAgent(to.Agent);
                }
                if (dstMatchesAgent)
                {
                    evt.OverrideDstAgent(to.Agent);
                }
            }
        }
        // Copy attack targets
        var attackTargetAgents = new HashSet<AgentItem>();
        var attackTargets = combatData.Where(x => x.IsStateChange == StateChange.AttackTarget && x.DstMatchesAgent(redirectFrom)).ToList() ;
        var targetableOns = combatData.Where(x => x.IsStateChange == StateChange.Targetable && x.DstAgent == 1);
        foreach (CombatItem c in attackTargets)
        {
            var cExtra = new CombatItem(c);
            cExtra.OverrideTime(to.FirstAware);
            cExtra.OverrideDstAgent(to.Agent);
            combatData.Add(cExtra);
            AgentItem at = agentData.GetAgent(c.SrcAgent, c.Time);
            if (targetableOns.Any(x => x.SrcMatchesAgent(at)))
            {
                attackTargetAgents.Add(at);
            }
        }
        // Copy states
        var toCopy = new List<CombatItem>();
        Func<CombatItem, bool> canCopyFromAgent = (evt) => stateCopyFroms.Any(x => evt.SrcMatchesAgent(x));
        var stateChangeCopyFromAgentConditions = new List<Func<CombatItem, bool>>()
        {
            (x) => x.IsStateChange == StateChange.BreakbarState,
            (x) => x.IsStateChange == StateChange.MaxHealthUpdate,
            (x) => x.IsStateChange == StateChange.HealthUpdate,
            (x) => x.IsStateChange == StateChange.BreakbarPercent,
            (x) => x.IsStateChange == StateChange.BarrierUpdate,
            (x) => (x.IsStateChange == StateChange.EnterCombat || x.IsStateChange == StateChange.ExitCombat),
            (x) => (x.IsStateChange == StateChange.Spawn || x.IsStateChange == StateChange.Despawn || x.IsStateChange == StateChange.ChangeDead || x.IsStateChange == StateChange.ChangeDown || x.IsStateChange == StateChange.ChangeUp),
        };
        if (!copyPositionalDataFromAttackTarget || attackTargetAgents.Count == 0)
        {
            stateChangeCopyFromAgentConditions.Add((x) => x.IsStateChange == StateChange.Position);
            stateChangeCopyFromAgentConditions.Add((x) => x.IsStateChange == StateChange.Rotation);
            stateChangeCopyFromAgentConditions.Add((x) => x.IsStateChange == StateChange.Velocity);
        }
        foreach (Func<CombatItem, bool> stateChangeCopyCondition in stateChangeCopyFromAgentConditions)
        {
            CombatItem? stateToCopy = combatData.LastOrDefault(x => stateChangeCopyCondition(x) && canCopyFromAgent(x) && x.Time <= to.FirstAware);
            if (stateToCopy != null)
            {
                toCopy.Add(stateToCopy);
            }
        }
        // Copy positional data from attack targets
        if (copyPositionalDataFromAttackTarget && attackTargetAgents.Count != 0)
        {
            Func<CombatItem, bool> canCopyFromAttackTarget = (evt) => attackTargetAgents.Any(x => evt.SrcMatchesAgent(x));
            var stateChangeCopyFromAttackTargetConditions = new List<Func<CombatItem, bool>>()
            {
                (x) => x.IsStateChange == StateChange.Position,
                (x) => x.IsStateChange == StateChange.Rotation,
                (x) => x.IsStateChange == StateChange.Velocity,
            };
            foreach (Func<CombatItem, bool> stateChangeCopyCondition in stateChangeCopyFromAttackTargetConditions)
            {
                CombatItem? stateToCopy = combatData.LastOrDefault(x => stateChangeCopyCondition(x) && canCopyFromAttackTarget(x) && x.Time <= to.FirstAware);
                if (stateToCopy != null)
                {
                    toCopy.Add(stateToCopy);
                }
            }
        }
        foreach (CombatItem c in toCopy)
        {
            var cExtra = new CombatItem(c);
            cExtra.OverrideTime(to.FirstAware);
            cExtra.OverrideSrcAgent(to.Agent);
            combatData.Add(cExtra);
        }
        // Redirect NPC masters
        foreach (AgentItem ag in agentData.GetAgentByType(AgentItem.AgentType.NPC))
        {
            if (ag.Master == redirectFrom && to.InAwareTimes(ag.FirstAware))
            {
                ag.SetMaster(to);
            }
        }
        // Redirect Gadget masters
        foreach (AgentItem ag in agentData.GetAgentByType(AgentItem.AgentType.Gadget))
        {
            if (ag.Master == redirectFrom && to.InAwareTimes(ag.FirstAware))
            {
                ag.SetMaster(to);
            }
        }
    }

    /// <summary>
    /// Method used to redirect all events from redirectFrom to to
    /// </summary>
    /// <param name="combatData"></param>
    /// <param name="extensions"></param>
    /// <param name="agentData"></param>
    /// <param name="redirectFrom">AgentItem the events need to be redirected from</param>
    /// <param name="to">AgentItem the events need to be redirected to</param>
    /// <param name="extraRedirections">function to handle special conditions, given event either src or dst matches from</param>
    internal static void RedirectAllEvents(IReadOnlyList<CombatItem> combatData, IReadOnlyDictionary<uint, ExtensionHandler> extensions, AgentData agentData, AgentItem redirectFrom, AgentItem to, ExtraRedirection? extraRedirections = null)
    {
        // Redirect combat events
        foreach (CombatItem evt in combatData)
        {
            var srcMatchesAgent = evt.SrcMatchesAgent(redirectFrom, extensions);
            var dstMatchesAgent = evt.DstMatchesAgent(redirectFrom, extensions);
            if (!dstMatchesAgent && !srcMatchesAgent)
            {
                continue;
            }
            if (extraRedirections != null && !extraRedirections(evt, redirectFrom, to))
            {
                continue;
            }
            if (srcMatchesAgent)
            {
                evt.OverrideSrcAgent(to.Agent);
            }
            if (dstMatchesAgent)
            {
                evt.OverrideDstAgent(to.Agent);
            }
        }
        agentData.SwapMasters(redirectFrom, to);
    }

    public static IReadOnlyDictionary<BuffAttribute, string> BuffAttributesStrings { get; private set; } = new Dictionary<BuffAttribute, string>()
    {
        { BuffAttribute.Power, "Power" },
        { BuffAttribute.Precision, "Precision" },
        { BuffAttribute.Toughness, "Toughness" },
        { BuffAttribute.DefensePercent, "Defense" },
        { BuffAttribute.Vitality, "Vitality" },
        { BuffAttribute.VitalityPercent, "Vitality" },
        { BuffAttribute.Ferocity, "Ferocity" },
        { BuffAttribute.Healing, "Healing Power" },
        { BuffAttribute.Condition, "Condition Damage" },
        { BuffAttribute.Concentration, "Concentration" },
        { BuffAttribute.Expertise, "Expertise" },
        { BuffAttribute.AllStatsPercent, "All Stats" },
        { BuffAttribute.FishingPower, "Fishing Power" },
        { BuffAttribute.Armor, "Armor" },
        { BuffAttribute.Agony, "Agony" },
        { BuffAttribute.StatOutgoing, "Stat Increase" },
        { BuffAttribute.FlatOutgoing, "Flat Increase" },
        { BuffAttribute.PhysOutgoing, "Outgoing Strike Damage" },
        { BuffAttribute.CondOutgoing, "Outgoing Condition Damage" },
        { BuffAttribute.SiphonOutgoing, "Outgoing Life Leech Damage" },
        { BuffAttribute.SiphonIncomingAdditive1, "Incoming Life Leech Damage" },
        { BuffAttribute.SiphonIncomingAdditive2, "Incoming Life Leech Damage" },
        { BuffAttribute.CondIncomingAdditive, "Incoming Condition Damage" },
        { BuffAttribute.CondIncomingMultiplicative, "Incoming Condition Damage (Mult)" },
        { BuffAttribute.PhysIncomingAdditive, "Incoming Strike Damage" },
        { BuffAttribute.PhysIncomingMultiplicative, "Incoming Strike Damage (Mult)" },
        { BuffAttribute.AttackSpeed, "Attack Speed" },
        { BuffAttribute.ConditionDurationOutgoing, "Outgoing Condition Duration" },
        { BuffAttribute.BoonDurationOutgoing, "Outgoing Boon Duration" },
        { BuffAttribute.DamageFormulaSquaredLevel, "Damage Formula" },
        { BuffAttribute.DamageFormula, "Damage Formula" },
        { BuffAttribute.GlancingBlow, "Glancing Blow" },
        { BuffAttribute.CriticalChance, "Critical Chance" },
        { BuffAttribute.StrikeDamageToHP, "Strike Damage to Health" },
        { BuffAttribute.ConditionDamageToHP, "Condition Damage to Health" },
        { BuffAttribute.SkillActivationDamageFormula, "Damage Formula on Skill Activation" },
        { BuffAttribute.MovementActivationDamageFormula, "Damage Formula based on Movement" },
        { BuffAttribute.EnduranceRegeneration, "Endurance Regeneration" },
        { BuffAttribute.HealingEffectivenessIncomingNonStacking, "Incoming Healing Effectiveness" },
        { BuffAttribute.HealingEffectivenessIncomingAdditive, "Incoming Healing Effectiveness" },
        { BuffAttribute.HealingEffectivenessIncomingMultiplicative, "Incoming Healing Effectiveness (Mult)" },
        { BuffAttribute.HealingEffectivenessConvOutgoing, "Outgoing Healing Effectiveness" },
        { BuffAttribute.HealingEffectivenessOutgoingAdditive, "Outgoing Healing Effectiveness" },
        { BuffAttribute.HealingOutputFormula, "Healing Formula" },
        { BuffAttribute.ExperienceFromKills, "Experience From Kills" },
        { BuffAttribute.ExperienceFromAll, "Experience From All" },
        { BuffAttribute.GoldFind, "GoldFind" },
        { BuffAttribute.MovementSpeed, "Movement Speed" },
        { BuffAttribute.MovementSpeedStacking, "Movement Speed (Stacking)" },
        { BuffAttribute.MovementSpeedStacking2, "Movement Speed (Stacking)" },
        { BuffAttribute.MaximumHP, "Maximum Health" },
        { BuffAttribute.KarmaBonus, "Karma Bonus" },
        { BuffAttribute.SkillRechargeSpeedIncrease, "Skill Recharge Speed Increase" },
        { BuffAttribute.MagicFind, "Magic Find" },
        { BuffAttribute.WXP, "WXP" },
        { BuffAttribute.Unknown, "Unknown" },
    };

    public static IReadOnlyDictionary<BuffAttribute, string> BuffAttributesPercent { get; private set; } = new Dictionary<BuffAttribute, string>()
    {
        { BuffAttribute.FlatOutgoing, "%" },
        { BuffAttribute.PhysOutgoing, "%" },
        { BuffAttribute.CondOutgoing, "%" },
        { BuffAttribute.CondIncomingAdditive, "%" },
        { BuffAttribute.CondIncomingMultiplicative, "%" },
        { BuffAttribute.PhysIncomingAdditive, "%" },
        { BuffAttribute.PhysIncomingMultiplicative, "%" },
        { BuffAttribute.AttackSpeed, "%" },
        { BuffAttribute.ConditionDurationOutgoing, "%" },
        { BuffAttribute.BoonDurationOutgoing, "%" },
        { BuffAttribute.GlancingBlow, "%" },
        { BuffAttribute.CriticalChance, "%" },
        { BuffAttribute.StrikeDamageToHP, "%" },
        { BuffAttribute.ConditionDamageToHP, "%" },
        { BuffAttribute.EnduranceRegeneration, "%" },
        { BuffAttribute.HealingEffectivenessIncomingNonStacking, "%" },
        { BuffAttribute.HealingEffectivenessIncomingAdditive, "%" },
        { BuffAttribute.HealingEffectivenessIncomingMultiplicative, "%" },
        { BuffAttribute.SiphonOutgoing, "%" },
        { BuffAttribute.SiphonIncomingAdditive1, "%" },
        { BuffAttribute.SiphonIncomingAdditive2, "%" },
        { BuffAttribute.HealingEffectivenessConvOutgoing , "%" },
        { BuffAttribute.HealingEffectivenessOutgoingAdditive , "%" },
        { BuffAttribute.ExperienceFromKills, "%" },
        { BuffAttribute.ExperienceFromAll, "%" },
        { BuffAttribute.GoldFind, "%" },
        { BuffAttribute.MovementSpeed, "%" },
        { BuffAttribute.MovementSpeedStacking, "%" },
        { BuffAttribute.MovementSpeedStacking2, "%" },
        { BuffAttribute.MaximumHP, "%" },
        { BuffAttribute.KarmaBonus, "%" },
        { BuffAttribute.SkillRechargeSpeedIncrease, "%" },
        { BuffAttribute.MagicFind, "%" },
        { BuffAttribute.WXP, "%" },
        { BuffAttribute.DefensePercent, "%" },
        { BuffAttribute.VitalityPercent, "%" },
        { BuffAttribute.AllStatsPercent, "%" },
        { BuffAttribute.MovementActivationDamageFormula, " adds" },
        { BuffAttribute.SkillActivationDamageFormula, " replaces" },
        { BuffAttribute.Unknown, "Unknown" },
    };
    
    public static void Add<TKey, TValue>(Dictionary<TKey, List<TValue>> dict, TKey key, TValue evt)
    {
        if (dict.TryGetValue(key, out List<TValue>? list))
        {
            list.Add(evt);
        }
        else
        {
            dict[key] =
            [
                evt
            ];
        }
    }
    public static void Add<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> dict, TKey key, TValue evt)
    {
        if (dict.TryGetValue(key, out HashSet<TValue>? list))
        {
            list.Add(evt);
        }
        else
        {
            dict[key] =
            [
                evt
            ];
        }
    }

    public static int IndexOf<T>(this IReadOnlyList<T> self, T elementToFind)
    {
        int i = 0;
        foreach (T element in self)
        {
            if (Equals(element, elementToFind))
            {
                return i;
            }

            i++;
        }
        return -1;
    }
}
