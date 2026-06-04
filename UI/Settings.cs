using System;
using System.Collections.Generic;
using System.Linq;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using SpellFall.UI.Fluent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SpellFall.UI;

public class Settings
{
    public event Action<DisplayMode> ResolutionChanged;
    public event Action<bool> FullscreenChanged;
    public event Action<double> VolumeChanged;

    public event Action<double> SFXchanged;
    public event Action ReturnClicked;
    
    public bool IsVisible
    {
        get => _panel.IsVisible;
        set => _panel.IsVisible = value;
    }
    
    private Panel _panel;

    private ComboBox _resolutionSelect;
    private CheckBox _fullscreenToggle;
    private Slider _volumeSlider;
    private Slider _sfx;
    private Slider _music;
    private FluentButton _returnButton;

    public static readonly List<DisplayMode> Resolutions = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
        .Distinct()
        .Where(m => m.Height >= 480)
        .Where(IsVirtualAspectRatio)
        .OrderByDescending(m => m.Width)
        .ToList();

    private static bool IsVirtualAspectRatio(DisplayMode mode)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS reports all modes as 16:10, even if they are actually 16:9, so we need to check the actual aspect ratio
            return mode.Width * 10 == mode.Height * 16 || mode.Width * 9 == mode.Height * 16;
        }
        else
        {
            return mode.Width * 9 == mode.Height * 16;
        }
    }

    public void CreatePanel(ContentManager content)
    {
        Texture2D buttonTexture = content.Load<Texture2D>("brown");
        
        _panel = new Panel();
        _panel.Dock(Gum.Wireframe.Dock.Fill);
        _panel.AddToRoot();
        
        _resolutionSelect = new ComboBox();
        _resolutionSelect.Anchor(Anchor.Center);
        _resolutionSelect.X = 0;
        _resolutionSelect.Y = 0;
        _resolutionSelect.Width = 128;
        _resolutionSelect.Items = Resolutions
            .Select(res => $"{res.Width}x{res.Height}")
            .ToList();
        _resolutionSelect.SelectedIndex = _resolutionSelect.Items.Count - 1;
        _resolutionSelect.SelectionChanged += OnResolutionChanged;
        _panel.AddChild(_resolutionSelect);
        
        _fullscreenToggle = new CheckBox();
        _fullscreenToggle.Anchor(Anchor.Center);
        _fullscreenToggle.X = 0;
        _fullscreenToggle.Y = 16 +16;
        _fullscreenToggle.Width = 128;
        _fullscreenToggle.Text = "Fullscreen";
        _fullscreenToggle.Click += OnFullscreenChanged;
        _panel.AddChild(_fullscreenToggle);
        
        var label = new Label();
        label.Anchor(Anchor.Center);
        label.X = 0;
        label.Y = 16 + 48;
        label.Text = "Music:";
        _panel.AddChild(label);

        _volumeSlider = new Slider();
        _volumeSlider.Anchor(Anchor.Center);
        _volumeSlider.X = 0;
        _volumeSlider.Y = 16 + 64 + 10;
        _volumeSlider.Width = 128;
        _volumeSlider.Value = 100;
        _volumeSlider.Minimum = 0;
        _volumeSlider.Maximum = 100;
        _volumeSlider.IsSnapToTickEnabled = true;
        _volumeSlider.TicksFrequency = 1;
        _volumeSlider.ValueChanged += OnVolumeChanged;
        _panel.AddChild(_volumeSlider);
        
        var label2 = new Label();
        label2.Anchor(Anchor.Center);
        label2.X = 0;
        label2.Y = 16 + 104;
        label2.Text = "SFX:";
        _panel.AddChild(label2);

        _sfx = new Slider();
        _sfx.Anchor(Anchor.Center);
        _sfx.X = 0;
        _sfx.Y = 16 + 120 + 10;
        _sfx.Width = 128;
        _sfx.Value = 100;
        _sfx.Minimum = 0;
        _sfx.Maximum = 100;
        _sfx.IsSnapToTickEnabled = true;
        _sfx.TicksFrequency = 1;
        _sfx.ValueChanged += onSFXChanged;
        _panel.AddChild(_sfx);

        _returnButton = new FluentButton(buttonTexture)
            .WithText("Return")
            .WithFont("Test.fnt")
            .Anchored(Anchor.Bottom)
            .At(0, -16)
            .OnClick(() => ReturnClicked?.Invoke())
            .AddTo(_panel);
    }
    
    private void OnResolutionChanged(object sender, SelectionChangedEventArgs e)
    {
        ResolutionChanged?.Invoke(Resolutions[_resolutionSelect.SelectedIndex]);
    }
    
    private void OnFullscreenChanged(object sender, EventArgs e)
    {
        FullscreenChanged?.Invoke(_fullscreenToggle.IsChecked ?? false);
    }

    private void OnVolumeChanged(object sender, EventArgs e)
    {
        VolumeChanged?.Invoke(_volumeSlider.Value / 100f);
    }

    private void onSFXChanged(object sender, EventArgs e)
    {
        SFXchanged?.Invoke(_sfx.Value / 100f);
    }
}