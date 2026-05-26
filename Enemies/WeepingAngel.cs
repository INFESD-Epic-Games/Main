using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;
using SpellFall.Character;
using Microsoft.Xna.Framework.Audio;
using SpellFall.Collision;
using System.Collections.Generic;

namespace SpellFall.Enemies
{
    public class WeepingAngel : Enemy, IWatchable
    {
        private const float MoveSpeed = 100f;
        private const float AlienScale = 1.25f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 50;
        private const int ContactDamage = 10;
        private const float ContactCooldownSeconds = 2f;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private int _currentHealth;
        private bool _isDead;
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
            : base(startPosition)
        {
            _currentHealth = MaxHealth;
            _isDead = false;
            _contactCooldownTimer = 0f;
        }

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

            if (IsWatched)
            {
                UpdateCollider();
                base.Update(gameTime);
                return;
            }

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            
            // Pathfinding update timer
            _pathRecalcTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            var playerTile = _map.WorldToTile(playerPosition);
            var myTile = _map.WorldToTile(_position);

            if (_path == null || _pathIndex >= (_path?.Count ?? 0) || _pathRecalcTimer <= 0f || !playerTile.Equals(_lastTargetTile))
            {
                _path = _map.FindPath(myTile, playerTile);
                _pathIndex = 0;
                _pathRecalcTimer = 0.2f; // recalc every 0.2 second
                _lastTargetTile = playerTile;
            }

            int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);

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
                    Vector2 velocity = directionToTarget * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    TryMove(velocity, colliderWidth, colliderHeight);
                }
            }
            else
            {
                // fallback to direct movement if no path found
                Vector2 directionToPlayer = playerPosition - _position;

                if (directionToPlayer != Vector2.Zero)
                {
                    directionToPlayer.Normalize();
                    Vector2 velocity = directionToPlayer * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    TryMove(velocity, colliderWidth, colliderHeight);
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
            int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }
    }
}
