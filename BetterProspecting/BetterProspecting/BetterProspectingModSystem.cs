using Vintagestory.API.Common;

namespace BetterProspecting
{
    public class BetterProspectingModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ItemProspectingPick", typeof(ItemBetterProspecting));
        }
    }
}
