using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public abstract class Enemy : GameObject
    {
        private static int _nextEnemyId = 1;

        protected readonly GameManager _gameManager;
        protected readonly RectangleCollider _rectangleCollider;
        protected readonly int _enemyId;
        protected Vector2 _position;

        protected Enemy(Point startPosition)
        {
            _gameManager = GameManager.GetGameManager();
            _enemyId = _nextEnemyId++;
            _position = startPosition.ToVector2();
            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            SetCollider(_rectangleCollider);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Enemy otherEnemy)
            {
                ResolveEnemyCollision(otherEnemy);
            }

            base.OnCollision(other);
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