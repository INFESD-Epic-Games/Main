using Gum.Converters;
using Gum.DataTypes;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;

namespace SpellFall.UI;

public class Inventory
{
    private readonly NineSliceRuntime _background;

    public Inventory(Texture2D texture, int margin = 48)
    {
        _background = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
            
            XUnits = GeneralUnitType.PixelsFromMiddle,
            YUnits = GeneralUnitType.PixelsFromMiddle,
            XOrigin = HorizontalAlignment.Center,
            YOrigin = VerticalAlignment.Center,
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = -margin * 2,
            Height = -margin * 2,
            
            Visible = false,
        };
        
        _background.AddToManagers();
    }

    public void Show(bool visible = true)
    {
        _background.Visible = visible;
    }
}