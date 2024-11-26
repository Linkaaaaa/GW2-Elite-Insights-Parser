﻿using System.Numerics;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EIData.Decoration;

namespace GW2EIEvtcParser.EIData;

internal class CombatReplayDecorationContainer
{

    internal delegate GeographicalConnector? CustomConnectorBuilder(ParsedEvtcLog log, AgentItem agent, long start, long end);

    private readonly Dictionary<string, _DecorationMetadata> DecorationCache;
    private readonly List<(_DecorationMetadata metadata, _DecorationRenderingData renderingData)> Decorations;

    internal CombatReplayDecorationContainer(Dictionary<string, _DecorationMetadata> cache, int capacity = 0)
    {
        DecorationCache = cache;
        Decorations = new(capacity);
    }

    public void Add(Decoration decoration)
    {
        if (decoration.Lifespan.end <= decoration.Lifespan.start)
        {
            return;
        }

        _DecorationMetadata constantPart = decoration.DecorationMetadata;
        var id = constantPart.GetSignature();
        if (!DecorationCache.TryGetValue(id, out var cachedMetadata))
        {
            cachedMetadata = constantPart;
            DecorationCache[id] = constantPart;
        }
        Decorations.Add((cachedMetadata, decoration.DecorationRenderingData));
    }

    public void ReserveAdditionalCapacity(int additionalCapacity)
    {
        if(Decorations.Capacity >= Decorations.Count + additionalCapacity) { return; }

        Decorations.Capacity = (int)(Decorations.Capacity * 1.4f);
    }

    public List<DecorationRenderingDescription> GetCombatReplayRenderableDescriptions(CombatReplayMap map, ParsedEvtcLog log, Dictionary<long, SkillItem> usedSkills, Dictionary<long, Buff> usedBuffs)
    {
        var result = new List<DecorationRenderingDescription>(Decorations.Count);
        foreach (var (constant, renderingData) in Decorations)
        {
            result.Add(renderingData.GetCombatReplayRenderingDescription(map, log, usedSkills, usedBuffs, constant.GetSignature()));
        }
        return result;
    }

    /// <summary>
    /// Add an overhead icon decoration
    /// </summary>
    /// <param name="segment">Lifespan interval</param>
    /// <param name="actor">actor to which the decoration will be attached to</param>
    /// <param name="icon">URL of the icon</param>
    /// <param name="pixelSize">Size in pixel of the icon</param>
    /// <param name="opacity">Opacity of the icon</param>
    internal void AddOverheadIcon(Segment segment, SingleActor actor, string icon, uint pixelSize = ParserHelper.CombatReplayOverheadDefaultSizeInPixel, float opacity = ParserHelper.CombatReplayOverheadDefaultOpacity)
    {
        Add(new IconOverheadDecoration(icon, pixelSize, opacity, segment, new AgentConnector(actor)));
    }
    internal void AddOverheadIcon((long start, long end) lifespan, SingleActor actor, string icon, uint pixelSize = ParserHelper.CombatReplayOverheadDefaultSizeInPixel, float opacity = ParserHelper.CombatReplayOverheadDefaultOpacity)
    {
        Add(new IconOverheadDecoration(icon, pixelSize, opacity, lifespan, new AgentConnector(actor)));
    }

    /// <summary>
    /// Add an overhead icon decoration
    /// </summary>
    /// <param name="segment">Lifespan interval</param>
    /// <param name="actor">actor to which the decoration will be attached to</param>
    /// <param name="icon">URL of the icon</param>
    /// <param name="rotation">rotation of the icon</param>
    /// <param name="pixelSize">Size in pixel of the icon</param>
    /// <param name="opacity">Opacity of the icon</param>
    internal void AddRotatedOverheadIcon(Segment segment, SingleActor actor, string icon, float rotation, uint pixelSize = ParserHelper.CombatReplayOverheadDefaultSizeInPixel, float opacity = ParserHelper.CombatReplayOverheadDefaultOpacity)
    {
        Add(new IconOverheadDecoration(icon, pixelSize, opacity, segment, new AgentConnector(actor)).UsingRotationConnector(new AngleConnector(rotation)));
    }

