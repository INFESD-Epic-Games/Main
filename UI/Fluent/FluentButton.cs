using System;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;

namespace SpellFall.UI.Fluent;

public class FluentButton : Button
{
    private readonly NineSliceRuntime _background;
    private readonly TextRuntime _textElement;

    public FluentButton(Texture2D texture)
    {
        // 1. Initialize NineSlice Background
        _background = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
        };

        // 2. Initialize Text Element
        _textElement = new TextRuntime
        {
            HasEvents = false,
            Name = "TextInstance",
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            Width = 0,
            Height = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _background.Children.Add(_textElement);
        Visual = _background;

        Width = 128;
        Height = 40;

        GotFocus += (_, _) => _background.Color = new Color(220, 220, 220);
        LostFocus += (_, _) => _background.Color = Color.White;
        Push += (_, _) => _background.Color = Color.Gray;
        Click += (_, _) => _background.Color = Color.White;
    }

    // --- Builder Methods ---

    public FluentButton WithText(string text, Color? color = null)
    {
        Text = text;
        if (color.HasValue) _textElement.Color = color.Value;
         return this;
    }
    
    public FluentButton WithFont(string fontName, float scaling = 0.375f)
    {
        _textElement.UseCustomFont = true;
        _textElement.FontScale = scaling;
        _textElement.CustomFontFile = fontName; 
    
        return this;
    }

    public FluentButton At(float x, float y)
    {
        X = x;
        Y = y;
        return this;
    }

    public FluentButton Size(float width, float height)
    {
        Width = width;
        Height = height;
        return this;
    }

    public FluentButton Anchored(Anchor anchor)
    {
        Visual.Anchor(anchor);
        return this;
    }

    public FluentButton AlignText(HorizontalAlignment h, VerticalAlignment v = VerticalAlignment.Center)
    {
        _textElement.HorizontalAlignment = h;
        _textElement.VerticalAlignment = v;
        return this;
    }

    public FluentButton OnClick(Action action)
    {
        Click += (_, _) => action?.Invoke();
        return this;
    }

    public FluentButton AddTo(Panel container)
    {
        container.AddChild(this);
        return this;
    }
}