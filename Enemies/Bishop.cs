using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
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
        private int _currentHealth;
        private bool _isDead;
        private static readonly HashSet<Bishop> _activeBishops = new HashSet<Bishop>();
        // private readonly Vector2 _fixedPosition;

        public Bishop(Point startPosition) : base(startPosition)
        {
            _currentHealth = MaxHealth;
            _isDead = false;
            // _fixedPosition = new Vector2(startPosition.X, startPosition.Y);
        }

        protected override bool CanBePushedByEnemies => false;

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("bishop");
            UpdateCollider();
            _activeBishops.Add(this);
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            // _position = _fixedPosition;
            UpdateCollider();
        }

        public override void Destroy()
        {
            _activeBishops.Remove(this);
            base.Destroy();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            spriteBatch.Draw(
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
                _currentHealth,
                MaxHealth,
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

        private void TakeDamage(int damage)
        {
            if (_isDead)
            {
                return;
            }

            _currentHealth -= damage;
            if (_currentHealth > 0)
            {
                return;
            }

            _isDead = true;
            KillEnemy(_enemyDeathSFX);
        }

        public static bool TryProtectEnemy(Enemy ally)
        {
            foreach (Bishop bishop in _activeBishops)
            {
                if (bishop.ProtectAlly(ally))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ProtectAlly(Enemy ally)
        {
            if (_isDead || ally == null || ally == this || ally is Bishop)
            {
                return false;
            }

            _gameManager.AddGameObject(new BishopLightning(this, ally, ProtectionEffectDurationSeconds));
            return true;
        }
    }
}