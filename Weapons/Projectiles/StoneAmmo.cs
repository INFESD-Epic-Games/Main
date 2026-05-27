using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public class StoneAmmo : Ammo
    {
        private const float StoneAmmoScale = 1.15f;
        private const float StoneAmmoHitboxRatio = 0.24f;
        public int Damage { get; }

        public StoneAmmo(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 5f)
            : base(
                location,
                direction,
                speed,
                StoneAmmoScale,
                StoneAmmoHitboxRatio,
                maxLifetime)
        {
            Damage = Math.Max(0, damage);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Stone");
            SetHitboxFromTexture();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 stoneOrigin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _circleCollider.Center,
                null,
                Color.White,
                _rotation,
                stoneOrigin,
                Scale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Player player)
            {
                player.HealthBar.TakeDamage(Damage);
                _gameManager.RemoveGameObject(this);
            }

            base.OnCollision(other);
        }
    }
}
