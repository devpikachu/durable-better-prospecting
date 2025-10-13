using Common.Mod.Common.Config;
using DryIoc;
using DurableBetterProspecting.Core;
using DurableBetterProspecting.Managers;
using DurableBetterProspecting.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Server;

namespace DurableBetterProspecting.Items;

public class ItemProspectingPick : Vintagestory.GameContent.ItemProspectingPick
{
    public const string ItemRegistryId = "ItemProspectingPick";

    private const string ProspectableAttributeKey = "propickable";

    private readonly IConfigSystem _configSystem;
    private readonly INetworkChannel _channel;
    private readonly ModeManager _modeManager;

    private DurableBetterProspectingCommonConfig _commonConfig;

    public ItemProspectingPick()
    {
        var container = DurableBetterProspectingSystem.Instance!.Container;

        _configSystem = container.Resolve<IConfigSystem>();
        _channel = container.Resolve<INetworkChannel>();
        _modeManager = container.Resolve<ModeManager>();

        _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
        _configSystem.Updated += type =>
        {
            if (type is RootConfigType.Common)
            {
                _commonConfig = _configSystem.GetCommon<DurableBetterProspectingCommonConfig>();
            }
        };

        if (_channel is IServerNetworkChannel serverChannel)
        {
            serverChannel
                .RegisterMessageType<ReadingPacket>()
                .SetMessageHandler<ReadingPacket>((_, _) => { });
        }

        if (_channel is IClientNetworkChannel clientChannel)
        {
            var readingManager = container.Resolve<ReadingManager>();
            clientChannel
                .RegisterMessageType<ReadingPacket>()
                .SetMessageHandler<ReadingPacket>(readingManager.ProcessReading);
        }
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        return Math.Min(_modeManager.GetSkillItems().Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        return _modeManager.GetSkillItems();
    }

    public override float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemSlot, float remainingResistance, float dt, int counter)
    {
        var remain = base.OnBlockBreaking(player, blockSel, itemSlot, remainingResistance, dt, counter);
        var mode = _modeManager.GetMode(GetToolMode(itemSlot, player, blockSel));
        return mode.Equals(_modeManager.DensityMode) ? remain : (float)((remain + (double)remainingResistance) / 2.0f);
    }

