using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace DurableBetterProspecting.Items;

public class ItemDurableBetterProspectingPick : ItemProspectingPick
{
    private const string DensityMode = "density";
    private const string RockMode = "rock";
    private const string DistanceSmallMode = "distance_small";
    private const string DistanceMediumMode = "distance_medium";
    private const string DistanceLargeMode = "distance_large";
    private const string AreaSmallMode = "area_small";
    private const string AreaMediumMode = "area_medium";
    private const string AreaLargeMode = "area_large";

    private List<Mode> _modes = [];
    private SkillItem[] _skillItems = [];

    public override void OnLoaded(ICoreAPI api)
    {
        var config = ModConfig.Loaded;

        // @formatter:off
        Mode[] modes =
        [
            new(DensityMode, "Density Search Mode (Long range, chance based search)", "game", "heatmap", config.DensityModeEnabled),
            new(RockMode, "Rock Search Mode (Long range, distance search for rock types)", RockMode, config.RockModeEnabled),
            new(DistanceSmallMode, "Distance Search Mode (Short range, distance search for ore types)", DistanceSmallMode, config.DistanceModeEnabled),
            new(DistanceMediumMode, "Distance Search Mode (Medium range, distance search for ore types)", DistanceMediumMode, config.DistanceModeEnabled),
            new(DistanceLargeMode, "Distance Search Mode (Long range, distance search for ore types)", DistanceLargeMode, config.DistanceModeEnabled),
            new(AreaSmallMode, "Area Search Mode (Short range, exact search for ore types)", AreaSmallMode, config.AreaModeEnabled),
            new(AreaMediumMode, "Area Search Mode (Medium range, exact search for ore types)", AreaMediumMode, config.AreaModeEnabled),
            new(AreaLargeMode, "Area Search Mode (Long range, exact search for ore types)", AreaLargeMode, config.AreaModeEnabled)
        ];
        _modes = modes.Where(m => m.Enabled).ToList();
        // @formatter:on

        _skillItems = ObjectCacheUtil.GetOrCreate(api, "proPickToolModes", () =>
        {
            return _modes.Select(m =>
            {
                var skillItem = new SkillItem()
                {
                    Code = m.Code,
                    Name = m.Name
                };

                if (api is not ICoreClientAPI capi)
                {
                    return skillItem;
                }

                skillItem.WithIcon(capi, LoadIcon(capi, m.IconDomain, m.IconName));
                skillItem.TexturePremultipliedAlpha = false;

                return skillItem;
            }).ToArray();
        });

        base.OnLoaded(api);
    }

    public override float OnBlockBreaking(
        IPlayer player,
        BlockSelection blockSelection,
        ItemSlot itemSlot,
        float remainingResistance,
        float dt,
        int counter)
    {
        var remain = base.OnBlockBreaking(player, blockSelection, itemSlot, remainingResistance, dt, counter);

        var modeIndex = GetToolMode(itemSlot, player, blockSelection);
        var mode = _modes[modeIndex];

        return mode.Code switch
        {
            DensityMode => remain,
            _ => (float)((remain + (double)remainingResistance) / 2.0)
        };
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

        var modeIndex = GetToolMode(itemSlot, player.Player, blockSelection);
        var mode = _modes[modeIndex];

        var damage = 1;
        switch (mode.Code)
        {
            case DensityMode:
                ProbeBlockDensityMode(world, byEntity, itemSlot, blockSelection);
                damage = ModConfig.Loaded.DensityModeDurabilityCost;
                break;

            case RockMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.RockModeSize,
                    ProspectingTargetType.Rock);
                damage = ModConfig.Loaded.RockModeDurabilityCost;
                break;

            case DistanceSmallMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeSmallSize,
                    ProspectingTargetType.Ore);
                damage = ModConfig.Loaded.DistanceModeSmallDurabilityCost;
                break;

            case DistanceMediumMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeMediumSize,
                    ProspectingTargetType.Ore);
                damage = ModConfig.Loaded.DistanceModeMediumDurabilityCost;
                break;

            case DistanceLargeMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeLargeSize,
                    ProspectingTargetType.Ore);
                damage = ModConfig.Loaded.DistanceModeLargeDurabilityCost;
                break;

            case AreaSmallMode:
                ProbeAreaMode(world, player, blockSelection, ModConfig.Loaded.AreaModeSmallSize);
                damage = ModConfig.Loaded.AreaModeSmallDurabilityCost;
                break;

            case AreaMediumMode:
                ProbeAreaMode(world, player, blockSelection, ModConfig.Loaded.AreaModeMediumSize);
                damage = ModConfig.Loaded.AreaModeMediumDurabilityCost;
                break;

            case AreaLargeMode:
                ProbeAreaMode(world, player, blockSelection, ModConfig.Loaded.AreaModeLargeSize);
                damage = ModConfig.Loaded.AreaModeLargeDurabilityCost;
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
        return _skillItems;
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        return Math.Min(_skillItems.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
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

        SendNotification(serverPlayer, "Distance sample taken within a size of {0}", searchSize);

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
                    if (block.BlockMaterial != EnumBlockMaterial.Ore || !block.Variant.TryGetValue("type", out var oreType))
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
            SendNotification(serverPlayer, "No {0} nearby", mode == ProspectingTargetType.Rock ? "rocks" : "ores");
            return;
        }

        SendNotification(serverPlayer, "Found the following {0}:", mode == ProspectingTargetType.Rock ? "rocks" : "ores");
        foreach (var distance in distances)
        {
            var key = Lang.GetL(serverPlayer.LanguageCode, distance.Key).ToUpperInvariant();
            SendNotification(serverPlayer, "{0}: {1} block(s) away", key, distance.Value);
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

        SendNotification(serverPlayer, "Area sample taken within a size of {0}", searchSize);

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
            SendNotification(serverPlayer, "No {0} nearby", "ores");
            return;
        }

        SendNotification(serverPlayer, "Found the following {0}:", "ores");
        foreach (var quantity in quantities)
        {
            var key = Lang.GetL(serverPlayer.LanguageCode, quantity.Key);
            var value = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(quantity.Value), Lang.Get(quantity.Key));
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, value, key), EnumChatType.Notification);
        }
    }

    private static bool IsPropickable(Block? block)
    {
        return block?.Attributes?["propickable"].AsBool() == true;
    }

    private static void SendNotification(IServerPlayer player, string message, params object[] args)
    {
        player.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(player.LanguageCode, message, args), EnumChatType.Notification);
    }

    private static LoadedTexture LoadIcon(ICoreClientAPI api, string domain, string name)
    {
        return api.Gui.LoadSvgWithPadding(new AssetLocation(domain, $"textures/icons/{name}.svg"), 48, 48, 5, ColorUtil.WhiteArgb);
    }

    private enum ProspectingTargetType
    {
        Rock,
        Ore
    }

    private class Mode
    {
        public string Code { get; init; }
        public string Name { get; init; }
        public string IconDomain { get; init; }
        public string IconName { get; init; }
        public bool Enabled { get; init; }

        public Mode(string code, string name, string iconName, bool enabled)
        {
            Code = code;
            Name = name;
            IconDomain = ModSystem.ModId;
            IconName = iconName;
            Enabled = enabled;
        }

        public Mode(string code, string name, string iconDomain, string iconName, bool enabled)
        {
            Code = code;
            Name = name;
            IconDomain = iconDomain;
            IconName = iconName;
            Enabled = enabled;
        }
    }
}
