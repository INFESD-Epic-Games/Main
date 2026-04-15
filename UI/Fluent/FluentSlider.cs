using System;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;

namespace SpellFall.UI.Fluent;

public class FluentSlider : Slider
{
    private readonly ContainerRuntime _container;
    private readonly NineSliceRuntime _track;
    private readonly NineSliceRuntime _thumb;
    private readonly TextRuntime _titleLabel;
    private readonly TextRuntime _valueLabel;

    private float _baseFontScale = 0.375f;

    public FluentSlider(Texture2D trackTex, Texture2D thumbTex)
    {
        _container = new ContainerRuntime();

        // --- The Track ---
        _track = new NineSliceRuntime
        {
            Name = "TrackInstance",
            Texture = trackTex,
            // Make it fill the width of the Slider
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            Width = 0,
            Height = 12,
            // Center it vertically within the 40px height of the slider
            YOrigin = VerticalAlignment.Center,
            Y = 0,
            TextureWidth = trackTex.Width,
            TextureHeight = trackTex.Height
        };

        // --- The Thumb ---
        _thumb = new NineSliceRuntime
        {
            Name = "ThumbInstance",
            Texture = thumbTex,
            Width = 24,
            Height = 24,
            Y = 0,
            TextureWidth = thumbTex.Width,
            TextureHeight = thumbTex.Height
        };

        // --- Labels ---
        _titleLabel = new TextRuntime
        {
            Name = "TitleLabel",
            Y = -25, // Appears above the track
            WidthUnits = DimensionUnitType.RelativeToParent,
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        _valueLabel = new TextRuntime
        {
            Name = "ValueLabel",
            // Position it relative to the RIGHT of the container
            XOrigin = HorizontalAlignment.Left,
            XUnits = (GeneralUnitType)PositionUnitType.PixelsFromRight,
            X = 10,
            Width = 40,
            VerticalAlignment = VerticalAlignment.Center
        };

        // --- Assembly ---
        _container.Children.Add(_track);
        _track.Children.Add(_thumb);
        _container.Children.Add(_titleLabel);
        _container.Children.Add(_valueLabel);

        // Set Visual last so Gum finds TrackInstance/ThumbInstance
        Visual = _container;

        // --- Default Dimensions ---
        this.Width = 200;
        this.Height = 40;
        this.Minimum = 0;
        this.Maximum = 100;

        this.ValueChanged += (_, _) => UpdateValueText();
    }

    private void UpdateValueText()
    {
        _valueLabel.Text = Value.ToString("0");
    }

    // --- Builder Methods ---

    public FluentSlider WithRange(float min, float max, float? step = null)
    {
        Minimum = min;
        Maximum = max;
        if (step.HasValue)
        {
            IsSnapToTickEnabled = true;
            TicksFrequency = step.Value;
        }

        UpdateValueText();
        return this;
    }

    public FluentSlider WithValue(float value)
    {
        Value = value;
        UpdateValueText();
        return this;
    }

    public FluentSlider WithLabel(string text)
    {
        _titleLabel.Text = text;
        return this;
    }

    public FluentSlider WithFont(string fontName, float scaling = 0.375f)
    {
        _baseFontScale = scaling;

        _titleLabel.UseCustomFont = true;
        _titleLabel.CustomFontFile = fontName;
        _titleLabel.FontScale = scaling;

        _valueLabel.UseCustomFont = true;
        _valueLabel.CustomFontFile = fontName;
        _valueLabel.FontScale = scaling;

        return this;
    }

    public FluentSlider Size(float width, float height)
    {
        Width = width;
        Height = height;
        return this;
    }
    
    public FluentSlider Anchored(Anchor anchor)
    {
        Visual.Anchor(anchor);
        return this;
    }

    public FluentSlider At(float x, float y)
    {
        X = x;
        Y = y;
        return this;
    }

    public FluentSlider OnValueChanged(Action<float> action)
    {
        ValueChanged += (_, _) => action?.Invoke((float)Value);
        return this;
    }

    public FluentSlider AddTo(Panel container)
    {
        container.AddChild(this);
        return this;
    }
}