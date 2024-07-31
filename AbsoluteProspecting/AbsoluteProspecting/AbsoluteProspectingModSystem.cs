using Vintagestory.API.Common;

namespace AbsoluteProspecting
{
    public class AbsoluteProspectingModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ItemProspectingPick", typeof(ItemAbsoluteProspecting));
        }
    }
}
