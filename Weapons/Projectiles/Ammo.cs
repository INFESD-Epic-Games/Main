using System;
using System.Reflection;
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

        // moet nog
        public int VerticalAmount { get; }
        public int HorizontalAmount { get; }
        public bool HasHoming { get; }
        public int PierceCount { get; }
        public int BounceCount { get; }
        public float ExplosionRadius { get; }
        public int LightningChains { get; }

        protected virtual bool CanDamageEnemies => false;

        protected virtual int GetEnemyDamage()
        {
            return 0;
        }

        protected Ammo(
            Vector2 location,
            Vector2 direction,
            float speed,
            float scale,
            float hitboxRatio,
            float maxLifetime,
            // alles vanaf hier lijkt me nog cool toetevoegen
            int verticalAmount = 1,
            int horizontalAmount = 1,
            bool hasHoming = false,
            int pierceCount = 0,
            int bounceCount = 0,
            float explosionRadius = 0f,
            int lightningChains = 0)
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

            // moet nog
            VerticalAmount = Math.Max(1, verticalAmount);
            HorizontalAmount = Math.Max(1, horizontalAmount);
            HasHoming = hasHoming;
            PierceCount = Math.Max(0, pierceCount);
            BounceCount = Math.Max(0, bounceCount);
            ExplosionRadius = Math.Max(0f, explosionRadius);
            LightningChains = Math.Max(0, lightningChains);
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
                if (!Bishop.TryProtectEnemy(enemy))
                {
                    ApplyEnemyDamage(enemy, GetEnemyDamage());
                }

                _gameManager.RemoveGameObject(this);
            }

            base.OnCollision(other);
        }

        private static void ApplyEnemyDamage(Enemy enemy, int damage)
        {
            MethodInfo takeDamageMethod = enemy.GetType().GetMethod(
                "TakeDamage",
                BindingFlags.Instance | BindingFlags.NonPublic);

            takeDamageMethod?.Invoke(enemy, new object[] { damage });
        }
    }
}