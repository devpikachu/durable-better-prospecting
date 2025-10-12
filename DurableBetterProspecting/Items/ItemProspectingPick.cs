using System.Diagnostics.CodeAnalysis;
using Common.Mod.Common.Config;
using DryIoc;
using DurableBetterProspecting.Core;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace DurableBetterProspecting.Items;

public class ItemProspectingPick : Vintagestory.GameContent.ItemProspectingPick
{
    public const string ItemRegistryId = "ItemProspectingPick";

    private const string CacheKey = "proPickToolModes";

    private static Mode? _densityMode;
    private static Mode? _nodeMode;
    private static Mode? _rockMode;
    private static Mode? _columnMode;
    private static Mode? _distanceShortMode;
    private static Mode? _distanceMediumMode;
    private static Mode? _distanceLongMode;
    private static Mode? _quantityShortMode;
    private static Mode? _quantityMediumMode;
    private static Mode? _quantityLongMode;

    private static Mode[]? _modes;
    private static SkillItem[] _skillItems = [];

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public override void OnLoaded(ICoreAPI api)
    {
        GenerateModes();
        base.OnLoaded(api);
    }

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public override void OnUnloaded(ICoreAPI api)
    {
        foreach (var skillItem in _skillItems)
        {
            skillItem.Dispose();
        }
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        return Math.Min(_skillItems.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        return _skillItems;
    }

    private static void GenerateModes()
    {
        var container = DurableBetterProspectingSystem.Instance!.Container;
        var api = container.Resolve<ICoreAPI>();
        var config = container.Resolve<IConfigSystem>().GetCommon<DurableBetterProspectingCommonConfig>();

        // TODO: Icons

        #region Mode instantiation

        _densityMode ??= Mode.Create(
            id: "density",
            name: "Density Mode (Long range, chance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _nodeMode ??= Mode.Create(
            id: "node",
            name: "Node Mode (Short range, quantity based)",
            icon: Icon.Create("game", "rocks"),
            enabled: () => config.NodeMode.Enabled
        );

        _rockMode ??= Mode.Create(
            id: "rock",
            name: "Rock Mode (Medium range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _columnMode ??= Mode.Create(
            id: "column",
            name: "Column Mode (Long range, presence based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _distanceShortMode ??= Mode.Create(
            id: "distance_short",
            name: "Distance Mode (Short range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _distanceMediumMode ??= Mode.Create(
            id: "distance_medium",
            name: "Distance Mode (Medium range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _distanceLongMode ??= Mode.Create(
            id: "distance_long",
            name: "Distance Mode (Long range, distance based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _quantityShortMode ??= Mode.Create(
            id: "quantity_short",
            name: "Quantity Mode (Short range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _quantityMediumMode ??= Mode.Create(
            id: "quantity_medium",
            name: "Quantity Mode (Medium range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _quantityLongMode ??= Mode.Create(
            id: "quantity_long",
            name: "Quantity Mode (Long range, quantity based)",
            icon: Icon.Create("game", "heatmap"),
            enabled: () => config.DensityMode.Enabled
        );

        _modes ??=
        [
            _densityMode,
            _nodeMode,
            _rockMode,
            _columnMode,
            _distanceShortMode,
            _distanceMediumMode,
            _distanceLongMode,
            _quantityShortMode,
            _quantityMediumMode,
            _quantityLongMode
        ];

        #endregion Mode instantiation

        ObjectCacheUtil.Delete(api, CacheKey);
        _skillItems = ObjectCacheUtil.GetOrCreate(api, CacheKey, () =>
        {
            return _modes
                .Where(mode => mode.Enabled.Invoke())
                .Select(mode =>
                {
                    var skillItem = new SkillItem
                    {
                        Code = mode.Id,
                        Name = mode.Name
                    };

                    if (api is not ICoreClientAPI clientApi)
                    {
                        return skillItem;
                    }

                    skillItem.WithIcon(clientApi, mode.Icon.Load(clientApi));
                    skillItem.TexturePremultipliedAlpha = false;

                    return skillItem;
                })
                .ToArray();
        });
    }
}
