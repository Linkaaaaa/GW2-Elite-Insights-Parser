namespace GW2EIEvtcParser.ParsedData;

public class GadgetAnimationEvent : TimeCombatEvent
{
    public readonly Token AnimationToken;

    public readonly AgentItem Gadget;

    public GadgetAnimationEvent Next { get; private set; }

    public long? LoopEnd => Next?.Time;

    internal GadgetAnimationEvent(CombatItem evtcItem, AgentData agentData) : base(evtcItem.Time)
    {
        AnimationToken = GetAnimationToken(evtcItem);
        Gadget = agentData.GetAgent(evtcItem.SrcAgent, evtcItem.Time);
    }

    internal static Token GetAnimationToken(CombatItem evtcItem)
    {
        return new Token(evtcItem.DstAgent);
    }

    internal void SetNext(GadgetAnimationEvent next)
    {
        Next = next;
    }
}
