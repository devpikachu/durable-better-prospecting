using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Super = DurableBetterProspecting.DurableBetterProspectingModSystem;

namespace DurableBetterProspecting.Items;

public class ItemDurableBetterProspectingPick : ItemProspectingPick
{
    private enum ProspectingTargetType
    {
        Rock,
        Ore,
    }

    private const int DensityModeIndex = 0;
    private const int DistanceSmallModeIndex = 1;
    private const int DistanceLargeModeIndex = 2;
    private const int RockModeIndex = 3;
    private const int AreaSmallModeIndex = 4;
    private const int AreaMediumModeIndex = 5;
    private const int AreaLargeModeIndex = 6;

    private SkillItem[] _toolModes = [];

    public override void OnLoaded(ICoreAPI api)
    {
        _toolModes = ObjectCacheUtil.GetOrCreate(api, "proPickToolModes", () =>
        {
            SkillItem[] modes =
            [
                new()
                {
                    Code = new AssetLocation("density"),
                    Name = Lang.Get("Density Search Mode (Long range, chance based search)")
                },
                new()
                {
                    Code = new AssetLocation("distance_small"),
                    Name = Lang.Get("Distance Search Mode (Short range, distance search)")
                },
                new()
                {
                    Code = new AssetLocation("distance_large"),
                    Name = Lang.Get("Distance Search Mode (Long range, distance search)")
                },
                new()
                {
                    Code = new AssetLocation("rock"),
                    Name = Lang.Get("Rock Search Mode (Long range, distance search for rock)")
                },
                new()
                {
                    Code = new AssetLocation("area_small"),
                    Name = Lang.Get("Area Search Mode (Short range, node based search)")
                },
                new()
                {
                    Code = new AssetLocation("area_medium"),
                    Name = Lang.Get("Area Search Mode (Medium range, node based search)")
                },
                new()
                {
                    Code = new AssetLocation("area_large"),
                    Name = Lang.Get("Area Search Mode (Long range, node based search)")
                }
            ];

            if (api is not ICoreClientAPI capi)
            {
                return modes;
            }

            modes[0].WithIcon(capi, LoadIcon(capi, "heatmap", "game"));
            modes[0].TexturePremultipliedAlpha = false;

            modes[1].WithIcon(capi, LoadIcon(capi, "distance_small"));
            modes[1].TexturePremultipliedAlpha = false;

            modes[2].WithIcon(capi, LoadIcon(capi, "distance_large"));
            modes[2].TexturePremultipliedAlpha = false;

            modes[3].WithIcon(capi, LoadIcon(capi, "rock"));
            modes[3].TexturePremultipliedAlpha = false;

            modes[4].WithIcon(capi, LoadIcon(capi, "area_small"));
            modes[4].TexturePremultipliedAlpha = false;

            modes[5].WithIcon(capi, LoadIcon(capi, "area_medium"));
            modes[5].TexturePremultipliedAlpha = false;

            modes[6].WithIcon(capi, LoadIcon(capi, "area_large"));
            modes[6].TexturePremultipliedAlpha = false;

            return modes;
        });

        base.OnLoaded(api);
    }

    public override float OnBlockBreaking(
        IPlayer player,
        BlockSelection blockSel,
        ItemSlot itemslot,
        float remainingResistance,
        float dt,
        int counter)
    {
        var remain = base.OnBlockBreaking(player, blockSel, itemslot, remainingResistance, dt, counter);
        var toolMode = GetToolMode(itemslot, player, blockSel);

        if (toolMode > 1)
        {
            remain = (float)((remain + (double)remainingResistance) / 2.0);
        }

        return remain;
    }

