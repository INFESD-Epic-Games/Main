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
    private readonly int _baseWidth;
    private readonly int _baseHeight;
    private float _baseFontScale;
    private float _hoverScaleMultiplier;
    
    private readonly NineSliceRuntime _background;
    private readonly TextRuntime _textElement;

    public FluentButton(Texture2D texture)
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
        };

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
        
        _baseWidth = 128;
        _baseHeight = 40;
        _baseFontScale = 1.0f;
        _hoverScaleMultiplier = 1.05f;
        
        Width = _baseWidth;
        Height = _baseHeight;

        Visual.RollOver += (_, _) =>
        {
            Width = _baseWidth * _hoverScaleMultiplier;
            Height = _baseHeight * _hoverScaleMultiplier;
            _textElement.FontScale = _baseFontScale * _hoverScaleMultiplier;
        };
        Visual.RollOff += (_, _) =>
        {
            Width = _baseWidth;
            Height = _baseHeight;
            _textElement.FontScale = _baseFontScale;
        };
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
        
        _baseFontScale = scaling;
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