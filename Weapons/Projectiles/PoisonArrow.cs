using System;
using System.Collections.Generic;
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
        private const int MaxEnemyPierces = 1;
        private const float PierceForwardStep = 12f;
        public int Damage { get; }
        private int _enemiesPierced;
        private readonly HashSet<Enemy> _hitEnemies;

        public PoisonArrow(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 5f)
            : base(location, direction, speed, PoisonArrowScale, PoisonArrowHitboxRatio, maxLifetime)
        {
            Damage = Math.Max(0, damage);
            _enemiesPierced = 0;
            _hitEnemies = new HashSet<Enemy>();
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

        public override void OnCollision(Engine.GameObject other)
        {
            if (other is Enemy enemy && CanDamageEnemies)
            {
                if (_hitEnemies.Contains(enemy))
                {
                    return;
                }

                if (Bishop.TryProtectEnemy(enemy))
                {
                    _gameManager.RemoveGameObject(this);
                    return;
                }

                _hitEnemies.Add(enemy);
                OnEnemyHit(enemy);

                if (_enemiesPierced >= MaxEnemyPierces)
                {
                    _gameManager.RemoveGameObject(this);
                }
                else
                {
                    _enemiesPierced++;
                    _circleCollider.Center += Vector2.Normalize(_velocity) * PierceForwardStep;
                }
                return;
            }

            base.OnCollision(other);
        }
    }
}
