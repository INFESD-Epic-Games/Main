using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Engine;
using System;
using Microsoft.Xna.Framework;

namespace SpellFall.Enemies
{
    public class Eye : Enemy
    {
        private const float MoveSpeed = 100f;
        private const float AlienScale = 0.5f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 50;
        private const int ContactDamage = 5;
        private const float ContactCooldownSeconds = 1f;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private float _contactCooldownTimer;
        private int _frameWidth;
        private int _frameHeight;
        private SoundEffect _enemyDeathSFX;
        private float _bobSpeed = 3f;
        private float _bobHeight = 8f;

        public Eye(Point startPosition)
            : base(startPosition, MaxHealth)
        {
            _contactCooldownTimer = 0f;
        }

        protected override SoundEffect DeathSoundEffect => _enemyDeathSFX;

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("eye");
            _frameWidth = _texture.Width / 4;
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
                Vector2 velocity = directionToPlayer * MoveSpeed * MovementSpeedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;

                int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
                int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);
                TryMove(velocity, colliderWidth, colliderHeight);
            }

            UpdateCollider();   
            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Player && _contactCooldownTimer <= 0f)
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
            
            float elapsedSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            float bobOffset = (float)Math.Sin(elapsedSeconds * _bobSpeed) * _bobHeight;

            Vector2 floatingPosition = new Vector2(_position.X, _position.Y + bobOffset);

            spriteBatch.Draw(
                _texture,
                floatingPosition,
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
                CurrentHealth,
                MaxHealthValue,
                40,
                6);

            base.Draw(gameTime, spriteBatch);
        }

        private int GetFrameIndex(Vector2 playerPosition)
        {
            bool isRightOfPlayer = _position.X >= playerPosition.X;
            bool isAbovePlayer = _position.Y < playerPosition.Y;

            if (!isRightOfPlayer && isAbovePlayer)
            {
                return 0;
            }

            if (isRightOfPlayer && isAbovePlayer)
            {
                return 1;
            }

            if (!isRightOfPlayer && !isAbovePlayer)
            {
                return 3;
            }

            return 2;
        }

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }
    }
}
