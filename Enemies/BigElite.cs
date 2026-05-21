using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;
using SpellFall.Character;
using Microsoft.Xna.Framework.Audio;

namespace SpellFall.Enemies
{
    public class BigElite : Enemy
    {
        private const float MoveSpeed = 40f;
        private const float AlienScale = 1f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 100;
        private const int ContactDamage = 25;
        private const float ContactCooldownSeconds = 5f;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private int _currentHealth;
        private bool _isDead;
        private float _contactCooldownTimer;
        private int _frameWidth;
        private int _frameHeight;
        private SoundEffect _enemyDeathSFX;

        public BigElite(Point startPosition)
            : base(startPosition)
        {
            _currentHealth = MaxHealth;
            _isDead = false;
            _contactCooldownTimer = 0f;
        }

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("elite");
            _frameWidth = _texture.Width / 1;
            _frameHeight = _texture.Height;
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_contactCooldownTimer > 0f)
            {
                _contactCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_contactCooldownTimer < 0f)
                {
                    _contactCooldownTimer = 0f;
                }

                UpdateCollider();
                base.Update(gameTime);
                return;
            }

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            Vector2 directionToPlayer = playerPosition - _position;

            if (directionToPlayer != Vector2.Zero)
            {
                directionToPlayer.Normalize();
                _position += directionToPlayer * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            UpdateCollider();
            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Arrow arrow)
            {
                _gameManager.RemoveGameObject(other);
                TakeDamage(arrow.Damage);
            }
            else if (other is Player && _contactCooldownTimer <= 0f)
            {
                _gameManager.Player.HealthBar.TakeDamage(ContactDamage);
                _contactCooldownTimer = ContactCooldownSeconds;
            }

            base.OnCollision(other);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int frameIndex = GetFrameIndex(_gameManager.Player.GetPosition().Center.ToVector2());
            Rectangle sourceRectangle = new Rectangle(frameIndex * _frameWidth, 0, _frameWidth, _frameHeight);
            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight / 2f);

            spriteBatch.Draw(
                _texture,
                _position,
                sourceRectangle,
                Color.White,
                0f,
                origin,
                AlienScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(
                spriteBatch,
                ref _healthBarTexture,
                _frameHeight * AlienScale,
                _currentHealth,
                MaxHealth,
                40,
                6);

            base.Draw(gameTime, spriteBatch);
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
            KillEnemy(_enemyDeathSFX, () =>
            {
            });
        }

        private int GetFrameIndex(Vector2 playerPosition)
        {
            return 0; // TODO: Walking animation and stuff
            
            bool isRightOfPlayer = _position.X >= playerPosition.X;
            bool isAbovePlayer = _position.Y < playerPosition.Y;

            if (isRightOfPlayer && isAbovePlayer)
            {
                return 0;
            }

            if (!isRightOfPlayer && isAbovePlayer)
            {
                return 1;
            }

            if (!isRightOfPlayer && !isAbovePlayer)
            {
                return 2;
            }

            return 3;
        }

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }
    }
}
