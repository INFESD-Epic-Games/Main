using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public class FireArrow : Ammo
    {
        private const float FireArrowScale = 0.4f;
        private const float FireArrowHitboxRatio = 0.2f;
        public int Damage { get; }

        public FireArrow(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 5f)
            : base(location, direction, speed, FireArrowScale, FireArrowHitboxRatio, maxLifetime)
        {
            Damage = System.Math.Max(0, damage);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("firearrow");
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
            return 0;
        }

        protected override void OnEnemyHit(SpellFall.Enemies.Enemy enemy)
        {
            enemy.ApplyFire();
        }
    }
}
