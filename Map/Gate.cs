using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Enemies;
using SpellFall.Character;
using System.Linq;

namespace SpellFall.Background
{
    public class Gate : GameObject
    {
        private Texture2D _texture;
        private Texture2D _textureground;

        public Rectangle Bounds;
        public bool IsOpen;

        private Map _room;

        public Gate(Rectangle bounds, Map room)
        {
            Bounds = bounds;
            _room = room;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("gate");
            _textureground = content.Load<Texture2D>("TX Tileset Stone Ground");

            int frameWidth = _texture.Width;
            int frameHeight = _texture.Height / 2;

            float scale = 4f;

            Bounds = new Rectangle(
                Bounds.X,
                Bounds.Y,
                (int)(frameWidth * scale),
                (int)(frameHeight * scale)
            );

            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!GameManager.GetGameManager().QuestManager.HasActiveQuest("Main Quest"))
            {
                IsOpen = false;
                return;
            }

            var enemiesInRoom = Enemy.GetActiveEnemies()
                .Where(e => e.IsAlive &&
                            e.CurrentMap == _room);


            IsOpen = !enemiesInRoom.Any();

            base.Update(gameTime);
        }
        public override void Draw(
            GameTime gameTime,
            SpriteBatch spriteBatch)
        {
                Rectangle stoneSource = new Rectangle(
                    256,
                    128, 
                    32,
                    32 
            );
            
            int tileSize = 32 * 4;

            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(
                    _textureground,
                    new Rectangle(
                        Bounds.X + 64 + i * tileSize,
                        Bounds.Y - 12 + Bounds.Height / 2,
                        tileSize,
                        tileSize
                    ),
                    stoneSource,
                    Color.White
                );
            }

            int frameWidth = _texture.Width;
            int frameHeight = _texture.Height / 2;

            int frame = IsOpen ? 1 : 0;

            Rectangle sourceRect = new Rectangle(
                0,
                frame * frameHeight,
                frameWidth,
                frameHeight
            );

            spriteBatch.Draw(
                _texture,
                Bounds,
                sourceRect,
                Color.White
            );

            base.Draw(gameTime, spriteBatch);
        }
    }
}