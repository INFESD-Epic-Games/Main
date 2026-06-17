using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Background
{
    public class Map : GameObject
    {
        private Texture2D _texture;
        private string _textureName;
        public Vector2 Position { get; set; }
        private const int _tileSize = 32;
        private const int _renderScale = 4;
        private const int _screenTileSize = _tileSize * _renderScale;

        private int[,] _collision;
        public bool EnemiesSpawned { get; set; }

        public Map(
            string textureName,
            Vector2 position,
            int[,] collision)
        {
            _textureName = textureName;
            Position = position;
            _collision = collision;
            EnemiesSpawned = false;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>(_textureName);
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                Position,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                _renderScale,
                SpriteEffects.None,
                0f
            );
            
            // Texture2D pixel = new Texture2D(
            //     spriteBatch.GraphicsDevice,
            //     1,
            //     1);

            // pixel.SetData(new[] { Color.Red });

            // for (int x = 0; x < _collision.GetLength(1); x++)
            // {
            //     for (int y = 0; y < _collision.GetLength(0); y++)
            //     {
            //         if (_collision[y, x] == 1)
            //         {
            //             spriteBatch.Draw(
            //                 pixel,
            //                 new Rectangle(
            //                     (int)Position.X + x * _screenTileSize,
            //                     (int)Position.Y + y * _screenTileSize,
            //                     _screenTileSize,
            //                     _screenTileSize),
            //                 Color.Red * 0.3f
            //             );
            //         }
            //     }
            // }

            base.Draw(gameTime, spriteBatch);
        }

        public Point WorldToTile(Vector2 worldPosition)
        {
            Vector2 local = worldPosition - Position;

            return new Point(
                (int)(local.X / _screenTileSize),
                (int)(local.Y / _screenTileSize)
            );
        }

        public bool IsBlocked(int x, int y)
        {
            int rows = _collision.GetLength(0);
            int cols = _collision.GetLength(1);

            if (x < 0 || y < 0 || x >= cols || y >= rows)
                return false;

            return _collision[y, x] == 1;
        }


        public Vector2 TileToWorldCenter(Point tile)
        {
            return new Vector2(
                tile.X * _screenTileSize + _screenTileSize / 2f,
                tile.Y * _screenTileSize + _screenTileSize / 2f);
        }

        public List<Point> FindPath(Point start, Point goal, ISet<Point> blockedTiles = null)
        {
            int rows = _collision.GetLength(0);
            int cols = _collision.GetLength(1);

            if (start.X < 0 || start.Y < 0 || start.X >= cols || start.Y >= rows)
                return null;

            if (goal.X < 0 || goal.Y < 0 || goal.X >= cols || goal.Y >= rows)
                return null;

            bool IsTileBlocked(Point tile)
            {
                return IsBlocked(tile.X, tile.Y) || (blockedTiles != null && blockedTiles.Contains(tile));
            }

            if (IsTileBlocked(goal))
                return null;

            var directions = new Point[]
            {
                new Point(1,0), new Point(-1,0), new Point(0,1), new Point(0,-1),
                new Point(1,1), new Point(1,-1), new Point(-1,1), new Point(-1,-1)
            };

            var open = new PriorityQueue<Point, float>();
            var gScore = new Dictionary<Point, float>();
            var fScore = new Dictionary<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();

            float Heuristic(Point a, Point b) => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

            gScore[start] = 0f;
            fScore[start] = Heuristic(start, goal);
            open.Enqueue(start, fScore[start]);

            while (open.Count > 0)
            {
                var current = open.Dequeue();

                if (current == goal)
                {
                    var path = new List<Point>();
                    var node = current;
                    while (!node.Equals(start))
                    {
                        path.Add(node);
                        node = cameFrom[node];
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var dir in directions)
                {
                    var neighbor = new Point(current.X + dir.X, current.Y + dir.Y);

                    if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= cols || neighbor.Y >= rows)
                        continue;

                    if (IsTileBlocked(neighbor))
                        continue;

                    // prevent cutting corners: if moving diagonally, ensure adjacent cardinal tiles are free
                    if (Math.Abs(dir.X) == 1 && Math.Abs(dir.Y) == 1)
                    {
                        if (IsTileBlocked(new Point(current.X + dir.X, current.Y)) || IsTileBlocked(new Point(current.X, current.Y + dir.Y)))
                            continue;
                    }

                    float moveCost = (Math.Abs(dir.X) == 1 && Math.Abs(dir.Y) == 1) ? 1.41421356f : 1f;
                    float tentativeG = gScore[current] + moveCost;

                    if (!gScore.TryGetValue(neighbor, out var existingG) || tentativeG < existingG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + Heuristic(neighbor, goal);
                        fScore[neighbor] = f;
                        open.Enqueue(neighbor, f);
                    }
                }
            }

            return null;
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