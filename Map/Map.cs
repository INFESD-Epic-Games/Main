using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Background
{
    public class Map : GameObject
    {
        private Texture2D _map1;

        private Texture2D _texture;
        private Texture2D _controlsTexture;
        private const int _tileSize = 32;
        private const int _renderScale = 4;
        private const int _screenTileSize = _tileSize * _renderScale;

        private int[,] _collision =
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,1},
            {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1},
            {1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

        public override void Load(ContentManager content)
        {
            _map1 = content.Load<Texture2D>("map");
            _texture = content.Load<Texture2D>("TX Tileset Grass");
            _controlsTexture = content.Load<Texture2D>("Control_layout");
            _tilesPerRow = Math.Max(1, _texture.Width / _tileSize);

            GenerateMap();
            AddDetails();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _map1,
                Vector2.Zero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                _renderScale,
                SpriteEffects.None,
                0f
            );

            ////For debugging collisions
            // Texture2D pixel = new Texture2D(
            //     spriteBatch.GraphicsDevice,
            //     1,
            //     1);

            // pixel.SetData(new[] { Color.Red });

            // for (int x = 0; x < _collision.GetLength(1); x++)
            // {
            //     for (int y = 0; y < _collision.GetLength(0); y++)
            //     {
            //         if (_collision[y,x] == 1)
            //         {
            //             spriteBatch.Draw(
            //                 pixel,
            //                 new Rectangle(
            //                     x * _screenTileSize,
            //                     y * _screenTileSize,
            //                     _screenTileSize,
            //                     _screenTileSize),
            //                 Color.Red * 0.3f
            //             );
            //         }
            //     }
            // }

            Vector2 controlsPosition = new Vector2(
                (_mapWidth * _screenTileSize - _controlsTexture.Width * 0.75f) / 2f,
                (_mapHeight * _screenTileSize - _controlsTexture.Height * 0.75f) / 2f
            );

            spriteBatch.Draw(
                _controlsTexture, 
                controlsPosition, 
                null, 
                Color.White * 0.6f,
                0f,
                Vector2.Zero,
                0.75f,
                SpriteEffects.None,
                0f
            );

            base.Draw(gameTime, spriteBatch);
        }

        public bool IsBlocked(int x, int y)
        {
            int rows = _collision.GetLength(0);
            int cols = _collision.GetLength(1);

            if (x < 0 || y < 0 || x >= cols || y >= rows)
                return true;

            return _collision[y, x] == 1;
        }

        public Point WorldToTile(Vector2 position)
        {
            return new Point(
                (int)(position.X / _screenTileSize),
                (int)(position.Y / _screenTileSize)
            );
        }

        public bool IsColliding(Vector2 position, int width, int height)
        {
            Point topLeft = WorldToTile(position);

            Point topRight = WorldToTile(
                new Vector2(position.X + width - 1, position.Y));

            Point bottomLeft = WorldToTile(
                new Vector2(position.X, position.Y + height - 1));

            Point bottomRight = WorldToTile(
                new Vector2(position.X + width - 1,
                            position.Y + height - 1));

            return IsBlocked(topLeft.X, topLeft.Y) ||
                   IsBlocked(topRight.X, topRight.Y) ||
                   IsBlocked(bottomLeft.X, bottomLeft.Y) ||
                   IsBlocked(bottomRight.X, bottomRight.Y);
        }
    }
}