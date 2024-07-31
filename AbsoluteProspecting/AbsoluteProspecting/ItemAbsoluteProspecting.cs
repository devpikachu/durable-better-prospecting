using System;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.Server;
using Vintagestory.API.Datastructures;

namespace AbsoluteProspecting
{
    public class ItemAbsoluteProspecting : ItemProspectingPick
    {
        SkillItem[]? toolModes;

        public override void OnLoaded(ICoreAPI api)
        {
            //I need to add assets!

            ICoreClientAPI? capi = api as ICoreClientAPI;
            toolModes = ObjectCacheUtil.GetOrCreate(api, "proPickToolModes", () =>
            {
                SkillItem[] modes;
                if (api.World.Config.GetString("propickNodeSearchRadius").ToInt() > 0)
                {
                    modes = new SkillItem[7];
                    modes[0] = new SkillItem() { Code = new AssetLocation("density"), Name = Lang.Get("Density Search Mode (Long range, chance based search)") };
                    modes[1] = new SkillItem() { Code = new AssetLocation("line"), Name = Lang.Get("Line Sample Mode (Searches in a straight line)") };
                    modes[2] = new SkillItem() { Code = new AssetLocation("area1"), Name = Lang.Get("Area Sample Mode (Searches in a small area)") };
                    modes[3] = new SkillItem() { Code = new AssetLocation("area2"), Name = Lang.Get("Area Sample Mode (Searches in a medium area)") };
                    modes[4] = new SkillItem() { Code = new AssetLocation("area3"), Name = Lang.Get("Area Sample Mode (Searches in a large area)") };
                    modes[5] = new SkillItem() { Code = new AssetLocation("stone"), Name = Lang.Get("Stone Sample Mode (Searches a very large area for stone)") };
                    modes[6] = new SkillItem() { Code = new AssetLocation("node"), Name = Lang.Get("Node Search Mode (Short range, exact search)") };

                }
                else
                {
                    modes = new SkillItem[5];
                    modes[0] = new SkillItem() { Code = new AssetLocation("density"), Name = Lang.Get("Density Search Mode (Long range, chance based search)") };
                    modes[1] = new SkillItem() { Code = new AssetLocation("line"), Name = Lang.Get("Line Sample Mode (Searches in a straight line)") };
                    modes[2] = new SkillItem() { Code = new AssetLocation("area1"), Name = Lang.Get("Area Sample Mode (Searches in a small area)") };
                    modes[3] = new SkillItem() { Code = new AssetLocation("area2"), Name = Lang.Get("Area Sample Mode (Searches in a medium area)") };
                    modes[4] = new SkillItem() { Code = new AssetLocation("area3"), Name = Lang.Get("Area Sample Mode (Searches in a large area)") };
                    modes[5] = new SkillItem() { Code = new AssetLocation("stone"), Name = Lang.Get("Stone Sample Mode (Searches a very large area for stone)") };
                }

                if (capi != null)
                {
                    modes[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/heatmap.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[0].TexturePremultipliedAlpha = false;

                    modes[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("absoluteprospecting", "textures/icons/abpro_line.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[1].TexturePremultipliedAlpha = false;

                    modes[2].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("absoluteprospecting", "textures/icons/abpro_small.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[2].TexturePremultipliedAlpha = false;

                    modes[3].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("absoluteprospecting", "textures/icons/abpro_med.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[3].TexturePremultipliedAlpha = false;

                    modes[4].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("absoluteprospecting", "textures/icons/abpro_large.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[4].TexturePremultipliedAlpha = false;

                    modes[5].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("absoluteprospecting", "textures/icons/abpro_stone.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[5].TexturePremultipliedAlpha = false;

                    if (modes.Length > 6)
                    {
                        modes[6].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/rocks.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                        modes[6].TexturePremultipliedAlpha = false;
                    }
                }

                return modes;
            });

            base.OnLoaded(api);
        }

        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return toolModes;
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            return Math.Min(toolModes.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
        }

        public override float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            float remain = base.OnBlockBreaking(player, blockSel, itemslot, remainingResistance, dt, counter);
            int toolMode = GetToolMode(itemslot, player, blockSel);

            //line search
            if (toolMode == 1) remain = (remain + remainingResistance) / 2.2f;
            //small area search
            if (toolMode == 2) remain = (remain + remainingResistance) / 2f;
            //medium area search
            if (toolMode == 3) remain = (remain + remainingResistance) / 2f;
            //large area search
            if (toolMode == 4) remain = (remain + remainingResistance) / 2f;
            //very large area search
            if (toolMode == 5) remain = (remain + remainingResistance) / 2f;
            //vanilla node search
            if (toolMode == 6) remain = (remain + remainingResistance) / 2.2f;

            return remain;
        }

        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            int toolMode = GetToolMode(itemslot, (byEntity as EntityPlayer).Player, blockSel);
            int radius = api.World.Config.GetString("propickNodeSearchRadius").ToInt();
            int damage = 1;

            if (toolMode == 6 && radius > 0)
            {
                ProbeBlockNodeMode(world, byEntity, itemslot, blockSel, radius);
                damage = 2;
            }
            else if (toolMode == 5)
            {
                ProbeStoneSampleMode(world, byEntity, itemslot, blockSel);
                damage = 6;
            }
            else if (toolMode == 4)
            {
                //large
                ProbeAreaSampleMode(world, byEntity, itemslot, blockSel, (int)EnumProspectingArea.LargeArea, (int)EnumProspectingArea.Ycoords);
                damage = 5;
            }
            else if (toolMode == 3)
            {
                //medium
                ProbeAreaSampleMode(world, byEntity, itemslot, blockSel, (int)EnumProspectingArea.MediumArea, (int)EnumProspectingArea.Ycoords);
                damage = 4;
            } 
            else if (toolMode == 2)
            {
                //small
                ProbeAreaSampleMode(world, byEntity, itemslot, blockSel, (int)EnumProspectingArea.SmallArea, (int)EnumProspectingArea.Ycoords);
                damage = 3;
            }
            else if (toolMode == 1)
            {
                ProbeLineSampleMode(world, byEntity, itemslot, blockSel);
                damage = 2;
            }
            else
            {
                ProbeBlockDensityMode(world, byEntity, itemslot, blockSel);
            }

            if (DamagedBy != null && DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
            {
                DamageItem(world, byEntity, itemslot, damage);
            }

            return true;
        }

        protected virtual void ProbeStoneSampleMode(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel)
        {
            var radius = (int)EnumProspectingArea.SaltArea;
            IPlayer? byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

            if (!isPropickable(block)) return;

            IServerPlayer? serverPlayer = byPlayer as IServerPlayer;
            if (serverPlayer == null) return;

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, $"Stone sample taken for a length of 128:"), EnumChatType.Notification);
            
            Dictionary<string, int> quantityFound = new Dictionary<string, int>();

            BlockPos blockPos = blockSel.Position.Copy();
            api.World.BlockAccessor.WalkBlocks(blockPos.AddCopy(radius, 200, radius), blockPos.AddCopy(-radius, -200, -radius), delegate (Block nblock, int x, int y, int z)
            {
                if (nblock.Variant.ContainsKey("rock"))
                {
                    string key = nblock.Variant["rock"];
                    int value = 0;
                    quantityFound.TryGetValue(key, out value);
                    quantityFound[key] = value + 1;
                }
            });
            List<KeyValuePair<string, int>> list = quantityFound.OrderByDescending((KeyValuePair<string, int> val) => val.Value).ToList();
            if (list.Count == 0)
            {
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "No stone found nearby"), EnumChatType.Notification);
                return;
            }

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Found the following stone types"), EnumChatType.Notification);
            foreach (KeyValuePair<string, int> item in list)
            {
                string l = Lang.GetL(serverPlayer.LanguageCode, item.Key);
                string l2 = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(item.Value), Lang.Get(item.Key));
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, l2, l), EnumChatType.Notification);
            }
        }

