using DurableBetterProspecting.Core;
using Vintagestory.API.Client;

namespace DurableBetterProspecting.Items;

public class ItemProspectingPick : Vintagestory.GameContent.ItemProspectingPick
{
    public const string ItemRegistryId = "ItemProspectingPick";

    private static readonly List<ProspectingPickMode> _modes = [];
    private static readonly List<SkillItem> _skillItems = [];

    private static void GenerateModes()
    {

    }
}
