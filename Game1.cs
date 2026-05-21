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
        private Npc _npc;
        private KeyboardState _previousKeyboardState;

        private readonly MainMenu _mainMenu = new MainMenu();
        private readonly Settings _settings = new Settings();
        private readonly IntroScroll _introScroll = new IntroScroll();
        private readonly GameOverScreen _gameOverScreen = new GameOverScreen();
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

            _gameOverScreen.RestartRequested += () =>
            {
                GameState.InGameOver = false;
                GameState.IsPaused = false;
                GameState.InMainMenu = false;
                GameState.InIntro = false;
                _mainMenu.IsVisible = false;
                _settings.IsVisible = false;
                InitializeGame();
            };

            _gameOverScreen.ReturnToMenuRequested += () =>
            {
                GameState.InGameOver = false;
                GameState.IsPaused = false;
                GameState.InMainMenu = true;
                GameState.InIntro = false;
                MediaPlayer.Stop();
                _gameManager.ClearWorldState();
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
            _npc.Initialize(_gameManager.QuestManager, () =>
            {
                _gameManager.AddGameObject(AlienSpawner.CreateQuestSpawner());
            });
            _npc.SetPlayerHealthBar(player.HealthBar);
            
            // Add the starting objects to the GameManager
            _gameManager.AddGameObject(new Map());
            _gameManager.AddGameObject(_npc);
            _gameManager.AddGameObject(player);
            _gameManager.AddGameObject(startingWeapon);
            // Spawn the projectile enemy near the player so it stays on the playable area.
            Point projectileEnemyPosition = new Point(
                player.GetPosition().Center.X + 300,
                player.GetPosition().Center.Y - 100
            );
            _gameManager.AddGameObject(new Goblin(projectileEnemyPosition));
            
            _gameManager.AddGameObject(new WeepingAngel(new Point(0, 0)));
            _gameManager.AddGameObject(new BigElite(new Point(-50, -50)));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderManager.Initialize(GraphicsDevice);
            _introScroll.Load(Content);
            _gameOverScreen.Load(Content);
            _gameManager.Load(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboard = Keyboard.GetState();

            if (GameState.IsPaused)
            {
                bool enterPressed = currentKeyboard.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);

                if (enterPressed)
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

            if (GameState.InGameOver)
            {
                _gameOverScreen.Update(gameTime);
                _previousKeyboardState = currentKeyboard;
                base.Update(gameTime);
                return;
            }

            GumUI.Update(gameTime);
            _gameManager.Update(gameTime);

            if (_gameManager.Player?.HealthBar.currentHealth <= 0 && !GameState.InMainMenu)
            {
                GameState.InGameOver = true;
                GameState.IsPaused = false;
                MediaPlayer.Stop();
                _gameOverScreen.ResetInputState();
                _previousKeyboardState = currentKeyboard;
                base.Update(gameTime);
                return;
            }

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
            else if (GameState.InMainMenu)
            {
                // Keep the world hidden while menu UI is active.
            }
            else
            {
                _gameManager.Draw(gameTime, _spriteBatch);

                if (GameState.InGameOver)
                {
                    _gameOverScreen.Draw(gameTime, _spriteBatch);
                }
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
