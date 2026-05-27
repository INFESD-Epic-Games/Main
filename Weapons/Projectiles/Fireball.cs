using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public class Fireball : Ammo
    {
        private const float FireballScale = 1f;
        private const float FireballHitboxRatio = 0.2f;
        public int Damage { get; }
        private float _fireballRotation;

        public Fireball(Vector2 location, Vector2 direction, float rotation, float speed, int damage, float maxLifetime = 5f)
            : base(
                location,
                direction,
                speed,
                FireballScale,
                FireballHitboxRatio,
                maxLifetime)
        {
            Damage = Math.Max(0, damage);
            _fireballRotation = rotation;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Fireball");
            SetHitboxFromTexture();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 fireballOrigin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _circleCollider.Center,
                null,
                Color.White,
                _fireballRotation,
                fireballOrigin,
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