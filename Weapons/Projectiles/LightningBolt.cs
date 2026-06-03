using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Enemies;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public class LightningBolt : Ammo
    {
        private const float LightningScale = 0.8f;
        private const float LightningHitboxRatio = 0.2f;
        private const int ChainCount = 2;
        private const float ChainMaxDistance = 450f;

        public int Damage { get; }

        protected override bool CanDamageEnemies => true;

        public LightningBolt(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 3f)
            : base(
                location,
                direction,
                speed,
                LightningScale,
                LightningHitboxRatio,
                maxLifetime)
        {
            Damage = System.Math.Max(0, damage);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("lightning");
            SetHitboxFromTexture();
            base.Load(content);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 lightningOrigin = new Vector2(_texture.Width / 2f, _texture.Height / 4f);
            float drawRotation = _rotation + MathHelper.PiOver2;
            spriteBatch.Draw(
                _texture,
                _circleCollider.Center,
                new Rectangle(0, 0, _texture.Width, _texture.Height / 2),
                Color.White,
                drawRotation,
                lightningOrigin,
                Scale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }

        protected override void OnEnemyHit(Enemy enemy)
        {
            HashSet<Enemy> hitEnemies = new HashSet<Enemy> { enemy };
            if (Bishop.TryProtectEnemy(enemy))
            {
                return;
            }

            DealEnemyDamage(enemy, Damage);

            Vector2 chainOrigin = enemy.GetPosition();
            Enemy previous = enemy;
            for (int chainIndex = 0; chainIndex < ChainCount; chainIndex++)
            {
                Enemy nextEnemy = Enemy.GetClosestEnemy(chainOrigin, hitEnemies);
                if (nextEnemy == null)
                {
                    break;
                }

                float distSq = Vector2.DistanceSquared(chainOrigin, nextEnemy.GetPosition());
                if (distSq > ChainMaxDistance * ChainMaxDistance)
                {
                    break;
                }

                hitEnemies.Add(nextEnemy);
                if (Bishop.TryProtectEnemy(nextEnemy))
                {
                    continue;
                }

                DealEnemyDamage(nextEnemy, Damage);
               
                _gameManager.AddGameObject(new LightningChain(previous, nextEnemy, 0.15f));
                previous = nextEnemy;
                chainOrigin = nextEnemy.GetPosition();
            }
        }
    }
}