﻿using System.Text.Json;
using System.Text.Json.Serialization;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.ParsedData;

namespace GW2EIBuilders.HtmlModels.HTMLMetaData;


/// <summary> A struct holding skill data that gets serialized as an array of numbers. </summary>
[JsonConverter(typeof(SkillCastDtoConverter))]
public struct SkillCastDto
{
    public double Start;
    public long   SkillId;
    public int    ActualDuration;
    public int    Status;
    public double Acceleration;
}

class SkillCastDtoConverter : JsonConverter<SkillCastDto>
{
    public override SkillCastDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, SkillCastDto value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Start);
        writer.WriteNumberValue(value.SkillId);
        writer.WriteNumberValue(value.ActualDuration);
        writer.WriteNumberValue(value.Status);
        writer.WriteNumberValue(value.Acceleration);
        writer.WriteEndArray();
    }
}



internal class SkillDto : AbstractSkillDto
{
    public bool Aa;
    public bool IsSwap;
    public bool NotAccurate;
    public bool TraitProc;
    public bool GearProc;

    public SkillDto(SkillItem skill, ParsedEvtcLog log) : base(skill, log)
    {
        Aa = skill.AA;
        IsSwap = skill.IsSwap;
        NotAccurate = log.SkillData.IsNotAccurate(skill.ID);
        GearProc = log.SkillData.IsGearProc(skill.ID);
        TraitProc = log.SkillData.IsTraitProc(skill.ID);
    }

    public static void AssembleSkills(ICollection<SkillItem> skills, Dictionary<string, SkillDto> dict, ParsedEvtcLog log)
    {
        dict.EnsureCapacity(dict.Count + skills.Count);

        foreach (SkillItem skill in skills)
        {
            dict["s" + skill.ID] = new SkillDto(skill, log);
        }
    }

    private static SkillCastDto GetSkillData(AbstractCastEvent cl, long phaseStart)
    {
        return new SkillCastDto()
        {
            Start = (cl.Time - phaseStart) / 1000.0,
            SkillId = cl.SkillId,
            ActualDuration = cl.ActualDuration,
            Status = (int)cl.Status,
            Acceleration = cl.Acceleration,
        };
    }

    public static List<SkillCastDto> BuildRotationData(ParsedEvtcLog log, AbstractSingleActor p, PhaseData phase, Dictionary<long, SkillItem> usedSkills)
    {
        var list = new List<SkillCastDto>(); //TODO(Rennorb) @perf
        var casting = p.GetIntersectingCastEvents(log, phase.Start, phase.End);
        foreach (AbstractCastEvent cl in casting)
        {
            if (!usedSkills.ContainsKey(cl.SkillId))
            {
                usedSkills.Add(cl.SkillId, cl.Skill);
            }

            list.Add(GetSkillData(cl, phase.Start));
        }
        return list;
    }
}
