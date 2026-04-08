using System.Linq;
using Gum.Forms;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
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
using SpellFall.Character;
using SpellFall.Background;

namespace SpellFall
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private static GraphicsDeviceManager _graphics;
        private GameManager _gameManager;
        
        private readonly MainMenu _mainMenu = new MainMenu();
        private readonly Settings _settings = new Settings();
        
        GumService GumUI => GumService.Default;
        private Npc npc;
        private QuestManager questManager;

        public Game1()
        {
            DisplayMode mode = Settings.Resolutions.Last();
            
            _graphics = new GraphicsDeviceManager(this);
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
            _mainMenu.CreatePanel();
            _mainMenu.NewGameClicked += () =>
            {
                GameState.InMainMenu = false;
                _mainMenu.IsVisible = false;
                InitializeGame();
            };
            _mainMenu.QuitClicked += Exit;
            _mainMenu.SettingsClicked += () =>
            {
                _mainMenu.IsVisible = false;
                _settings.IsVisible = true;
            };

            _settings.CreatePanel();
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
            _settings.ReturnClicked += () =>
            {
                _settings.IsVisible = false;
                _mainMenu.IsVisible = true;
            };
        }

        protected void InitializeGame()
        {
            // Place the player at the center of the screen
            Player player = new Player(new Point(GraphicsDevice.Viewport.Width/2 - 100, GraphicsDevice.Viewport.Height/2 - 100));
            StartingWeapon startingWeapon = new StartingWeapon();
            player.EquipWeapon(startingWeapon);

            questManager = new QuestManager();

            Point npcPosition = new Point(
                player.GetPosition().Center.X + 200,
                player.GetPosition().Center.Y
            );

            npc = new Npc(npcPosition);
            npc.Initialize(questManager);
            npc.SetPlayerHealthBar(player.HealthBar);

            // Voeg toe aan game
            _gameManager.AddGameObject(npc);

            // Add the starting objects to the GameManager
            _gameManager.Initialize(Content, this, player);
            _gameManager.AddGameObject(new Map());
            _gameManager.AddGameObject(player);
            _gameManager.AddGameObject(startingWeapon);

            // Dit is puur voor testen, en ben er niet trots op.
            Vector2 playerCenter = player.GetPosition().Center.ToVector2();
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset;
                float distance;

                do
                {
                    float x = _gameManager.RNG.Next(-800, 800);
                    float y = _gameManager.RNG.Next(-800, 800);
                    offset = new Vector2(x, y);
                    distance = offset.Length();
                }
                while (distance < 600f || distance > 800f);

                Point spawnPoint = (playerCenter + offset).ToPoint();

                _gameManager.AddGameObject(new Alien(spawnPoint));
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameManager.Load(Content);
        }

        protected override void Update(GameTime gameTime)
        {
    
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            GumUI.Update(gameTime);
            _gameManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GumUI.Draw();
            _gameManager.Draw(gameTime, _spriteBatch);
            base.Draw(gameTime);
        }

        public static GraphicsDeviceManager GetGraphicsDeviceManager()
        {
            return _graphics;
        }

    }
}
