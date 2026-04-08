using System;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;

namespace SpellFall.UI;

public class MainMenu
{
    public event Action LoadGameClicked;
    public event Action NewGameClicked;
    public event Action SettingsClicked;
    public event Action QuitClicked;
    
    public bool IsVisible
    {
        get => _panel.IsVisible;
        set => _panel.IsVisible = value;
    }
    
    private Panel _panel;
    
    private Button _loadGameButton;
    private Button _newGameButton;
    private Button _settingsButton;
    private Button _quitButton;

    public void CreatePanel()
    {
        _panel = new Panel();
        _panel.Dock(Gum.Wireframe.Dock.Fill);
        _panel.AddToRoot();
        
        _loadGameButton = new Button();
        _loadGameButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _loadGameButton.X = 16;
        _loadGameButton.Y = -16 -48 -48 -48;
        _loadGameButton.Width = 128;
        _loadGameButton.Text = "Load Game";
        _loadGameButton.Click += OnLoadGameClicked;
        _panel.AddChild(_loadGameButton);
        
        _newGameButton = new Button();
        _newGameButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _newGameButton.X = 16;
        _newGameButton.Y = -16 -48 -48;
        _newGameButton.Width = 128;
        _newGameButton.Text = "New Game";
        _newGameButton.Click += OnNewGameClicked;
        _panel.AddChild(_newGameButton);
        
        _settingsButton = new Button();
        _settingsButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _settingsButton.X = 16;
        _settingsButton.Y = -16 -48;
        _settingsButton.Width = 128;
        _settingsButton.Text = "Settings";
        _settingsButton.Click += OnSettingsClicked;
        _panel.AddChild(_settingsButton);
        
        _quitButton = new Button();
        _quitButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _quitButton.X = 16;
        _quitButton.Y = -16;
        _quitButton.Width = 128;
        _quitButton.Text = "Quit";
        _quitButton.Click += OnQuitClicked;
        _panel.AddChild(_quitButton);
    }

    private void OnLoadGameClicked(object sender, EventArgs e)
    {
        LoadGameClicked?.Invoke();
    }
    
    private void OnNewGameClicked(object sender, EventArgs e)
    {
        NewGameClicked?.Invoke();
    }
    
    private void OnSettingsClicked(object sender, EventArgs e)
    {
        SettingsClicked?.Invoke();
    }
    
    private void OnQuitClicked(object sender, EventArgs e)
    {
        QuitClicked?.Invoke();
    }
}