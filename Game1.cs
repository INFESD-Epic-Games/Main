using System.Linq;
using System;
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
using SpellFall.Items;
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
        private Firebow _firebow;
        private Icebow _icebow;
        private Earthbow _earthbow;
        private Poisonbow _poisonbow;
        private KeyboardState _previousKeyboardState;
        private bool _settingsOpenedFromPause;
        private Texture2D _pauseOverlayTexture;

        private readonly MainMenu _mainMenu = new MainMenu();
        private readonly Settings _settings = new Settings();
        private readonly IntroScroll _introScroll = new IntroScroll();
        private readonly GameOverScreen _gameOverScreen = new GameOverScreen();
        private readonly PauseMenu _pauseMenu = new PauseMenu();
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
                _settingsOpenedFromPause = false;
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
                if (_settingsOpenedFromPause)
                {
                    _pauseMenu.IsVisible = true;
                }
                else
                {
                    _mainMenu.IsVisible = true;
                }

                _settingsOpenedFromPause = false;
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
            _firebow = new Firebow();
            _icebow = new Icebow();
            _earthbow = new Earthbow();
            _poisonbow = new Poisonbow();

            // Starting weapon is Common; other weapons are unlocked through loot.
            _startingWeapon.ApplyLootTier("Common");

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
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
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
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1,1},
                {1,0,0,1,1,0,0,0,0,0,0,1,0,0,0,0,1,1},
                {1,0,0,1,1,0,0,0,0,0,0,0,0,1,0,0,0,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,0,0,0,1,0,0,0,0,0,0,1,1,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,1},
                {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            int[,] map3Collision =
            {
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
                {1,0,0,0,1,1,0,0,0,0,0,0,1,0,0,0,0,1},
                {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1,0,1},
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
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

            Map map3 = new Map(
                "map (2)",
                new Vector2(4608,0),
                map3Collision
            );

            _gameManager.Maps.Add(map1);
            _gameManager.Maps.Add(map2);
            _gameManager.Maps.Add(map3);

            _gameManager.CurrentMap = map1;

            _gameManager.AddGameObject(map1);
            _gameManager.AddGameObject(map2);
            _gameManager.AddGameObject(map3);
            Gate roomGate = new Gate(
                new Rectangle(
                    2100,
                    820,
                    128,
                    256
                ),
                map1
            );
             Gate roomGate2 = new Gate(
                new Rectangle(
                    4400,
                    820,
                    128,
                    256
                ),
                map2
            );

            _gameManager.AddGameObject(roomGate);
            _gameManager.AddGameObject(roomGate2);
            // Add the starting objects to the GameManager
            _gameManager.AddGameObject(_npc);
            _gameManager.AddGameObject(_player);
            _gameManager.AddGameObject(_startingWeapon);
            _gameManager.AddGameObject(_lbow);
            _gameManager.AddGameObject(_firebow);
            _gameManager.AddGameObject(_icebow);
            _gameManager.AddGameObject(_earthbow);
            _gameManager.AddGameObject(_poisonbow);
            _gameManager.AddGameObject(new EnemyClearChestTestSpawner());
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pauseOverlayTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pauseOverlayTexture.SetData(new[] { Color.White });
            _renderManager.Initialize(GraphicsDevice);
            _introScroll.Load(Content);
            _gameOverScreen.Load(Content);
            _gameManager.Load(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboard = Keyboard.GetState();
            bool escapePressed = currentKeyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape);

            if (GameState.IsPaused)
            {
                bool isInteractKeyPressed = (currentKeyboard.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E)) || 
                                (currentKeyboard.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space)) || 
                                (currentKeyboard.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter));

                if (escapePressed)
                {
                    if (_settings.IsVisible)
                    {
                        _settings.IsVisible = false;
                        GameState.InPauseMenu = true;
                        _pauseMenu.IsVisible = true;
                    }
                    else
                    {
                        GameState.IsPaused = false;
                        GameState.InPauseMenu = false;
                        _pauseMenu.IsVisible = false;
                    }
                }

                if (isInteractKeyPressed)
                {
                    _npc?.ContinueDialogue();
                }

                GumUI.Update(gameTime);
                _previousKeyboardState = currentKeyboard;
                base.Update(gameTime);
                return;
            }

            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || escapePressed) && !GameState.InMainMenu)
                // Exit();
                PauseGame();

            // if (currentKeyboard.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            //     PauseGame();

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

            if (currentKeyboard.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B))
            {
                (_gameManager.Player ?? _player)?.UnlockAllWeapons();
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
            Player activePlayer = _gameManager.Player ?? _player;

            if (activePlayer == null)
            {
                return;
            }

            Keys[] slotKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6 };
            int maxSlots = Math.Min(slotKeys.Length, activePlayer.OwnedWeaponCount);

            for (int slot = 1; slot <= maxSlots; slot++)
            {
                Keys key = slotKeys[slot - 1];
                bool keyPressed = currentKeyboard.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
                if (!keyPressed)
                {
                    continue;
                }

                activePlayer.EquipWeaponBySlot(slot);

                break;
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

            if (GameState.IsPaused)
            {
                _spriteBatch.Begin();
                if (GameState.InPauseMenu)
                {
                    _spriteBatch.Draw(
                        _pauseOverlayTexture,
                        new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                        Color.Black * 0.65f
                    );
                }
                _spriteBatch.End();
            }

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

        private void PauseGame()
        {
            GameState.IsPaused = true;
            GameState.InPauseMenu = true;

            _pauseMenu.CreatePanel(Content);
            _pauseMenu.ResumeClicked += () =>
            {
                GameState.IsPaused = false;
                GameState.InPauseMenu = false;
                _pauseMenu.IsVisible = false;
            };
            _pauseMenu.MainMenuClicked += () =>
            {
                GameState.IsPaused = false;
                GameState.InPauseMenu = false;
                GameState.InMainMenu = true;
                GameState.InIntro = false;
                MediaPlayer.Stop();
                _gameManager.ClearWorldState();
                _pauseMenu.IsVisible = false;
                _mainMenu.IsVisible = true;
            };
            _pauseMenu.SettingsClicked += () =>
            {
                _settingsOpenedFromPause = true;
                GameState.InPauseMenu = true;
                _pauseMenu.IsVisible = false;
                _settings.IsVisible = true;
            };
            _pauseMenu.QuitClicked += Exit;

        }
    }
}
