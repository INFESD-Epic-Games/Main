using System;
using System.Collections.Generic;
using System.Linq;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;

namespace SpellFall.UI;

public class Settings
{
    public event Action<DisplayMode> ResolutionChanged;
    public event Action<bool> FullscreenChanged;
    public event Action ReturnClicked;
    
    public bool IsVisible
    {
        get => _panel.IsVisible;
        set => _panel.IsVisible = value;
    }
    
    private Panel _panel;

    private ComboBox _resolutionSelect;
    private CheckBox _fullscreenToggle;
    
    private Button _returnButton;

    public static readonly List<DisplayMode> Resolutions = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
        .Distinct()
        .Where(m => m.Height >= 480)
        .OrderByDescending(m => m.Width)
        .ToList();

    public void CreatePanel()
    {
        _panel = new Panel();
        _panel.Dock(Gum.Wireframe.Dock.Fill);
        _panel.AddToRoot();
        
        _resolutionSelect = new ComboBox();
        _resolutionSelect.Anchor(Gum.Wireframe.Anchor.Center);
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
        _fullscreenToggle.Anchor(Gum.Wireframe.Anchor.Center);
        _fullscreenToggle.X = 0;
        _fullscreenToggle.Y = 16 +16;
        _fullscreenToggle.Width = 128;
        _fullscreenToggle.Text = "Fullscreen";
        _fullscreenToggle.Click += OnFullscreenChanged;
        _panel.AddChild(_fullscreenToggle);
        
        _returnButton = new Button();
        _returnButton.Anchor(Gum.Wireframe.Anchor.Bottom);
        _returnButton.X = 0;
        _returnButton.Y = -16;
        _returnButton.Width = 128;
        _returnButton.Text = "Return";
        _returnButton.Click += OnReturnClicked;
        _panel.AddChild(_returnButton);
    }
    
    private void OnResolutionChanged(object sender, SelectionChangedEventArgs e)
    {
        ResolutionChanged?.Invoke(Resolutions[_resolutionSelect.SelectedIndex]);
    }
    
    private void OnFullscreenChanged(object sender, EventArgs e)
    {
        FullscreenChanged?.Invoke(_fullscreenToggle.IsChecked ?? false);
    }

    private void OnReturnClicked(object sender, EventArgs e)
    {
        ReturnClicked?.Invoke();
    }

}