using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Background
{
    public class Map : GameObject
    {
        private Texture2D _texture;
        private const int _tileSize = 32;
        private const int _renderScale = 4;
        private const int _screenTileSize = _tileSize * _renderScale;
        private const int _mapWidth = 16;
        private const int _mapHeight = 16;
        private int[,] _map = new int[_mapWidth, _mapHeight];
        private int _tilesPerRow;
        private Random _random = new Random();  
        private GameManager _gameManager;
        private Texture2D _textureBoundaries;
        private Texture2D _textureStoneWall;
        private Texture2D _texturePlantShadows;
        private Texture2D _propsShadows;
        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("TX Tileset Grass");
            _textureBoundaries = content.Load<Texture2D>("TX Tileset Wall");
            _textureStoneWall = content.Load<Texture2D>("TX Tileset Stone Ground");
            _texturePlantShadows = content.Load<Texture2D>("TX Plant with Shadow");
            _propsShadows = content.Load<Texture2D>("TX Props with Shadow");
            _tilesPerRow = Math.Max(1, _texture.Width / _tileSize); 
            GenerateMap();
            AddDetails();
            base.Load(content);
        }

        private Rectangle GetTileRectangle(int tileIndex, int tilesPerRow)
        {
            int x = (tileIndex % tilesPerRow) * _tileSize;
            int y = (tileIndex / tilesPerRow) * _tileSize;

            return new Rectangle(x, y, _tileSize, _tileSize);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    int tileIndex = _map[x, y];
                    Rectangle source = GetTileRectangle(tileIndex, _tilesPerRow);

                    spriteBatch.Draw(
                        _texture,
                        new Vector2(x * _screenTileSize, y * _screenTileSize),
                        source,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _renderScale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            base.Draw(gameTime, spriteBatch);
        }

        public void GenerateMap()
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    if (x == 0 || y == 0 || x == _mapWidth - 1 || y == _mapHeight - 1)
                    {
                        _map[x, y] = 40; // Wall tile
                    }
                    else
                    {
                        _map[x, y] = 0; // Floor tile
                    }
                }
            }
        }
        
       public bool IsBlocked(int x, int y)
        {
            // Outside map = always blocked
            if (x < 0 || y < 0 || x >= _mapWidth || y >= _mapHeight)
                return true;

            return _map[x, y] == 40;
        }

        // Convert world position (pixels) → tile position
        public Point WorldToTile(Vector2 position)
        {
            return new Point(
                (int)(position.X / _screenTileSize),
                (int)(position.Y / _screenTileSize)
            );
        }

        // Check full rectangle collision (for player/enemy size)
        public bool IsColliding(Vector2 position, int width, int height)
        {
            Point topLeft = WorldToTile(position);
            Point topRight = WorldToTile(new Vector2(position.X + width, position.Y));
            Point bottomLeft = WorldToTile(new Vector2(position.X, position.Y + height));
            Point bottomRight = WorldToTile(new Vector2(position.X + width, position.Y + height));

            return IsBlocked(topLeft.X, topLeft.Y) ||
                   IsBlocked(topRight.X, topRight.Y) ||
                   IsBlocked(bottomLeft.X, bottomLeft.Y) ||
                   IsBlocked(bottomRight.X, bottomRight.Y);
        }
        public void AddDetails()
        {
            for (int x = 1; x < _mapWidth - 1; x++)
            {
                for (int y = 1; y < _mapHeight - 1; y++)
                {
                    if (_map[x, y] == 0 && _random.NextDouble() < 0.3)
                    {
                        _map[x, y] = _random.Next(3, 61); // Detail tile
                    }
                }
            }
        }
    }
}