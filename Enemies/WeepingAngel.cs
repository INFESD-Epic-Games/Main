using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public class WeepingAngel : Enemy, IWatchable
    {
        private const float MoveSpeed = 100f;
        private const float EnemyScale = 1.25f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 60;
        private const int ContactDamage = 10;
        private const float ContactCooldownSeconds = 2f;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private float _contactCooldownTimer;
        private int _frameWidth;
        private int _frameHeight;
        private SoundEffect _enemyDeathSFX;
        public bool IsWatched { get; set; }
        private List<Point> _path;
        private int _pathIndex;
        private float _pathRecalcTimer;
        private Point _lastTargetTile = new Point(-1, -1);

        public WeepingAngel(Point startPosition)
            : base(startPosition, MaxHealth)
        {
            _contactCooldownTimer = 0f;
        }

        protected override SoundEffect DeathSoundEffect => _enemyDeathSFX;

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("weeping-angel");
            _frameWidth = _texture.Width / 3;
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

            if (_map == null)
            {
                _map = _gameManager.CurrentMap;
            }

            if (IsWatched)
            {
                UpdateCollider();
                base.Update(gameTime);
                return;
            }

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();

            if (_map == null)
            {
                Vector2 directionToPlayer = playerPosition - _position;
                if (directionToPlayer != Vector2.Zero)
                {
                    directionToPlayer.Normalize();
                    Vector2 velocity = directionToPlayer * MoveSpeed * MovementSpeedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    int directColliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
                    int directColliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);
                    TryMove(velocity, directColliderWidth, directColliderHeight);
                }

                UpdateCollider();
                base.Update(gameTime);
                return;
            }

            _pathRecalcTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            Point playerTile = _map.WorldToTile(playerPosition);
            Point myTile = _map.WorldToTile(_position);
            HashSet<Point> blockedTiles = new HashSet<Point>();

            foreach (Enemy enemy in Enemy.GetActiveEnemies())
            {
                if (enemy == this || !enemy.IsAlive)
                {
                    continue;
                }

                blockedTiles.Add(_map.WorldToTile(enemy.GetPosition()));
            }

            if (_path == null || _pathIndex >= (_path?.Count ?? 0) || _pathRecalcTimer <= 0f || !playerTile.Equals(_lastTargetTile))
            {
                _path = _map.FindPath(myTile, playerTile, blockedTiles);
                _pathIndex = 0;
                _pathRecalcTimer = 0.2f;
                _lastTargetTile = playerTile;
            }

            int pathColliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int pathColliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);

            if (_path != null && _path.Count > 0 && _pathIndex < _path.Count)
            {
                Vector2 targetWorld = _map.TileToWorldCenter(_path[_pathIndex]);
                Vector2 directionToTarget = targetWorld - _position;
                float dist = directionToTarget.Length();
                if (dist < 4f)
                {
                    _pathIndex++;
                }
                else
                {
                    directionToTarget.Normalize();
                    Vector2 velocity = directionToTarget * MoveSpeed * MovementSpeedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    TryMove(velocity, pathColliderWidth, pathColliderHeight);
                }
            }
            else
            {
                Vector2 directionToPlayer = playerPosition - _position;

                if (directionToPlayer != Vector2.Zero)
                {
                    directionToPlayer.Normalize();
                    Vector2 velocity = directionToPlayer * MoveSpeed * MovementSpeedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    TryMove(velocity, pathColliderWidth, pathColliderHeight);
                }
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
            int frameIndex = GetFrameIndex(_gameManager.Player.GetPosition().Center.ToVector2(), IsWatched);
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
                CurrentHealth,
                MaxHealthValue,
                40,
                6);

            base.Draw(gameTime, spriteBatch);
        }

        private int GetFrameIndex(Vector2 playerPosition, bool isWatched)
        {
            bool isAbovePlayer = _position.Y < playerPosition.Y;

            if (!isAbovePlayer)
            {
                return 2;
            }

            if (isWatched)
            {
                return 1;
            }

            return 0;
        }

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }
    }
}
