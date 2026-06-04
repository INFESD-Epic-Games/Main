using System;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using SpellFall.UI.Fluent;

namespace SpellFall.UI
{
    public class PauseMenu
    {
        public event Action ResumeClicked;
        public event Action MainMenuClicked;
        public event Action SettingsClicked;
        public event Action QuitClicked;

        public bool IsVisible
        {
            get => _panel.IsVisible;
            set => _panel.IsVisible = value;
        }

        private Panel _panel;

        private FluentButton _resumeButton;
        private FluentButton _mainMenuButton;
        private FluentButton _settingsButton;
        private FluentButton _quitButton;

        public void CreatePanel(ContentManager content)
        {
            Texture2D buttonTexture = content.Load<Texture2D>("brown");

            _panel = new Panel();
            _panel.Dock(Dock.Fill);
            _panel.AddToRoot();

            const float startX = 16;
            const float spacing = 48;

            _resumeButton = new FluentButton(buttonTexture)
                .WithText("Resume")
                .WithFont("Test.fnt")
                .Anchored(Anchor.BottomLeft)
                .At(startX, -16 - (spacing * 3))
                .OnClick(() => ResumeClicked?.Invoke())
                .AddTo(_panel);

            _mainMenuButton = new FluentButton(buttonTexture)
                .WithText("Main Menu")
                .WithFont("Test.fnt")
                .Anchored(Anchor.BottomLeft)
                .At(startX, -16 - (spacing * 2))
                .OnClick(() => MainMenuClicked?.Invoke())
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
}