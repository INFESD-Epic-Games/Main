using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpellFall.Engine;
using SpellFall.Collision;
using SpellFall.Character;
using Microsoft.Xna.Framework.Content;
using SpellFall.Weapons;
using SpellFall.Enemies;

namespace SpellFall
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private static GraphicsDeviceManager _graphics;
        private GameManager _gameManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;

            // Set the size of the screen
            _graphics.PreferredBackBufferWidth = 2000;
            _graphics.PreferredBackBufferHeight = 1200;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //Initialize the GameManager
            _gameManager = GameManager.GetGameManager();
            base.Initialize();

            // Place the player at the center of the screen
            Player player = new Player(new Point(GraphicsDevice.Viewport.Width/2 - 100, GraphicsDevice.Viewport.Height/2 - 100));
            StartingWeapon startingWeapon = new StartingWeapon();
            player.EquipWeapon(startingWeapon);

            // Add the starting objects to the GameManager
            _gameManager.Initialize(Content, this, player);
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
            _gameManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _gameManager.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }

        public static GraphicsDeviceManager GetGraphicsDeviceManager()
        {
            return _graphics;
        }

    }
}
