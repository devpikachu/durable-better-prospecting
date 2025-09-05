using DurableBetterProspecting.Items;
using Vintagestory.API.Common;

namespace DurableBetterProspecting;

public class DurableBetterProspectingModSystem : ModSystem
{
    private const string ConfigFileName = "DurableBetterProspecting.json";

    public static DurableBetterProspectingConfig Config { get; private set; } = new();

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("ItemProspectingPick", typeof(ItemDurableBetterProspectingPick));

        try
        {
            Config = api.LoadModConfig<DurableBetterProspectingConfig>(ConfigFileName);
            if (Config != null)
            {
                return;
            }

            Config = new DurableBetterProspectingConfig();
            api.StoreModConfig(Config, ConfigFileName);
        }
        catch (System.Exception e)
        {
            Mod.Logger.Error("Could not load config for DurableBetterProspecting! Loading default settings instead.");
            Mod.Logger.Error(e);

            Config = new DurableBetterProspectingConfig();
        }
    }
}