    public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
    {
        if (byEntity is not EntityPlayer player)
        {
            return false;
        }

        var mode = _modeManager.GetMode(GetToolMode(itemSlot, player.Player, blockSel));
        var damage = 0;

        #region Density mode

        if (mode.Equals(_modeManager.DensityMode))
        {
            if (_commonConfig.DensityMode.Simplified)
            {
                var position = blockSel.Position;
                var block = world.BlockAccessor.GetBlock(position);
                block.OnBlockBroken(world, position, player.Player, 0);

                if (player.Player is IServerPlayer serverPlayer)
                {
                    PrintProbeResults(world, serverPlayer, itemSlot, position);
                    damage = 3 * _commonConfig.DensityMode.DurabilityCost;
                }
            }
            else
            {
                ProbeBlockDensityMode(world, byEntity, itemSlot, blockSel);
                damage = _commonConfig.DensityMode.DurabilityCost;
            }
        }

        #endregion Density mode

        #region Node mode

        if (mode.Equals(_modeManager.NodeMode))
        {
            var nodeSize = api.World.Config.GetString(ModeManager.NodeModeSampleSizeKey).ToInt(6);
            ProbeBlockNodeMode(world, byEntity, itemSlot, blockSel, nodeSize);
            damage = _commonConfig.NodeMode.DurabilityCost;
        }

        #endregion Node mode

        #region Rock mode

        if (mode.Equals(_modeManager.RockMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Rock, SampleType.Rock, SampleShape.Cube, _commonConfig.RockMode.SampleSize);
        }

        #endregion Rock mode

        #region Column mode

        if (mode.Equals(_modeManager.ColumnMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Column, SampleType.Ore, SampleShape.Cuboid, _commonConfig.ColumnMode.SampleSize);
        }

        #endregion Column mode

        #region Distance mode

        if (mode.Equals(_modeManager.DistanceShortMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Distance, SampleType.Ore, SampleShape.Cube, _commonConfig.DistanceMode.SampleSizeShort);
        }

        if (mode.Equals(_modeManager.DistanceMediumMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Distance, SampleType.Ore, SampleShape.Cube, _commonConfig.DistanceMode.SampleSizeMedium);
        }

        if (mode.Equals(_modeManager.DistanceLongMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Distance, SampleType.Ore, SampleShape.Cube, _commonConfig.DistanceMode.SampleSizeLong);
        }

        #endregion Distance mode

        #region Quantity mode

        if (mode.Equals(_modeManager.QuantityShortMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Quantity, SampleType.Ore, SampleShape.Cube, _commonConfig.QuantityMode.SampleSizeShort);
        }

        if (mode.Equals(_modeManager.QuantityMediumMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Quantity, SampleType.Ore, SampleShape.Cube, _commonConfig.QuantityMode.SampleSizeMedium);
        }

        if (mode.Equals(_modeManager.QuantityLongMode))
        {
            SampleArea(world, player, blockSel, SampleMode.Quantity, SampleType.Ore, SampleShape.Cube, _commonConfig.QuantityMode.SampleSizeLong);
        }

        #endregion Quantity mode

        if (DamagedBy is not null && DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
        {
            DamageItem(world, byEntity, itemSlot, damage);
        }

        return true;
    }

    private void SampleArea(
        IWorldAccessor world,
        EntityPlayer player,
        BlockSelection blockSel,
        SampleMode mode,
        SampleType type,
        SampleShape shape,
        int size
    )
    {
        var position = blockSel.Position;
        if (position is null)
        {
            return;
        }

        var sampledBlock = world.BlockAccessor.GetBlock(position);
        sampledBlock.OnBlockBroken(world, position, player.Player, 0);

        if (!sampledBlock.Attributes?[ProspectableAttributeKey].AsBool() ?? false)
        {
            return;
        }

        if (_channel is not IServerNetworkChannel serverChannel
            || world is not ServerMain serverWorld
            || player.Player is not IServerPlayer serverPlayer)
        {
            return;
        }

        var halfSize = (int)MathF.Round(size / 2.0f);
        var bottomPosition = shape is SampleShape.Cube ? position.Y - halfSize : 0;
        var topPosition = shape is SampleShape.Cube ? position.Y + halfSize : serverWorld.MapSize.Y;
        var minPosition = new Vec3i(position.X - halfSize, bottomPosition, position.Z - halfSize).ToBlockPos();
        var maxPosition = new Vec3i(position.X + halfSize, topPosition, position.Z + halfSize).ToBlockPos();

        Dictionary<string, Reading> readings = [];
        serverWorld.BlockAccessor.WalkBlocks(minPosition, maxPosition, (block, x, y, z) =>
        {
            Reading currentReading;

            switch (type)
            {
                case SampleType.Rock:
                {
                    if (!block.Variant.TryGetValue("rock", out var rockType))
                    {
                        return;
                    }

                    var rockId = $"rock-{rockType}";
                    if (readings.TryGetValue(rockId, out var reading))
                    {
                        currentReading = reading;
                        break;
                    }

                    currentReading = Reading.Create(int.MaxValue, 0, block);
                    readings.Add(rockId, currentReading);
                    break;
                }

                case SampleType.Ore:
                {
                    if (block.BlockMaterial is not EnumBlockMaterial.Ore || !block.Variant.TryGetValue("type", out var oreType))
                    {
                        return;
                    }

                    var oreId = $"ore-{oreType}";
                    if (readings.TryGetValue(oreId, out var reading))
                    {
                        currentReading = reading;
                        break;
                    }

                    currentReading = Reading.Create(int.MaxValue, 0, block);
                    readings.Add(oreId, currentReading);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var distance = (int)MathF.Round(position.DistanceTo(new Vec3i(x, y, z).ToBlockPos()));
            if (currentReading.Distance > distance)
            {
                currentReading.Distance = distance;
            }

            currentReading.Quantity += 1;
        });

        var readingPacket = new ReadingPacket
        {
            Mode = mode,
            SampleSize = size,
            Readings = readings.Values.ToArray()
        };
        serverChannel.SendPacket(readingPacket, serverPlayer);
    }
}
