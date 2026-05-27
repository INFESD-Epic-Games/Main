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
        private Player _player;
        private StartingWeapon _startingWeapon;
        private Lbow _lbow;
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
            int spawnTileX = 8;
            int spawnTileY = 8;

            int tileWorldSize = 32 * 4;

            _player = new Player(
                new Point(
                    spawnTileX * tileWorldSize + tileWorldSize / 2,
                    spawnTileY * tileWorldSize + tileWorldSize / 2
                )
            );
            _gameManager.Initialize(Content, this, _player);
            StartingWeapon startingWeapon = new StartingWeapon();
            _player.EquipWeapon(startingWeapon);
            _player = new Player(new Point(RenderManager.VirtualWidth / 2 - 100, RenderManager.VirtualHeight / 2 - 100));
            _gameManager.Initialize(Content, this, _player);
            _startingWeapon = new StartingWeapon();
            _lbow = new Lbow();
            _player.EquipWeapon(_startingWeapon);

            Point npcPosition = new Point(
                _player.GetPosition().Center.X + 200,
                _player.GetPosition().Center.Y
            );

            _npc = new Npc(npcPosition);
            _npc.Initialize(_gameManager.QuestManager, () =>
            {
                Point projectileEnemyPosition = new Point(
                _player.GetPosition().Center.X + 300,
                _player.GetPosition().Center.Y - 100
                );
                _gameManager.AddGameObject(new Goblin(projectileEnemyPosition));

                Point bishopPosition = new Point(
                    _player.GetPosition().Center.X + 500,
                    _player.GetPosition().Center.Y + 100
                );
                _gameManager.AddGameObject(new Bishop(bishopPosition));

                _gameManager.AddGameObject(new WeepingAngel(new Point(
                    _player.GetPosition().Center.X + 200,
                    _player.GetPosition().Center.Y + 100
                )));
                _gameManager.AddGameObject(new Eye(new Point(
                    _player.GetPosition().Center.X + 200,
                    _player.GetPosition().Center.Y + 150
                )));

                Point enemyPosition = new Point(
                    _player.GetPosition().Center.X + 300,
                    _player.GetPosition().Center.Y - 100
                );
                _gameManager.AddGameObject(new Ghost(enemyPosition));
            });
            _npc.SetPlayerHealthBar(_player.HealthBar);

            int[,] map1Collision =
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,1},
                {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };
            int[,] map2Collision =
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,1},
                {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            Map map1 = new Map(
                "map",
                Vector2.Zero,
                map1Collision
            );

            Map map2 = new Map(
                "map (1)",
                new Vector2(2304,0),
                map2Collision
            );

            _gameManager.Maps.Add(map1);
            _gameManager.Maps.Add(map2);

            _gameManager.CurrentMap = map1;

            _gameManager.AddGameObject(map1);
            _gameManager.AddGameObject(map2);
            Gate roomGate = new Gate(
                new Rectangle(
                    2100,
                    820,
                    128,
                    256
                ),
                map1
            );

            _gameManager.AddGameObject(roomGate);
            // Add the starting objects to the GameManager
            _gameManager.AddGameObject(_npc);
            _gameManager.AddGameObject(_player);
            _gameManager.AddGameObject(_startingWeapon);
            _gameManager.AddGameObject(_lbow);
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
                bool ePressed = currentKeyboard.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E);

                if (ePressed)
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

            HandleWeaponSwitch(currentKeyboard);

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

        private void HandleWeaponSwitch(KeyboardState currentKeyboard)
        {
            if (_player == null)
            {
                return;
            }

            bool onePressed = currentKeyboard.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1);
            bool twoPressed = currentKeyboard.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2);

            if (onePressed && _startingWeapon != null)
            {
                _player.EquipWeapon(_startingWeapon);
            }
            else if (twoPressed && _lbow != null)
            {
                _player.EquipWeapon(_lbow);
            }
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
