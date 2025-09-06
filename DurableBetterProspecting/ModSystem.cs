using System;
using DurableBetterProspecting.Items;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

[UsedImplicitly]
public class ModSystem : Vintagestory.API.Common.ModSystem
{
    public const string ModId = "durablebetterprospecting";

    public override void Start(ICoreAPI api)
    {
        try
        {
            ModConfig.LoadOrSaveDefault(api);
            ModConfig.RegisterListeners(api);
        }
        catch (Exception ex)
        {
            Mod.Logger.Error($"Could not load config for {ModId}! Loading defaults instead.");
            Mod.Logger.Error(ex);
        }

        api.RegisterItemClass("ItemProspectingPick", typeof(ItemDurableBetterProspectingPick));
    }

    public override void Dispose()
    {
        ModConfig.UnregisterListeners();
    }
}