    /// <summary>
    /// Add an overhead squad marker
    /// </summary>
    /// <param name="segment">Lifespan interval</param>
    /// <param name="actor">actor to which the decoration will be attached to</param>
    /// <param name="icon">URL of the icon</param>
    /// <param name="rotation">rotation of the icon</param>
    /// <param name="pixelSize">Size in pixel of the icon</param>
    /// <param name="opacity">Opacity of the icon</param>
    internal void AddRotatedOverheadMarkerIcon(Segment segment, SingleActor actor, string icon, float rotation, uint pixelSize = ParserHelper.CombatReplayOverheadDefaultSizeInPixel, float opacity = ParserHelper.CombatReplayOverheadDefaultOpacity)
    {
        Add(new IconOverheadDecoration(icon, pixelSize, opacity, segment, new AgentConnector(actor)).UsingSquadMarker(true).UsingRotationConnector(new AngleConnector(rotation)));
    }

    /// <summary>
    /// Add overhead icon decorations
    /// </summary>
    /// <param name="segments">Lifespan intervals</param>
    /// <param name="actor">actor to which the decoration will be attached to</param>
    /// <param name="icon">URL of the icon</param>
    /// <param name="pixelSize">Size in pixel of the icon</param>
    /// <param name="opacity">Opacity of the icon</param>
    internal void AddOverheadIcons(IEnumerable<Segment> segments, SingleActor actor, string icon, uint pixelSize = ParserHelper.CombatReplayOverheadDefaultSizeInPixel, float opacity = ParserHelper.CombatReplayOverheadDefaultOpacity)
    {
        foreach (Segment segment in segments)
        {
            AddOverheadIcon(segment, actor, icon, pixelSize, opacity);
        }
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a copy using given extra parameters
    /// </summary>
    /// <param name="decoration"></param>
    /// <param name="filled"></param>
    /// <param name="growingEnd"></param>
    /// <param name="reverseGrowing"></param>
    internal void AddDecorationWithFilledWithGrowing(FormDecoration decoration, bool filled, long growingEnd, bool reverseGrowing = false)
    {
        Add(decoration);
        Add(decoration.Copy().UsingFilled(filled).UsingGrowingEnd(growingEnd, reverseGrowing));
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a copy using given extra parameters
    /// </summary>
    /// <param name="decoration"></param>
    /// <param name="growingEnd"></param>
    /// <param name="reverseGrowing"></param>
    internal void AddDecorationWithGrowing(FormDecoration decoration, long growingEnd, bool reverseGrowing = false)
    {
        Add(decoration);
        Add(decoration.Copy().UsingGrowingEnd(growingEnd, reverseGrowing));
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a copy using given extra parameters
    /// </summary>
    /// <param name="decoration"></param>
    /// <param name="filled"></param>
    internal void AddDecorationWithFilled(FormDecoration decoration, bool filled)
    {
        Add(decoration);
        Add(decoration.Copy().UsingFilled(filled));
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a non filled copy using given extra parameters
    /// </summary>
    /// <param name="decoration">Must be filled</param>
    /// <param name="color"></param>
    internal void AddDecorationWithBorder(FormDecoration decoration, string? color = null)
    {
        Add(decoration);
        Add(decoration.GetBorderDecoration(color));
    }
    /// <summary>
    /// Add the decoration twice, the 2nd one being a non filled copy using given extra parameters
    /// </summary>
    /// <param name="decoration">Must be filled</param>
    /// <param name="color"></param>
    /// <param name="opacity"></param>
    internal void AddDecorationWithBorder(FormDecoration decoration, Color color, double opacity)
    {
        AddDecorationWithBorder(decoration, color.WithAlpha(opacity).ToString(true));
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a non filled copy using given extra parameters
    /// </summary>
    /// <param name="decoration">Must be filled</param>
    /// <param name="color"></param>
    /// <param name="growingEnd"></param>
    /// <param name="reverseGrowing"></param>
    internal void AddDecorationWithBorder(FormDecoration decoration, long growingEnd, string? color = null, bool reverseGrowing = false)
    {
        Add(decoration);
        Add(decoration.GetBorderDecoration(color).UsingGrowingEnd(growingEnd, reverseGrowing));
    }

    /// <summary>
    /// Add the decoration twice, the 2nd one being a non filled copy using given extra parameters
    /// </summary>
    /// <param name="decoration">Must be filled</param>
    /// <param name="color"></param>
    /// <param name="growingEnd"></param>
    /// <param name="reverseGrowing"></param>
    internal void AddDecorationWithBorder(FormDecoration decoration, long growingEnd, Color color, double opacity, bool reverseGrowing = false)
    {
        AddDecorationWithBorder(decoration, growingEnd, color.WithAlpha(opacity).ToString(true), reverseGrowing);
    }


    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="BuffEvent"/>.
    /// </summary>
    /// <param name="tethers">Buff events of the tethers.</param>
    /// <param name="color">color of the tether</param>
    /// <param name="thickness">thickness of the tether</param>
    /// <param name="worldSizeThickess">true to indicate that thickness is in inches instead of pixels</param>
    internal void AddTether(IEnumerable<BuffEvent> tethers, string color, uint thickness = 2, bool worldSizeThickess = false)
    {
        int tetherStart = 0;
        AgentItem src = ParserHelper._unknownAgent;
        AgentItem dst = ParserHelper._unknownAgent;
        foreach (BuffEvent tether in tethers)
        {
            if (tether is BuffApplyEvent)
            {
                tetherStart = (int)tether.Time;
                src = tether.By;
                dst = tether.To;
            }
            else if (tether is BuffRemoveAllEvent)
            {
                int tetherEnd = (int)tether.Time;
                if (src != ParserHelper._unknownAgent && dst != ParserHelper._unknownAgent)
                {
                    Add(new LineDecoration((tetherStart, tetherEnd), color, new AgentConnector(dst), new AgentConnector(src)).WithThickess(thickness, worldSizeThickess));
                    src = ParserHelper._unknownAgent;
                    dst = ParserHelper._unknownAgent;
                }
            }
        }
    }
    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="BuffEvent"/>.
    /// </summary>
    /// <param name="tethers">Buff events of the tethers.</param>
    /// <param name="color">color of the tether</param>
    /// <param name="opacity">opacity of the tether</param>
    /// <param name="thickness">thickness of the tether</param>
    /// <param name="worldSizeThickess">true to indicate that thickness is in inches instead of pixels</param>
    internal void AddTether(IEnumerable<BuffEvent> tethers, Color color, double opacity, uint thickness = 2, bool worldSizeThickess = false)
    {
        AddTether(tethers, color.WithAlpha(opacity).ToString(true), thickness, worldSizeThickess);
    }

    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="BuffEvent"/>.
    /// </summary>
    /// <param name="tethers">Buff events of the tethers.</param>
    /// <param name="color">color of the tether</param>
    /// <param name="thickness">thickness of the tether</param>
    /// <param name="worldSizeThickess">true to indicate that thickness is in inches instead of pixels</param>
    internal void AddTetherWithCustomConnectors(ParsedEvtcLog log, IEnumerable<BuffEvent> tethers, string color, CustomConnectorBuilder srcConnectorBuilder, CustomConnectorBuilder dstConnectorBuilder, uint thickness = 2, bool worldSizeThickess = false)
    {
        int tetherStart = 0;
        AgentItem src = ParserHelper._unknownAgent;
        AgentItem dst = ParserHelper._unknownAgent;
        foreach (BuffEvent tether in tethers)
        {
            if (tether is BuffApplyEvent)
            {
                tetherStart = (int)tether.Time;
                src = tether.By;
                dst = tether.To;
            }
            else if (tether is BuffRemoveAllEvent)
            {
                int tetherEnd = (int)tether.Time;
                if (src != ParserHelper._unknownAgent && dst != ParserHelper._unknownAgent)
                {
                    var srcConnector = srcConnectorBuilder(log, src, tetherStart, tetherEnd);
                    var dstConnector = dstConnectorBuilder(log, dst, tetherStart, tetherEnd);
                    if (srcConnector != null && dstConnector != null)
                    {
                        Add(new LineDecoration((tetherStart, tetherEnd), color, dstConnector, srcConnector).WithThickess(thickness, worldSizeThickess));
                        src = ParserHelper._unknownAgent;
                        dst = ParserHelper._unknownAgent;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="BuffEvent"/>.
    /// </summary>
    /// <param name="tethers">Buff events of the tethers.</param>
    /// <param name="color">color of the tether</param>
    /// <param name="opacity">opacity of the tether</param>
    /// <param name="thickness">thickness of the tether</param>
    /// <param name="worldSizeThickess">true to indicate that thickness is in inches instead of pixels</param>
    internal void AddTetherWithCustomConnectors(ParsedEvtcLog log, IEnumerable<BuffEvent> tethers, Color color, double opacity, CustomConnectorBuilder srcConnectorBuilder, CustomConnectorBuilder dstConnectorBuilder, uint thickness = 2, bool worldSizeThickess = false)
    {
        AddTetherWithCustomConnectors(log, tethers, color.WithAlpha(opacity).ToString(true), srcConnectorBuilder, dstConnectorBuilder, thickness, worldSizeThickess);
    }

    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="EffectEvent"/>.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="effect">Tether effect.</param>
    /// <param name="color">Color of the tether decoration.</param>
    /// <param name="duration">Manual set duration to use as override of the <paramref name="effect"/> duration.</param>
    /// <param name="overrideDuration">Wether to override the duration or not.</param>
    internal void AddTetherByEffectGUID(ParsedEvtcLog log, EffectEvent effect, string color, int duration = 0, bool overrideDuration = false)
    {
        if (!effect.IsAroundDst) { return; }

        (long, long) lifespan;
        if (overrideDuration == false)
        {
            lifespan = effect.ComputeLifespan(log, effect.Duration);
        }
        else
        {
            lifespan = (effect.Time, effect.Time + duration);
        }

        if (effect.Src != ParserHelper._unknownAgent && effect.Dst != ParserHelper._unknownAgent)
        {
            Add(new LineDecoration(lifespan, color, new AgentConnector(effect.Dst), new AgentConnector(effect.Src)));
        }
    }

    /// <summary>
    /// Add tether decorations which src and dst are defined by tethers parameter using <see cref="EffectEvent"/>.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="effect">Tether effect.</param>
    /// <param name="color">Color of the tether decoration.</param>
    /// <param name="opacity">Opacity of the tether decoration.</param>
    /// <param name="duration">Manual set duration to use as override of the <paramref name="effect"/> duration.</param>
    /// <param name="overrideDuration">Wether to override the duration or not.</param>
    internal void AddTetherByEffectGUID(ParsedEvtcLog log, EffectEvent effect, Color color, double opacity, int duration = 0, bool overrideDuration = false)
    {
        AddTetherByEffectGUID(log, effect, color.WithAlpha(opacity).ToString(true), duration, overrideDuration);
    }

    /// <summary>
    /// Add tether decoration connecting a player to an agent.<br></br>
    /// The <paramref name="buffId"/> is sourced by an agent that isn't the one to tether to.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="player">The player to tether to <paramref name="toTetherAgentId"/>.</param>
    /// <param name="buffId">ID of the buff sourced by <paramref name="buffSrcAgentId"/>.</param>
    /// <param name="buffSrcAgentId">ID of the agent sourcing the <paramref name="buffId"/>. Either <see cref="ArcDPSEnums.TargetID"/> or <see cref="ArcDPSEnums.TrashID"/>.</param>
    /// <param name="toTetherAgentId">ID of the agent to tether to the <paramref name="player"/>. Either <see cref="ArcDPSEnums.TargetID"/> or <see cref="ArcDPSEnums.TrashID"/>.</param>
    /// <param name="color">Color of the tether.</param>
    /// <param name="firstAwareThreshold">Time threshold in case the agent spawns before the buff application.</param>
    internal void AddTetherByThirdPartySrcBuff(ParsedEvtcLog log, PlayerActor player, long buffId, int buffSrcAgentId, int toTetherAgentId, string color, int firstAwareThreshold = 2000)
    {
        var buffEvents = log.CombatData.GetBuffDataByIDByDst(buffId, player.AgentItem).Where(x => x.CreditedBy.IsSpecies(buffSrcAgentId));
        var buffApplies = buffEvents.OfType<BuffApplyEvent>();
        var buffRemoves = buffEvents.OfType<BuffRemoveAllEvent>();
        var agentsToTether = log.AgentData.GetNPCsByID(toTetherAgentId);

        foreach (BuffApplyEvent buffApply in buffApplies)
        {
            BuffRemoveAllEvent? remove = buffRemoves.FirstOrDefault(x => x.Time > buffApply.Time);
            long removalTime = remove != null ? remove.Time : log.FightData.LogEnd;
            (long, long) lifespan = (buffApply.Time, removalTime);

            foreach (AgentItem agent in agentsToTether)
            {
                if ((Math.Abs(agent.FirstAware - buffApply.Time) < firstAwareThreshold || agent.FirstAware >= buffApply.Time) && agent.FirstAware < removalTime)
                {
                    Add(new LineDecoration(lifespan, color, new AgentConnector(agent), new AgentConnector(player)));
                }
            }
        }
    }
    /// <summary>
    /// Add tether decoration connecting a player to an agent.<br></br>
    /// The <paramref name="buffId"/> is sourced by an agent that isn't the one to tether to.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="player">The player to tether to <paramref name="toTetherAgentId"/>.</param>
    /// <param name="buffId">ID of the buff sourced by <paramref name="buffSrcAgentId"/>.</param>
    /// <param name="buffSrcAgentId">ID of the agent sourcing the <paramref name="buffId"/>. Either <see cref="ArcDPSEnums.TargetID"/> or <see cref="ArcDPSEnums.TrashID"/>.</param>
    /// <param name="toTetherAgentId">ID of the agent to tether to the <paramref name="player"/>. Either <see cref="ArcDPSEnums.TargetID"/> or <see cref="ArcDPSEnums.TrashID"/>.</param>
    /// <param name="color">Color of the tether.</param>
    /// <param name="opacity">Opacity of the tether.</param>
    /// <param name="firstAwareThreshold">Time threshold in case the agent spawns before the buff application.</param>
    internal void AddTetherByThirdPartySrcBuff(ParsedEvtcLog log, PlayerActor player, long buffId, int buffSrcAgentId, int toTetherAgentId, Color color, double opacity, int firstAwareThreshold = 2000)
    {
        AddTetherByThirdPartySrcBuff(log, player, buffId, buffSrcAgentId, toTetherAgentId, color.WithAlpha(opacity).ToString(true), firstAwareThreshold);
    }

    /// <summary>
    /// Adds a moving circle resembling a projectile from a <paramref name="startingPoint"/> to an <paramref name="endingPoint"/>.
    /// </summary>
    /// <param name="startingPoint">Starting position.</param>
    /// <param name="endingPoint">Ending position.</param>
    /// <param name="lifespan">Duration of the animation.</param>
    /// <param name="color">Color of the decoration.</param>
    /// <param name="opacity">Opacity of the color.</param>
    /// <param name="radius">Radius of the circle.</param>
    internal void AddProjectile(in Vector3 startingPoint, in Vector3 endingPoint, (long start, long end) lifespan, Color color, double opacity = 0.2, uint radius = 50)
    {
        AddProjectile(startingPoint, endingPoint, lifespan, color.WithAlpha(opacity).ToString(true), radius);
    }

    /// <summary>
    /// Adds a moving circle resembling a projectile from a <paramref name="startingPoint"/> to an <paramref name="endingPoint"/>.
    /// </summary>
    /// <param name="startingPoint">Starting position.</param>
    /// <param name="endingPoint">Ending position.</param>
    /// <param name="lifespan">Duration of the animation.</param>
    /// <param name="color">Color of the decoration.</param>
    /// <param name="radius">Radius of the circle.</param>
    internal void AddProjectile(in Vector3 startingPoint, in Vector3 endingPoint, (long start, long end) lifespan, string color, uint radius = 50)
    {
        if (startingPoint == default || endingPoint == default)
        {
            return;
        }
        var startPoint = new ParametricPoint3D(startingPoint, lifespan.start);
        var endPoint = new ParametricPoint3D(endingPoint, lifespan.end);
        var shootingCircle = new CircleDecoration(radius, lifespan, color, new InterpolationConnector([startPoint, endPoint]));
        Add(shootingCircle);
    }

    /// <summary>
    /// Adds a non-filled growing circle resembling a shockwave.
    /// </summary>
    /// <param name="connector">Starting position point.</param>
    /// <param name="lifespan">Lifespan of the shockwave.</param>
    /// <param name="color">Color.</param>
    /// <param name="opacity">Opacity of the <paramref name="color"/>.</param>
    /// <param name="radius">Radius of the shockwave.</param>
    /// <remarks>Uses <see cref="GeographicalConnector"/> which allows us to use <see cref="AgentConnector"/> and <see cref="PositionConnector"/>.</remarks>
    internal void AddShockwave(GeographicalConnector connector, (long start, long end) lifespan, Color color, double opacity, uint radius)
    {
        AddShockwave(connector, lifespan, color.WithAlpha(opacity).ToString(true), radius);
    }

    /// <summary>
    /// Adds a non-filled growing circle resembling a shockwave.
    /// </summary>
    /// <param name="connector">Starting position point.</param>
    /// <param name="lifespan">Lifespan of the shockwave.</param>
    /// <param name="color">Color.</param>
    /// <param name="radius">Radius of the shockwave.</param>
    /// <remarks>Uses <see cref="GeographicalConnector"/> which allows us to use <see cref="AgentConnector"/> and <see cref="PositionConnector"/>.</remarks>
    internal void AddShockwave(GeographicalConnector connector, (long start, long end) lifespan, string color, uint radius)
    {
        Add(new CircleDecoration(radius, lifespan, color, connector).UsingFilled(false).UsingGrowingEnd(lifespan.end));
    }



    /// <summary>
    /// Adds concentric doughnuts.
    /// </summary>
    /// <param name="minRadius">Starting radius.</param>
    /// <param name="radiusIncrease">Radius increase for each concentric ring.</param>
    /// <param name="lifespan">Lifespan of the decoration.</param>
    /// <param name="position">Starting position.</param>
    /// <param name="color">Color of the rings.</param>
    /// <param name="initialOpacity">Starting opacity of the rings' color.</param>
    /// <param name="rings">Total number of rings.</param>
    /// <param name="inverted">Inverts the opacity direction.</param>
    internal void AddContrenticRings(uint minRadius, uint radiusIncrease, (long, long) lifespan, in Vector3 position, Color color, float initialOpacity = 0.5f, int rings = 8, bool inverted = false)
    {
        var positionConnector = new PositionConnector(position);

        for (int i = 1; i <= rings; i++)
        {
            uint maxRadius = minRadius + radiusIncrease;
            float opacity = inverted ? initialOpacity * i : initialOpacity / i;
            var circle = new DoughnutDecoration(minRadius, maxRadius, lifespan, color, opacity, positionConnector);
            AddDecorationWithBorder(circle, color, 0.2);
            minRadius = maxRadius;
        }

    }
}

