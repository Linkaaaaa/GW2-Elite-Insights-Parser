using static GW2EIEvtcParser.ArcDPSEnums;

namespace GW2EIEvtcParser.ParsedData;

public class DirectHealthDamageEvent : HealthDamageEvent
{
    internal DirectHealthDamageEvent(CombatItem evtcItem, AgentData agentData, SkillData skillData, DamageResult result) : base(evtcItem, agentData, skillData)
    {
        HealthDamage = evtcItem.Value;
        AgainstDowned = evtcItem.IsOffcycle == 1;
        IsAbsorbed = result == DamageResult.Absorb || result == DamageResult.Invert;
        IsBlind = result == DamageResult.Blind;
        IsBlocked = result == DamageResult.Block;
        HasCrit = result == DamageResult.Crit;
        IsEvaded = result == DamageResult.Evade;
        HasGlanced = result == DamageResult.Glance;
        ShieldDamage = evtcItem.IsShields > 0 ? (int)evtcItem.OverstackValue : 0;
        HasHit = result == DamageResult.Normal || HasGlanced || HasCrit; 
    }

    internal override void MakeIntoAbsorbed()
    {
        HasHit = false;
        HasCrit = false;
        HasGlanced = false;
        IsBlind = false;
        IsBlocked = false;
        IsEvaded = false;
        IsAbsorbed = true;

        HealthDamage = 0;
        ShieldDamage = 0;
    }
}