    public override bool OnBlockBrokenWith(
        IWorldAccessor world,
        Entity byEntity,
        ItemSlot itemSlot,
        BlockSelection blockSelection,
        float dropQuantityMultiplier = 1)
    {
        if (byEntity is not EntityPlayer player)
        {
            return false;
        }

        var toolMode = GetToolMode(itemSlot, player.Player, blockSelection);
        var damage = 1;
        switch (toolMode)
        {
            case DensityModeIndex:
                ProbeBlockDensityMode(world, byEntity, itemSlot, blockSelection);
                damage = Super.Config.DensityModeDurabilityCost;
                break;

            case DistanceSmallModeIndex:
                ProbeDistanceMode(world, player, blockSelection, Super.Config.DistanceModeSmallSize,
                    ProspectingTargetType.Ore);
                damage = Super.Config.DistanceModeDurabilityCost;
                break;

            case DistanceLargeModeIndex:
                ProbeDistanceMode(world, player, blockSelection, Super.Config.DistanceModeLargeSize,
                    ProspectingTargetType.Ore);
                damage = (int)(Super.Config.DistanceModeDurabilityCost *
                               Super.Config.DistanceModeDurabilityCostMultiplier);
                break;

            case RockModeIndex:
                ProbeDistanceMode(world, player, blockSelection, Super.Config.RockModeSize, ProspectingTargetType.Rock);
                damage = Super.Config.RockModeDurabilityCost;
                break;

            case AreaSmallModeIndex:
                ProbeAreaMode(world, player, blockSelection, Super.Config.AreaModeSmallSize);
                damage = Super.Config.AreaModeDurabilityCost;
                break;

            case AreaMediumModeIndex:
                ProbeAreaMode(world, player, blockSelection, Super.Config.AreaModeMediumSize);
                damage = (int)(Super.Config.AreaModeDurabilityCost * Super.Config.AreaModeDurabilityCostMultiplier);
                break;

            case AreaLargeModeIndex:
                ProbeAreaMode(world, player, blockSelection, Super.Config.AreaModeLargeSize);
                damage =
                    (int)(Super.Config.AreaModeDurabilityCost * (Super.Config.AreaModeDurabilityCostMultiplier * 2));
                break;
        }

        if (DamagedBy != null && DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
        {
            DamageItem(world, byEntity, itemSlot, damage);
        }

        return true;
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        return _toolModes;
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        return Math.Min(_toolModes.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
    }

    private void ProbeDistanceMode(
        IWorldAccessor world,
        EntityPlayer player,
        BlockSelection blockSelection,
        int searchSize,
        ProspectingTargetType mode)
    {
        var position = blockSelection.Position;
        if (position == null)
        {
            return;
        }

        var block = world.BlockAccessor.GetBlock(position);
        block.OnBlockBroken(world, position, player.Player, 0);

        if (!IsPropickable(block))
        {
            return;
        }

        if (player.Player is not IServerPlayer serverPlayer)
        {
            return;
        }

        SendNotification(serverPlayer, $"Distance sample taken within a size of {searchSize}");

        var radius = searchSize / 2;
        var minPosition = position.AddCopy(-radius, -radius, -radius);
        var maxPosition = position.AddCopy(radius, radius, radius);

        var distances = new Dictionary<string, int>();
        api.World.BlockAccessor.WalkBlocks(minPosition, maxPosition, (block, x, y, z) =>
        {
            string key;
            int distance;

            switch (mode)
            {
                case ProspectingTargetType.Rock:
                    if (!block.Variant.TryGetValue("rock", out var rockType))
                    {
                        return;
                    }

                    key = $"rock-{rockType}";
                    distance = (int)position.DistanceTo(new BlockPos(x, y, z));
                    if (!distances.ContainsKey(key) || distances[key] > distance)
                    {
                        distances[key] = distance;
                    }

                    break;

                case ProspectingTargetType.Ore:
                    if (block.BlockMaterial != EnumBlockMaterial.Ore ||
                        !block.Variant.TryGetValue("type", out var oreType))
                    {
                        return;
                    }

                    key = $"ore-{oreType}";
                    distance = (int)position.DistanceTo(new BlockPos(x, y, z));

                    if (!distances.ContainsKey(key) || distances[key] > distance)
                    {
                        distances[key] = distance;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        });

        if (distances.Count == 0)
        {
            SendNotification(serverPlayer, $"No {(mode == ProspectingTargetType.Rock ? "rocks" : "ores")} nearby");
            return;
        }

        SendNotification(serverPlayer,
            $"Found the following {(mode == ProspectingTargetType.Rock ? "rocks" : "ores")}:");
        foreach (var distance in distances)
        {
            var key = Lang.GetL(serverPlayer.LanguageCode, distance.Key).ToUpperInvariant();
            SendNotification(serverPlayer, $"{key}: {distance.Value} block(s) away");
        }
    }

    private void ProbeAreaMode(
        IWorldAccessor world,
        EntityPlayer player,
        BlockSelection blockSelection,
        int searchSize)
    {
        var position = blockSelection.Position;
        if (position == null)
        {
            return;
        }

        var block = world.BlockAccessor.GetBlock(position);
        block.OnBlockBroken(world, position, player.Player, 0);

        if (!IsPropickable(block))
        {
            return;
        }

        if (player.Player is not IServerPlayer serverPlayer)
        {
            return;
        }

        SendNotification(serverPlayer, $"Area sample taken within a size of {searchSize}");

        var radius = searchSize / 2;
        var minPosition = position.AddCopy(-radius, -radius, -radius);
        var maxPosition = position.AddCopy(radius, radius, radius);

        var quantities = new Dictionary<string, int>();
        api.World.BlockAccessor.WalkBlocks(minPosition, maxPosition, (block, x, y, z) =>
        {
            if (block.BlockMaterial != EnumBlockMaterial.Ore || !block.Variant.TryGetValue("type", out var oreType))
            {
                return;
            }

            var key = $"ore-{oreType}";
            quantities.TryGetValue(key, out var quantity);
            quantities[key] = quantity + 1;
        });

        if (quantities.Count == 0)
        {
            SendNotification(serverPlayer, "No ores nearby");
            return;
        }

        SendNotification(serverPlayer, "Found the following ores:");
        foreach (var quantity in quantities)
        {
            var key = Lang.GetL(serverPlayer.LanguageCode, quantity.Key);
            var value = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(quantity.Value),
                Lang.Get(quantity.Key));

            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, value, key),
                EnumChatType.Notification);
        }
    }

    private static bool IsPropickable(Block? block)
    {
        return block?.Attributes?["propickable"].AsBool() == true;
    }

    private static LoadedTexture LoadIcon(ICoreClientAPI api, string name, string domain = "durablebetterprospecting")
    {
        return api.Gui.LoadSvgWithPadding(new AssetLocation(domain, $"textures/icons/{name}.svg"),
            48, 48, 5,
            ColorUtil.WhiteArgb);
    }

    private static void SendNotification(IServerPlayer player, string message)
    {
        player.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(player.LanguageCode, message),
            EnumChatType.Notification);
    }
}