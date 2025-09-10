// ReSharper disable AccessToModifiedClosure

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private const string NodeMode = "node";
    private const string RockMode = "rock";
    private const string DistanceSmallMode = "distance_small";
    private const string DistanceMediumMode = "distance_medium";
    private const string DistanceLargeMode = "distance_large";
    private const string AreaSmallMode = "area_small";
    private const string AreaMediumMode = "area_medium";
    private const string AreaLargeMode = "area_large";

    private List<Mode> _modes = [];
    private SkillItem[] _skillItems = [];

    public override void OnLoaded(ICoreAPI pApi)
    {
        ReloadModes();
        base.OnLoaded(pApi);

        ModConfig.SynchronizedConfig += ReloadModes;
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

        return mode.Id switch
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
        switch (mode.Id)
        {
            case DensityMode:
                ProbeBlockDensityMode(world, byEntity, itemSlot, blockSelection);
                damage = ModConfig.Loaded.DensityModeDurabilityCost;
                break;

            case NodeMode:
                var nodeSize = api.World.Config.GetString("propickNodeSearchRadius").ToInt();
                ProbeBlockNodeMode(world, byEntity, itemSlot, blockSelection, nodeSize);
                damage = ModConfig.Loaded.NodeModeDurabilityCost;
                break;

            case RockMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.RockModeSize,
                    Target.Rock);
                damage = ModConfig.Loaded.RockModeDurabilityCost;
                break;

            case DistanceSmallMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeSmallSize,
                    Target.Ore);
                damage = ModConfig.Loaded.DistanceModeSmallDurabilityCost;
                break;

            case DistanceMediumMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeMediumSize,
                    Target.Ore);
                damage = ModConfig.Loaded.DistanceModeMediumDurabilityCost;
                break;

            case DistanceLargeMode:
                ProbeDistanceMode(world, player, blockSelection, ModConfig.Loaded.DistanceModeLargeSize,
                    Target.Ore);
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

    public override void OnUnloaded(ICoreAPI pApi)
    {
        ModConfig.SynchronizedConfig -= ReloadModes;

        foreach (var skill in _skillItems)
        {
            skill.Dispose();
        }
    }

    private void ReloadModes()
    {
        var config = ModConfig.Loaded;
        var nodeSize = api.World.Config.GetString("propickNodeSearchRadius").ToInt();

        // @formatter:off
        Mode[] modes =
        [
            new(DensityMode, "Density Search Mode (Long range, chance based search)", "game", "heatmap", config.DensityModeEnabled),
            new(NodeMode, "Node Search Mode (Short range, exact search)", "game", "rocks", nodeSize > 0 && config.NodeModeEnabled),
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

        ObjectCacheUtil.Delete(api, "proPickToolModes");
        _skillItems = ObjectCacheUtil.GetOrCreate(api, "proPickToolModes", () =>
        {
            return _modes.Select(m =>
            {
                var skillItem = new SkillItem()
                {
                    Code = m.Id,
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
    }

    private void ProbeDistanceMode(
        IWorldAccessor world,
        EntityPlayer player,
        BlockSelection blockSelection,
        int searchSize,
        Target target)
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

        var radius = searchSize / 2;
        var minPosition = position.AddCopy(-radius, -radius, -radius);
        var maxPosition = position.AddCopy(radius, radius, radius);

        var readings = new Dictionary<string, DistanceReading>();
        // ReSharper disable once VariableHidesOuterVariable
        api.World.BlockAccessor.WalkBlocks(minPosition, maxPosition, (block, x, y, z) =>
        {
            string id;

            switch (target)
            {
                case Target.Rock:
                    if (!block.Variant.TryGetValue("rock", out var rockType))
                    {
                        return;
                    }

                    id = $"rock-{rockType}";
                    break;

                case Target.Ore:
                    if (block.BlockMaterial != EnumBlockMaterial.Ore || !block.Variant.TryGetValue("type", out var oreType))
                    {
                        return;
                    }

                    id = $"ore-{oreType}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            var distance = (int)position.DistanceTo(new BlockPos(x, y, z));

            if (!readings.TryGetValue(id, out var reading))
            {
                readings.Add(id, new DistanceReading(distance, block));
                return;
            }

            if (reading.Distance > distance)
            {
                reading.Distance = distance;
            }
        });

        var language = serverPlayer.LanguageCode;
        var rocks = Lang.GetL(language, "rocks");
        var ores = Lang.GetL(language, "ores");
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(Lang.GetL(language, "Distance sample taken within a size of {0}", searchSize));

        if (readings.Count == 0)
        {
            messageBuilder.AppendLine(Lang.GetL(language, "No {0} nearby", target == Target.Rock ? rocks : ores));
            SendNotification(serverPlayer, messageBuilder.ToString());
            return;
        }

        if (ModConfig.Loaded.OrderReadings)
        {
            readings = ModConfig.Loaded.OrderReadingsDirection == ModConfig.OrderAscending
                ? readings.OrderBy(r => r.Value.Distance).ToDictionary()
                : readings.OrderByDescending(r => r.Value.Distance).ToDictionary();
        }

        messageBuilder.AppendLine(Lang.GetL(language, "Found the following {0}:", target == Target.Rock ? rocks : ores));
        foreach (var reading in readings)
        {
            var name = Lang.GetL(language, reading.Key);
            var handBookLink = $"handbook://{GuiHandbookItemStackPage.PageCodeForStack(new ItemStack(reading.Value.Block))}";
            var link = $"<a href=\"{handBookLink}\">{name}</a>";

            messageBuilder.AppendLine(Lang.GetL(language, "{0}: {1} block(s) away", link, reading.Value.Distance));
        }

        SendNotification(serverPlayer, messageBuilder.ToString());
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

        var radius = searchSize / 2;
        var minPosition = position.AddCopy(-radius, -radius, -radius);
        var maxPosition = position.AddCopy(radius, radius, radius);

        var readings = new Dictionary<string, AreaReading>();
        // ReSharper disable once VariableHidesOuterVariable
        api.World.BlockAccessor.WalkBlocks(minPosition, maxPosition, (block, _, _, _) =>
        {
            if (block.BlockMaterial != EnumBlockMaterial.Ore || !block.Variant.TryGetValue("type", out var oreType))
            {
                return;
            }

            var key = $"ore-{oreType}";

            if (!readings.TryGetValue(key, out var reading))
            {
                readings.Add(key, new AreaReading(1, block));
                return;
            }

            reading.Quantity += 1;
        });

        var language = serverPlayer.LanguageCode;
        var ores = Lang.GetL(language, "ores");
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(Lang.GetL(language, "Area sample taken within a size of {0}", searchSize));

        if (readings.Count == 0)
        {
            messageBuilder.AppendLine(Lang.GetL(language, "No {0} nearby", ores));
            SendNotification(serverPlayer, messageBuilder.ToString());
            return;
        }

        if (ModConfig.Loaded.OrderReadings)
        {
            readings = ModConfig.Loaded.OrderReadingsDirection == ModConfig.OrderAscending
                ? readings.OrderBy(r => r.Value.Quantity).ToDictionary()
                : readings.OrderByDescending(r => r.Value.Quantity).ToDictionary();
        }

        messageBuilder.AppendLine(Lang.GetL(language, "Found the following {0}:", ores));
        foreach (var reading in readings)
        {
            var name = Lang.GetL(language, reading.Key);
            var handBookLink = $"handbook://{GuiHandbookItemStackPage.PageCodeForStack(new ItemStack(reading.Value.Block))}";
            var link = $"<a href=\"{handBookLink}\">{name}</a>";
            var quantity = Lang.GetL(language, resultTextByQuantity(reading.Value.Quantity), link);

            messageBuilder.AppendLine(quantity);
        }

        SendNotification(serverPlayer, messageBuilder.ToString());
    }

    private static bool IsPropickable(Block? block)
    {
        return block?.Attributes?["propickable"].AsBool() == true;
    }

    private static void SendNotification(IServerPlayer player, string message)
    {
        player.SendMessage(GlobalConstants.InfoLogChatGroup, message, EnumChatType.Notification);
    }

    private static LoadedTexture LoadIcon(ICoreClientAPI api, string domain, string name)
    {
        return api.Gui.LoadSvgWithPadding(new AssetLocation(domain, $"textures/icons/{name}.svg"), 48, 48, 5, ColorUtil.WhiteArgb);
    }

    private enum Target
    {
        Rock,
        Ore
    }

    private class Mode
    {
        public string Id { get; }
        public string Name { get; }
        public string IconDomain { get; }
        public string IconName { get; }
        public bool Enabled { get; }

        public Mode(string id, string name, string iconName, bool enabled)
        {
            Id = id;
            Name = name;
            IconDomain = ModSystem.ModId;
            IconName = iconName;
            Enabled = enabled;
        }

        public Mode(string id, string name, string iconDomain, string iconName, bool enabled)
        {
            Id = id;
            Name = name;
            IconDomain = iconDomain;
            IconName = iconName;
            Enabled = enabled;
        }
    }

    private class DistanceReading
    {
        public int Distance { get; set; }
        public Block Block { get; }

        public DistanceReading(int distance, Block block)
        {
            Distance = distance;
            Block = block;
        }
    }

    private class AreaReading
    {
        public int Quantity { get; set; }
        public Block Block { get; }

        public AreaReading(int quantity, Block block)
        {
            Quantity = quantity;
            Block = block;
        }
    }
}
