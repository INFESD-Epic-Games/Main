using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;

namespace SpellFall.Enemies
{
    public class Goblin : Enemy
    {
        private const float MoveSpeed = 50f;
        private const float EnemyScale = 0.35f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 22;
        private const int Damage = 10;
        private const float FireCooldownSeconds = 2f;
        private const float StopDistance = 400f;

        private Texture2D _texture;
        private SoundEffect _enemyDeathSFX;
        private Texture2D _healthBarTexture;
        private float _fireCooldownTimer;
        private int _frameWidth;
        private int _frameHeight;
        private List<Point> _path;
        private int _pathIndex;
        private float _pathRecalcTimer;
        private Point _lastTargetTile = new Point(-1, -1);

        public Goblin(Point startPosition) : base(startPosition, MaxHealth)
        {
            _fireCooldownTimer = 0f;
        }

        protected override SoundEffect DeathSoundEffect => _enemyDeathSFX;

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
                FireProjectile();
            }

            if (_map == null)
            {
                _map = _gameManager.CurrentMap;
            }

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            Vector2 directionToPlayer = playerPosition - _position;
            float distanceToPlayer = directionToPlayer.Length();

            if (_map == null || distanceToPlayer <= StopDistance)
            {
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

            int colliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);

            if (_path != null && _path.Count > 0 && _pathIndex < _path.Count)
            {
                Vector2 targetWorld = _map.Position + _map.TileToWorldCenter(_path[_pathIndex]);
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
                    TryMove(velocity, colliderWidth, colliderHeight);
                }
            }
            else if (directionToPlayer != Vector2.Zero)
            {
                directionToPlayer.Normalize();
                Vector2 velocity = directionToPlayer * MoveSpeed * MovementSpeedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
                TryMove(velocity, colliderWidth, colliderHeight);
            }

            UpdateCollider();
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int frameIndex = GetFrameIndex(_gameManager.Player.GetPosition().Center.ToVector2());
            Rectangle sourceRectangle = new Rectangle(frameIndex * _frameWidth, 0, _frameWidth, _frameHeight);
            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight / 2f);

            DrawEnemySprite(
                spriteBatch,
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

        protected override void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * EnemyScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * EnemyScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
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