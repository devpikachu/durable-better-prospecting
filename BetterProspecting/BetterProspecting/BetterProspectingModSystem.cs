using Vintagestory.API.Common;

namespace BetterProspecting
{
    public class BetterProspectingModSystem : ModSystem
    {
        public static BetterProspectingConfiguration Config;
        const string ConfigFileName = "BetterProspecting.json";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ItemProspectingPick", typeof(ItemBetterProspecting));

            try {
                Config = api.LoadModConfig<BetterProspectingConfiguration>(ConfigFileName);
                if (Config == null) {
                    Config = new BetterProspectingConfiguration();
                    api.StoreModConfig(Config, ConfigFileName);
                } 
            }
            catch (System.Exception e) {
                    Mod.Logger.Error("Could not load config for BetterProspecting! Loading default settings instead.");
                    Mod.Logger.Error(e);
                    Config = new BetterProspectingConfiguration();
            }
        }
    }
}
