using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public class Teleporter : Enemy
    {
        private const float EnemyScale = 0.5f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 20;
        private const float TeleportDistance = 400f;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private int _currentHealth;
        private int _frameWidth;
        private int _frameHeight;
        private bool _canTeleport = true;
        private Vector2 _teleportStartPosition;
        private Vector2 _teleportEndPosition;
        private bool _isTeleporting = false;
        private float _teleportElapsedTime = 0f;
        private float _teleportDuration = 0.5f; // Duration of the teleportation in seconds 
        private float _teleportTimer = 0f;
        private float _teleportCooldown = 5f;
        private static readonly Random _random = new Random();

        public Teleporter(Point startPosition) : base(startPosition)
        {
            _currentHealth = MaxHealth;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("alien");
            _frameWidth = _texture.Width / 4;
            _frameHeight = _texture.Height;
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!_canTeleport)
            {
                _teleportTimer -= deltaTime;
            }

            UpdateCollider();
            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
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
                EnemyScale,
                SpriteEffects.None,
                0f
            );

            DrawHealthBar(
                spriteBatch,
                ref _healthBarTexture,
                _frameHeight * EnemyScale,
                _currentHealth,
                MaxHealth,
                40,
                6
            );

            base.Draw(gameTime, spriteBatch);
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

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }

        private void Teleport()
        {
            if (!_canTeleport)
                return;

            Vector2 teleportDirection = new Vector2(
                _random.Next(-1, 2),
                _random.Next(-1, 2)
            );

            _teleportStartPosition = _position;
            _teleportEndPosition = _position + (teleportDirection * TeleportDistance);
            _isTeleporting = true;
            _teleportElapsedTime = 0f;

            _canTeleport = false;
            _teleportTimer = _teleportCooldown;
        }
    }
}