using System;
using System.Drawing;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using SpellFall.UI.Fluent;

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

    private FluentButton _loadGameButton;
    private FluentButton _newGameButton;
    private FluentButton _settingsButton;
    private FluentButton _quitButton;
    public Song _backgroundmusic;

    public void CreatePanel(ContentManager content)
    {
        Texture2D buttonTexture = content.Load<Texture2D>("brown");
        _backgroundmusic = content.Load<Song>("Title Theme");

        MediaPlayer.Play(_backgroundmusic);

        _panel = new Panel();
        _panel.Dock(Dock.Fill);
        _panel.AddToRoot();

        const float startX = 16;
        const float spacing = 48;

        ///
        // The Load Game button is currently disabled, as the save/load system is not yet implemented.
        ///
        // _loadGameButton = new FluentButton(buttonTexture)
        //     .WithText("Load Game")
        //     .WithFont("Test.fnt")
        //     .Anchored(Anchor.BottomLeft)
        //     .At(startX, -16 - (spacing * 3))
        //     .OnClick(() => LoadGameClicked?.Invoke())
        //     .AddTo(_panel);

        _newGameButton = new FluentButton(buttonTexture)
            .WithText("New Game")
            .WithFont("Test.fnt")
            .Anchored(Anchor.BottomLeft)
            .At(startX, -16 - (spacing * 2))
            .OnClick(() => NewGameClicked?.Invoke())
            .AddTo(_panel);

        _settingsButton = new FluentButton(buttonTexture)
            .WithText("Settings")
            .WithFont("Test.fnt")
            .Anchored(Anchor.BottomLeft)
            .At(startX, -16 - spacing)
            .OnClick(() => SettingsClicked?.Invoke())
            .AddTo(_panel);

        _quitButton = new FluentButton(buttonTexture)
            .WithText("Quit")
            .WithFont("Test.fnt")
            .Anchored(Anchor.BottomLeft)
            .At(startX, -16)
            .OnClick(() => QuitClicked?.Invoke())
            .AddTo(_panel);
    }
}