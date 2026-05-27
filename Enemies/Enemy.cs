using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Background;

namespace SpellFall.Enemies
{
    public abstract class Enemy : GameObject
    {
        private static int _nextEnemyId = 1;
        private static readonly HashSet<Enemy> _activeEnemies = new HashSet<Enemy>();

        protected readonly GameManager _gameManager;
        protected readonly RectangleCollider _rectangleCollider;
        protected readonly int _enemyId;
        protected Vector2 _position;
        public bool IsAlive { get; private set; } = true;
        protected Enemy(Point startPosition)
        {
            _gameManager = GameManager.GetGameManager();
            
            _enemyId = _nextEnemyId++;
            _position = startPosition.ToVector2();

            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            SetCollider(_rectangleCollider);
            _activeEnemies.Add(this);
        }

        public static IEnumerable<Enemy> GetActiveEnemies()
        {
            return _activeEnemies;
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
            // X movement
            Vector2 newPosX =
                new Vector2(_position.X + velocity.X, _position.Y);

            bool blockedX = false;

            foreach (var map in _gameManager.Maps)
            {
                if (map.IsColliding(newPosX, width, height))
                {
                    blockedX = true;
                    break;
                }
            }

            if (!blockedX)
            {
                _position = newPosX;
            }

            // Y movement
            Vector2 newPosY =
                new Vector2(_position.X,
                            _position.Y + velocity.Y);

            bool blockedY = false;

            foreach (var map in _gameManager.Maps)
            {
                if (map.IsColliding(newPosY, width, height))
                {
                    blockedY = true;
                    break;
                }
            }

            if (!blockedY)
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

        protected virtual bool CanBePushedByEnemies => true;

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

            spriteBatch.Draw(
                healthBarTexture,
                new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight),
                Color.LimeGreen);
        }

        protected void KillEnemy(SoundEffect deathSfx, Action onKilled = null)
        {
            IsAlive = false;
            onKilled?.Invoke();
            deathSfx.Play();
            _gameManager.RemoveGameObject(this);
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