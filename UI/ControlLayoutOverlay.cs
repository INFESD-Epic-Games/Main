using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.UI
{
    public class ControlLayoutOverlay : GameObject
    {
        private static readonly Vector2 WorldPosition = new Vector2(220f, 640f);

        private Texture2D _texture;
        // private Texture2D _textureInventory;
        private Vector2 position = new Vector2(2600, 600); 

        private float scale2 = 0.75f;
        private float opacity = 0.5f;

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Control_layout");
            // _textureInventory = content.Load<Texture2D>("inventory tut");
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }
            spriteBatch.Draw(_texture, 
                WorldPosition, 
                null,
                Color.White * opacity,
                0f,
                Vector2.Zero,
                scale2,
                SpriteEffects.None,
                0f
            );
            base.Draw(gameTime, spriteBatch);
        }
    }
}