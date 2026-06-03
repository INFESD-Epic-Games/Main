using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Enemies;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public abstract class Ammo : GameObject
    {
        protected readonly GameManager _gameManager;
        protected readonly CircleCollider _circleCollider;
        protected readonly Vector2 _velocity;
        protected readonly float _rotation;

        protected Texture2D _texture;
        protected float _lifetime;
        protected readonly float _maxLifetime;

        public float Scale { get; }
        public float HitboxRatio { get; }

        protected virtual bool CanDamageEnemies => false;

        protected virtual int GetEnemyDamage()
        {
            return 0;
        }

        protected virtual void OnEnemyHit(Enemy enemy)
        {
            DealEnemyDamage(enemy, GetEnemyDamage());
        }

        protected Ammo(
            Vector2 location,
            Vector2 direction,
            float speed,
            float scale,
            float hitboxRatio,
            float maxLifetime)
        {
            _gameManager = GameManager.GetGameManager();
            _circleCollider = new CircleCollider(location, 8f);
            SetCollider(_circleCollider);

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            else
            {
                direction = Vector2.UnitX;
            }

            _velocity = direction * speed;
            _rotation = LinePieceCollider.GetAngle(direction);

            Scale = Math.Max(0f, scale);
            HitboxRatio = Math.Max(0f, hitboxRatio);
            _maxLifetime = Math.Max(0.01f, maxLifetime);
            _lifetime = 0f;
        }

        protected void SetHitboxFromTexture()
        {
            _circleCollider.Radius = _texture.Width * Scale * HitboxRatio;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _circleCollider.Center += _velocity * dt;
            _lifetime += dt;

            if (_lifetime > _maxLifetime)
            {
                _gameManager.RemoveGameObject(this);
            }

            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Enemy enemy && CanDamageEnemies)
            {
                if (Bishop.TryProtectEnemy(enemy))
                {
                    _gameManager.RemoveGameObject(this);
                    base.OnCollision(other);
                    return;
                }

                OnEnemyHit(enemy);

                _gameManager.RemoveGameObject(this);
            }

            base.OnCollision(other);
        }

        protected void DealEnemyDamage(Enemy enemy, int damage)
        {
            enemy.TakeDamage(damage);
        }
    }
}