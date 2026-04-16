using System.Linq;
using Gum.Forms;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using SpellFall.Engine;
using SpellFall.Character;
using SpellFall.Weapons;
using SpellFall.Enemies;
using SpellFall.UI;
using SpellFall.Quests;
using SpellFall.Npcs;
using SpellFall.Background;
using Microsoft.Xna.Framework.Media;

namespace SpellFall
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private static GraphicsDeviceManager _graphics;
        private static RenderManager _renderManager;
        private GameManager _gameManager;
        private KeyboardState _previousKeyboardState;
        private Npc _npc;

        private readonly MainMenu _mainMenu = new MainMenu();
        private readonly Settings _settings = new Settings();
        private readonly IntroScroll _introScroll = new IntroScroll();
        GumService GumUI => GumService.Default;

        public Game1()
        {
            DisplayMode mode = Settings.Resolutions.Last();

            _graphics = new GraphicsDeviceManager(this);
            _renderManager = new RenderManager();
            _graphics.PreferredBackBufferWidth = mode.Width;
            _graphics.PreferredBackBufferHeight = mode.Height;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize the GameManager
            _gameManager = GameManager.GetGameManager();

            // Initialize the UI
            GumUI.Initialize(this, DefaultVisualsVersion.Newest);
            InitializeInterface();

            base.Initialize();
        }

        protected void InitializeInterface()
        {
            _mainMenu.CreatePanel(Content);
            _mainMenu.NewGameClicked += () =>
            {
                _mainMenu.IsVisible = false;
                _settings.IsVisible = false;
                MediaPlayer.Stop();
                GameState.InIntro = true;
                GameState.InMainMenu = true;
                _introScroll.Reset();
            };
            _mainMenu.QuitClicked += Exit;
            _mainMenu.SettingsClicked += () =>
            {
                _mainMenu.IsVisible = false;
                _settings.IsVisible = true;
            };

            _settings.CreatePanel(Content);
            _settings.IsVisible = false;
            _settings.ResolutionChanged += (res) =>
            {
                _graphics.PreferredBackBufferWidth = res.Width;
                _graphics.PreferredBackBufferHeight = res.Height;
                GraphicalUiElement.CanvasWidth = res.Width;
                GraphicalUiElement.CanvasHeight = res.Height;
                _graphics.ApplyChanges();
            };
            _settings.FullscreenChanged += (fullscreen) =>
            {
                _graphics.IsFullScreen = fullscreen;
                GraphicalUiElement.CanvasWidth = _graphics.PreferredBackBufferWidth;
                GraphicalUiElement.CanvasHeight = _graphics.PreferredBackBufferHeight;
                _graphics.ApplyChanges();
            };
            _settings.VolumeChanged += (volume) =>
            {
                MediaPlayer.Volume = (float)volume;
            };
            _settings.ReturnClicked += () =>
            {
                _settings.IsVisible = false;
                _mainMenu.IsVisible = true;
            };
        }

        protected void InitializeGame()
        {
            // Place the player at the center of the screen
            Player player = new Player(new Point(RenderManager.VirtualWidth / 2 - 100, RenderManager.VirtualHeight / 2 - 100));
            _gameManager.Initialize(Content, this, player);
            StartingWeapon startingWeapon = new StartingWeapon();
            player.EquipWeapon(startingWeapon);

            Point npcPosition = new Point(
                player.GetPosition().Center.X + 200,
                player.GetPosition().Center.Y
            );

            _npc = new Npc(npcPosition);
            _npc.Initialize(_gameManager.QuestManager);
            _npc.SetPlayerHealthBar(player.HealthBar);


            // Add the starting objects to the GameManager
            _gameManager.AddGameObject(new Map());
            _gameManager.AddGameObject(_npc);
            _gameManager.AddGameObject(player);
            _gameManager.AddGameObject(startingWeapon);

            Point spawnerPosition = new Point(
                player.GetPosition().Center.X + 420,
                player.GetPosition().Center.Y - 180);
            _gameManager.AddGameObject(new AlienSpawner(spawnerPosition));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderManager.Initialize(GraphicsDevice);
            _introScroll.Load(Content);
            _gameManager.Load(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboard = Keyboard.GetState();

            if (GameState.IsPaused)
            {
                if (currentKeyboard.GetPressedKeyCount() > 0 && _previousKeyboardState.GetPressedKeyCount() == 0)
                {
                    _npc?.ContinueDialogue();
                }

                _previousKeyboardState = currentKeyboard;
                base.Update(gameTime);
                return;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || currentKeyboard.IsKeyDown(Keys.Escape))
                Exit();

            if (GameState.InIntro)
            {
                _introScroll.Update(gameTime);

                if (_introScroll.IsFinished)
                {
                    GameState.InIntro = false;
                    GameState.InMainMenu = false;
                    InitializeGame();
                }

                _previousKeyboardState = currentKeyboard;
                base.Update(gameTime);
                return;
            }

            GumUI.Update(gameTime);
            _gameManager.Update(gameTime);
            _previousKeyboardState = currentKeyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderManager.UpdateDestinationRect(GraphicsDevice);
            _renderManager.BeginWorld(GraphicsDevice);

            if (GameState.InIntro)
            {
                _introScroll.Draw(gameTime, _spriteBatch);
            }
            else
            {
                _gameManager.Draw(gameTime, _spriteBatch);
            }

            _renderManager.PresentWorld(GraphicsDevice, _spriteBatch);

            GumUI.Draw();
            base.Draw(gameTime);
        }

        public static Vector2 ScreenToGameCoordinates(Vector2 screenPosition)
        {
            return _renderManager.ScreenToGameCoordinates(screenPosition);
        }

        public static GraphicsDeviceManager GetGraphicsDeviceManager()
        {
            return _graphics;
        }
    }
}
