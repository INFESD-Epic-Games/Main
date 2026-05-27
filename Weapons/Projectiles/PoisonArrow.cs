using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Enemies;

namespace SpellFall.Weapons.Projectiles
{
    public class PoisonArrow : Ammo
    {
        private const float PoisonArrowScale = 0.4f;
        private const float PoisonArrowHitboxRatio = 0.2f;
        public int Damage { get; }

        public PoisonArrow(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 5f)
            : base(location, direction, speed, PoisonArrowScale, PoisonArrowHitboxRatio, maxLifetime)
        {
            Damage = Math.Max(0, damage);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("poisonarrow");
            SetHitboxFromTexture();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(_texture, _circleCollider.Center, null, Color.White, _rotation, origin, Scale, SpriteEffects.None, 0f);
            base.Draw(gameTime, spriteBatch);
        }

        protected override bool CanDamageEnemies => true;

        protected override int GetEnemyDamage()
        {
            return Damage;
        }

        protected override void OnEnemyHit(Enemy enemy)
        {
            enemy.ApplyPoison();
            base.OnEnemyHit(enemy);
        }
    }
}
