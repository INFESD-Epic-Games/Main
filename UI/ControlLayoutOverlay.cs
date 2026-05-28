using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.UI
{
    public class ControlLayoutOverlay : GameObject
    {
        private static readonly Vector2 WorldPosition = new Vector2(320f, 640f);

        private Texture2D _texture;

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Control_layout");
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            spriteBatch.Draw(_texture, WorldPosition, Color.White);

            base.Draw(gameTime, spriteBatch);
        }
    }
}