        protected virtual void ProbeAreaSampleMode(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, int xzlength, int ylength)
        {
            IPlayer? byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

            if (!isPropickable(block)) return;

            IServerPlayer? serverPlayer = byPlayer as IServerPlayer;
            if (serverPlayer == null) return;

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, $"Area sample taken for a length of {xzlength}:"), EnumChatType.Notification);

            Dictionary<string, int> quantityFound = new Dictionary<string, int>();

            BlockPos blockPos = blockSel.Position.Copy();
            api.World.BlockAccessor.WalkBlocks(blockPos.AddCopy(xzlength, ylength, xzlength), blockPos.AddCopy(-xzlength, -ylength, -xzlength), delegate (Block nblock, int x, int y, int z)
            {
                if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.ContainsKey("type"))
                {
                    string key = "ore-" + nblock.Variant["type"];
                    int value = 0;
                    quantityFound.TryGetValue(key, out value);
                    quantityFound[key] = value + 1;
                }
            });
            List<KeyValuePair<string, int>> list = quantityFound.OrderByDescending((KeyValuePair<string, int> val) => val.Value).ToList();
            if (list.Count == 0)
            {
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "No ore node nearby"), EnumChatType.Notification);
                return;
            }

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Found the following ore nodes"), EnumChatType.Notification);
            foreach (KeyValuePair<string, int> item in list)
            {
                string l = Lang.GetL(serverPlayer.LanguageCode, item.Key);
                string l2 = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(item.Value), Lang.Get(item.Key));
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, l2, l), EnumChatType.Notification);
            }
        }


        protected virtual void ProbeLineSampleMode(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel)
        {
            IPlayer? byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

            if (!isPropickable(block)) return;

            IServerPlayer? serverPlayer = byPlayer as IServerPlayer;
            if (serverPlayer == null) return;

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Line sample taken for length of 32:"), EnumChatType.Notification);

            BlockFacing face = blockSel.Face;

            Dictionary<string, int> quantityFound = new Dictionary<string, int>();

            BlockPos searchPos = blockSel.Position.Copy();
            for (int i = 0; i < 32; i++)
            {
                Block nblock = api.World.BlockAccessor.GetBlock(searchPos);

                if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.ContainsKey("type"))
                {
                    string key = "ore-" + nblock.Variant["type"];
                    int q = 0;
                    quantityFound.TryGetValue(key, out q);
                    quantityFound[key] = q + 1;
                }

                switch (face.Code)
                {
                    case "north":
                        searchPos.Z++;
                        break;
                    case "south":
                        searchPos.Z--;
                        break;
                    case "east":
                        searchPos.X--;
                        break;
                    case "west":
                        searchPos.X++;
                        break;
                    case "up":
                        searchPos.Y--;
                        break;
                    case "down":
                        searchPos.Y++;
                        break;
                }
            }

            var resultsOrderedDesc = quantityFound.OrderByDescending(val => val.Value).ToList();

            if (resultsOrderedDesc.Count == 0)
            {
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "No ore node found"), EnumChatType.Notification);
            }
            else
            {
                serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Found the following ore nodes"), EnumChatType.Notification);
                foreach (var val in resultsOrderedDesc)
                {
                    string orename = Lang.GetL(serverPlayer.LanguageCode, val.Key);

                    string resultText = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(val.Value), Lang.Get(val.Key));

                    serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, resultText, orename), EnumChatType.Notification);
                }
            }
        }

        private bool isPropickable(Block block)
        {
            return block?.Attributes?["propickable"].AsBool(false) == true;
        }
    }
}
