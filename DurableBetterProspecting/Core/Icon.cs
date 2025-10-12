using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DurableBetterProspecting.Core;

public record Icon
{
    public required string Domain { get; init; }
    public required string Name { get; init; }

    public static Icon Create(string domain, string name)
    {
        return new Icon
        {
            Domain = domain,
            Name = name
        };
    }

    public static Icon Create(string name)
    {
        return new Icon
        {
            Domain = DurableBetterProspectingSystem.Instance!.ModId(),
            Name = name
        };
    }

    public LoadedTexture Load(ICoreClientAPI api, int width = 48, int height = 48, int padding = 5, int color = ColorUtil.WhiteArgb)
    {
        return api.Gui.LoadSvgWithPadding(
            loc: new AssetLocation(Domain, $"textures/icons/{Name}.svg"),
            textureWidth: width,
            textureHeight: height,
            padding: padding,
            color: color
        );
    }
}
