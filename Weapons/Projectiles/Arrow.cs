using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Weapons.Projectiles
{
    public class Arrow : Ammo
    {
        private const float ArrowScale = 0.4f;
        private const float ArrowHitboxRatio = 0.2f;
        public int Damage { get; }

        public Arrow(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 5f)
            : base(
                location,
                direction,
                speed,
                ArrowScale,
                ArrowHitboxRatio,
                maxLifetime)
        {
            Damage = Math.Max(0, damage);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("PIJL");
            SetHitboxFromTexture();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 arrowOrigin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _circleCollider.Center,
                null,
                Color.White,
                _rotation,
                arrowOrigin,
                Scale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }
    }
}
