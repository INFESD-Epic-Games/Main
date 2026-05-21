using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;

namespace SpellFall.Enemies
{
    public class Goblin : Enemy
    {
        private const float MoveSpeed = 50f;
        private const float EnemyScale = 0.35f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 15;
        private const int Damage = 10;
        private const float FireCooldownSeconds = 2f;
        private const float StopDistance = 400f;

        private Texture2D _texture;
        private SoundEffect _enemyDeathSFX;
        private Texture2D _healthBarTexture;
        private float _fireCooldownTimer;
        private int _currentHealth;
        private int _frameWidth;
        private int _frameHeight;
        private bool _isDead;

        public Goblin(Point startPosition) : base(startPosition)
        {
            _fireCooldownTimer = 0f;
            _currentHealth = MaxHealth;
            _isDead = false;
        }

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("Goblin");
            _frameWidth = _texture.Width / 4;
            _frameHeight = _texture.Height;
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            _fireCooldownTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_fireCooldownTimer >= FireCooldownSeconds)
            {
                _fireCooldownTimer = 0f;
                
                // Fire a projectile towards the player
                FireProjectile();
            }
            

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            Vector2 directionToPlayer = playerPosition - _position;
            float distanceToPlayer = directionToPlayer.Length();

            if (distanceToPlayer > StopDistance && directionToPlayer != Vector2.Zero)
            {
                directionToPlayer.Normalize();
                _position += directionToPlayer * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            UpdateCollider();
            base.Update(gameTime);
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
                EnemyScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(
                spriteBatch,
                ref _healthBarTexture,
                _frameHeight * EnemyScale,
                _currentHealth,
                MaxHealth,
                40,
                6);

            base.Draw(gameTime, spriteBatch);
        }

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);
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

        private int GetFrameIndex(Vector2 playerPosition)
        {
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

        private void FireProjectile()
        {
            Vector2 direction = _gameManager.Player.GetPosition().Center.ToVector2() - _position;
            direction.Normalize();  

            GameManager.GetGameManager().AddGameObject(new Stone(_position, direction, 400f, Damage));      
        }
    }
}