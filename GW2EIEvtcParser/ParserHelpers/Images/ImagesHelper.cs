using static GW2EIEvtcParser.ParserHelper;
using static GW2EIEvtcParser.ParserHelpers.ParserIcons;
using static GW2EIEvtcParser.SpeciesIDs;

namespace GW2EIEvtcParser.ParserHelpers;

public static class ImagesHelper
{
    internal static string GetHighResolutionProfIcon(Spec spec)
    {
        return HighResProfIcons.TryGetValue(spec, out var icon) ? icon : UnknownProfessionIcon;
    }

    internal static string GetProfIcon(Spec spec)
    {
        return BaseResProfIcons.TryGetValue(spec, out var icon) ? icon : UnknownProfessionIcon;
    }

    internal static string GetGadgetIcon()
    {
        return GenericGadgetIcon;
    }

    internal static string GetNPCIcon(int id)
    {
        if (id == 0)
        {
            return UnknownNPCIcon;
        }

        TargetID target = GetTargetID(id);
        if (target != TargetID.Unknown)
        {
            return TargetNPCIcons.TryGetValue(target, out var targetIcon) ? targetIcon : GenericEnemyIcon;
        }
        TrashID trash = GetTrashID(id);
        if (trash != TrashID.Unknown)
        {
            return TrashNPCIcons.TryGetValue(trash, out var trashIcon) ? trashIcon : GenericEnemyIcon;
        }
        MinionID minion = GetMinionID(id);
        if (minion != MinionID.Unknown)
        {
            return MinionNPCIcons.TryGetValue(minion, out var minionIcon) ? minionIcon : GenericEnemyIcon;
        }

        return GenericEnemyIcon;
    }
}

