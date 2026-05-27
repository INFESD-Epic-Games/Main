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