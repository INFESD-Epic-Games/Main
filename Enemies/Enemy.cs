using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Background;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public abstract partial class Enemy : GameObject
    {
        private const float IceSlowMultiplier = 0.5f;
        private const float PoisonDamagePercentPerSecond = 0.025f;
        private const float FireDamagePerSecond = 2.5f;
        private const float DefaultPoisonDurationSeconds = 5f;
        private const float DefaultFireDurationSeconds = 4f;
        private const float DefaultIceDurationSeconds = 2f;

        private static int _nextEnemyId = 1;
        private static readonly HashSet<Enemy> _activeEnemies = new HashSet<Enemy>();

        protected readonly GameManager _gameManager;
        protected readonly RectangleCollider _rectangleCollider;
        protected readonly int _enemyId;
        protected Vector2 _position;
        protected Map _map;
        public Map CurrentMap => _map;
        public bool IsAlive { get; private set; } = true;
        public int MaxHealthValue { get; }
        protected int CurrentHealth { get; private set; }

        private float _movementSpeedMultiplier = 1f;
        private float _iceSlowTimer = 0f;
        private float _poisonTimer = 0f;
        private float _poisonTickAccumulator = 0f;
        private float _fireTimer = 0f;
        private float _fireTickAccumulator = 0f;
        private Color _healthBarTintColor = Color.LimeGreen;
        private float _healthBarTintTimer = 0f;

        protected Enemy(Point startPosition, int maxHealth)
        {
            _gameManager = GameManager.GetGameManager();
            _enemyId = _nextEnemyId++;
            _position = startPosition.ToVector2();
            MaxHealthValue = Math.Max(1, maxHealth);
            CurrentHealth = MaxHealthValue;

            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            SetCollider(_rectangleCollider);
            _map = _gameManager.CurrentMap;
            _activeEnemies.Add(this);
        }

        public static IEnumerable<Enemy> GetActiveEnemies()
        {
            return _activeEnemies;
        }

        public static void ResetActiveEnemies()
        {
            _activeEnemies.Clear();
        }

        public static Enemy GetClosestEnemy(Vector2 position, ISet<Enemy> excludedEnemies = null)
        {
            Enemy closestEnemy = null;
            float closestDistanceSquared = float.MaxValue;

            foreach (Enemy enemy in _activeEnemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (excludedEnemies != null && excludedEnemies.Contains(enemy))
                {
                    continue;
                }

                float distanceSquared = Vector2.DistanceSquared(position, enemy._position);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        protected void TryMove(Vector2 velocity, int width, int height)
        {
            if (_map == null)
            {
                _position += velocity;
                return;
            }

            Vector2 halfOffset = new Vector2(width / 2f, height / 2f);

            Vector2 newPos = _position + velocity;
            if (!_map.IsColliding(newPos - halfOffset, width, height))
            {
                _position = newPos;
                UpdateCollider();
                return;
            }

            Vector2 newPosX = new Vector2(_position.X + velocity.X, _position.Y);
            if (!_map.IsColliding(newPosX - halfOffset, width, height))
            {
                _position = newPosX;
            }

            Vector2 newPosY = new Vector2(_position.X, _position.Y + velocity.Y);
            if (!_map.IsColliding(newPosY - halfOffset, width, height))
            {
                _position = newPosY;
            }

            UpdateCollider();
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Enemy otherEnemy)
            {
                ResolveEnemyCollision(otherEnemy);
            }

            base.OnCollision(other);
        }

        public override void Destroy()
        {
            IsAlive = false;
            _activeEnemies.Remove(this);
            base.Destroy();
        }

        protected abstract void UpdateCollider();

        protected abstract SoundEffect DeathSoundEffect { get; }

        protected virtual bool CanBePushedByEnemies => true;

        protected float MovementSpeedMultiplier => _movementSpeedMultiplier;

        protected void UpdateCenteredCollider(int colliderWidth, int colliderHeight)
        {
            Point colliderLocation = (_position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();
            _rectangleCollider.shape = new Rectangle(colliderLocation, new Point(colliderWidth, colliderHeight));
        }

        protected void DrawHealthBar(
            SpriteBatch spriteBatch,
            ref Texture2D healthBarTexture,
            float bodyHeight,
            int currentHealth,
            int maxHealth,
            int barWidth,
            int barHeight)
        {
            if (healthBarTexture == null)
            {
                healthBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                healthBarTexture.SetData(new[] { Color.White });
            }

            float healthPercentage = (float)currentHealth / maxHealth;
            int barX = (int)(_position.X - barWidth / 2f);
            int barY = (int)(_position.Y - (bodyHeight / 2f) - 16);

            spriteBatch.Draw(
                healthBarTexture,
                new Rectangle(barX, barY, barWidth, barHeight),
                Color.DarkRed);

            Color fillColor = _healthBarTintTimer > 0f ? _healthBarTintColor : Color.LimeGreen;
            spriteBatch.Draw(
                healthBarTexture,
                new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight),
                fillColor);
        }

        public void ApplyHealthBarTint(Color color, float durationSeconds)
        {
            _healthBarTintColor = color;
            _healthBarTintTimer = Math.Max(0f, durationSeconds);
        }

        public void ApplyIceSlow(float durationSeconds = DefaultIceDurationSeconds)
        {
            _iceSlowTimer = Math.Max(_iceSlowTimer, Math.Max(0f, durationSeconds));
            _movementSpeedMultiplier = IceSlowMultiplier;
            ApplyHealthBarTint(Color.CornflowerBlue, durationSeconds);
            ApplyStatusFlash(Color.CornflowerBlue, durationSeconds);
        }

        public void ApplyPoison(float durationSeconds = DefaultPoisonDurationSeconds)
        {
            _poisonTimer = Math.Max(_poisonTimer, Math.Max(0f, durationSeconds));
            Color poisonColor = new Color(0, 120, 0);
            ApplyHealthBarTint(poisonColor, durationSeconds);
            ApplyStatusFlash(poisonColor, durationSeconds);
        }

        public void ApplyFire(float durationSeconds = DefaultFireDurationSeconds)
        {
            _fireTimer = Math.Max(_fireTimer, Math.Max(0f, durationSeconds));
            ApplyHealthBarTint(Color.Orange, durationSeconds);
            ApplyStatusFlash(Color.Orange, durationSeconds);
        }

        protected void ApplyDamage(int damage, Action onKilled = null)
        {
            if (damage <= 0 || !IsAlive)
            {
                return;
            }

            CurrentHealth -= damage;
            if (CurrentHealth > 0)
            {
                return;
            }

            KillEnemy(onKilled);
        }

        public void TakeDamage(int damage)
        {
            ApplyDamage(damage);
        }

        public void ApplyKnockback(Vector2 knockback)
        {
            if (!CanBePushedByEnemies || knockback == Vector2.Zero)
            {
                return;
            }

            int colliderWidth = Math.Max(1, _rectangleCollider.shape.Width);
            int colliderHeight = Math.Max(1, _rectangleCollider.shape.Height);
            TryMove(knockback, colliderWidth, colliderHeight);
        }

        protected void KillEnemy(Action onKilled = null)
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            onKilled?.Invoke();
            DeathSoundEffect?.Play();
            _gameManager.RemoveGameObject(this);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_iceSlowTimer > 0f)
            {
                _iceSlowTimer -= dt;
                if (_iceSlowTimer <= 0f)
                {
                    _iceSlowTimer = 0f;
                    _movementSpeedMultiplier = 1f;
                }
            }

            if (_poisonTimer > 0f)
            {
                _poisonTimer -= dt;
                _poisonTickAccumulator += MaxHealthValue * PoisonDamagePercentPerSecond * dt;

                int poisonDamage = (int)_poisonTickAccumulator;
                if (poisonDamage > 0)
                {
                    _poisonTickAccumulator -= poisonDamage;
                    ApplyDamage(poisonDamage);
                }

                if (_poisonTimer <= 0f)
                {
                    _poisonTimer = 0f;
                    _poisonTickAccumulator = 0f;
                }
            }

            if (_fireTimer > 0f)
            {
                _fireTimer -= dt;
                _fireTickAccumulator += FireDamagePerSecond * dt;

                int fireDamage = (int)_fireTickAccumulator;
                if (fireDamage > 0)
                {
                    _fireTickAccumulator -= fireDamage;
                    ApplyDamage(fireDamage);
                }

                if (_fireTimer <= 0f)
                {
                    _fireTimer = 0f;
                    _fireTickAccumulator = 0f;
                }
            }

            if (_healthBarTintTimer > 0f)
            {
                _healthBarTintTimer -= dt;
                if (_healthBarTintTimer <= 0f)
                {
                    _healthBarTintTimer = 0f;
                    _healthBarTintColor = Color.LimeGreen;
                }
            }

            UpdateVisualEffects(dt);

            base.Update(gameTime);
        }

        public Vector2 GetPosition()
        {
            return _position;
        }

        private void ResolveEnemyCollision(Enemy other)
        {
            if (!CanBePushedByEnemies && !other.CanBePushedByEnemies)
            {
                return;
            }

            if (!CanBePushedByEnemies)
            {
                other.PushApartFrom(this);
                return;
            }

            if (!other.CanBePushedByEnemies || ShouldResolveCollision(other))
            {
                PushApartFrom(other);
            }
        }

        private bool ShouldResolveCollision(Enemy other)
        {
            return _enemyId < other._enemyId;
        }

        private void PushApartFrom(Enemy other)
        {
            Rectangle ownBounds = _rectangleCollider.GetBoundingBox();
            Rectangle otherBounds = other._rectangleCollider.GetBoundingBox();

            Vector2 ownCenter = ownBounds.Center.ToVector2();
            Vector2 otherCenter = otherBounds.Center.ToVector2();
            Vector2 delta = ownCenter - otherCenter;

            float overlapX = (ownBounds.Width + otherBounds.Width) * 0.5f - Math.Abs(delta.X);
            float overlapY = (ownBounds.Height + otherBounds.Height) * 0.5f - Math.Abs(delta.Y);

            if (overlapX <= 0f || overlapY <= 0f)
            {
                return;
            }

            if (overlapX < overlapY)
            {
                float direction = delta.X >= 0f ? 1f : -1f;
                if (delta.X == 0f)
                {
                    direction = 1f;
                }

                _position.X += direction * (overlapX + 1f);
            }
            else
            {
                float direction = delta.Y >= 0f ? 1f : -1f;
                if (delta.Y == 0f)
                {
                    direction = 1f;
                }

                _position.Y += direction * (overlapY + 1f);
            }

            UpdateCollider();
        }
    }
}
