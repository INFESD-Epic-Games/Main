using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public class Bishop : Enemy
    {
        private const float EnemyScale = 0.20f;
        private const float HitboxScale = 0.45f;
        private const int MaxHealth = 55;
        private const float ProtectionEffectDurationSeconds = 1f;

        private Texture2D _texture;
        private SoundEffect _enemyDeathSFX;
        private Texture2D _healthBarTexture;

        public Bishop(Point startPosition) : base(startPosition, MaxHealth)
        {
        }

        protected override SoundEffect DeathSoundEffect => _enemyDeathSFX;
        protected override bool CanBePushedByEnemies => false;

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("bishop");
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateCollider();
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            DrawEnemySprite(
                spriteBatch,
                _texture,
                _position,
                null,
                Color.White,
                0f,
                origin,
                EnemyScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(
                spriteBatch,
                ref _healthBarTexture,
                _texture.Height * EnemyScale,
                CurrentHealth,
                MaxHealthValue,
                40,
                6);

            base.Draw(gameTime, spriteBatch);
        }

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_texture.Width * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_texture.Height * EnemyScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }

        public static bool TryProtectEnemy(Enemy ally)
        {
            foreach (Enemy enemy in Enemy.GetActiveEnemies())
            {
                if (enemy is Bishop bishop && bishop.ProtectAlly(ally))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ProtectAlly(Enemy ally)
        {
            if (!IsAlive || ally == null || ally == this || ally is Bishop)
            {
                return false;
            }

            ally.ApplyHitFlash();
            _gameManager.AddGameObject(new BishopLightning(this, ally, ProtectionEffectDurationSeconds));
            return true;
        }
    }
}
