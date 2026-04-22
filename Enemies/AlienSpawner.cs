using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Items;
using SpellFall.Weapons.Projectiles;
using Microsoft.Xna.Framework.Audio;

namespace SpellFall.Enemies
{
    public class AlienSpawner : GameObject
    {
        private const float SpawnerScale = 0.75f;
        private const float HitboxScale = 0.5f;
        private const int MaxHealth = 100;
        private const float SpawnIntervalSeconds = 8f;
        private const int SpawnCountPerWave = 3;
        private const int MaxAliensInGame = 20;
        private const float SpawnIndicatorDurationSeconds = 2f;
        private readonly GameManager _gameManager;
        private readonly RectangleCollider _rectangleCollider;
        private readonly Random _rng;
        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private Vector2 _position;
        private int _currentHealth;
        private bool _isDead;
        private float _spawnTimer;
        private SoundEffect _enemyDeathSFX;

        public AlienSpawner(Point startPosition)
        {
            _gameManager = GameManager.GetGameManager();
            _rng = new Random();
            _position = startPosition.ToVector2();
            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            _currentHealth = MaxHealth;
            _isDead = false;
            _spawnTimer = 0f;
            SetCollider(_rectangleCollider);
        }

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("AlienSpawner");
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_isDead)
            {
                base.Update(gameTime);
                return;
            }

            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_spawnTimer >= SpawnIntervalSeconds)
            {
                _spawnTimer = 0f;
                QueueSpawnWave();
            }

            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Arrow arrow)
            {
                _gameManager.RemoveGameObject(other);
                TakeDamage(arrow.Damage);
            }

            base.OnCollision(other);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _position,
                null,
                Color.White,
                0f,
                origin,
                SpawnerScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(spriteBatch);

            base.Draw(gameTime, spriteBatch);
        }

        private void QueueSpawnWave()
        {
            int availableSlots = MaxAliensInGame - _gameManager.GetAlienCount();
            if (availableSlots <= 0)
            {
                return;
            }

            int spawnCount = Math.Min(SpawnCountPerWave, availableSlots);
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 spawnPosition = GetSpawnPosition();
                Point alienPoint = spawnPosition.ToPoint();

                _gameManager.AddGameObject(new SpawnIndicator(
                    spawnPosition,
                    SpawnIndicatorDurationSeconds,
                    () => TrySpawnAlien(alienPoint)));
            }
        }

        private void TrySpawnAlien(Point spawnPoint)
        {
            if (_gameManager.GetAlienCount() >= MaxAliensInGame)
            {
                return;
            }

            _gameManager.AddGameObject(new Alien(spawnPoint));
        }

        private Vector2 GetSpawnPosition()
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2d);
            float distance = _rng.Next(120, 260);
            return _position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
        }

        private void TakeDamage(int damage)
        {
            if (_isDead || damage <= 0)
            {
                return;
            }

            _currentHealth -= damage;
            if (_currentHealth > 0)
            {
                return;
            }

            _isDead = true;
            _gameManager.AddGameObject(new Loot(_position, _gameManager.Player.Stats.TotalLuck));
            if (_isDead)
            {
                _enemyDeathSFX.Play();
            }
            _gameManager.RemoveGameObject(this);
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            int barWidth = 56;
            int barHeight = 8;
            float healthPercentage = (float)_currentHealth / MaxHealth;
            int barX = (int)(_position.X - barWidth / 2f);
            int barY = (int)(_position.Y - (_texture.Height * SpawnerScale / 2f) - 16);

            spriteBatch.Draw(
                _healthBarTexture,
                new Rectangle(barX, barY, barWidth, barHeight),
                Color.DarkRed);

            spriteBatch.Draw(
                _healthBarTexture,
                new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight),
                Color.LimeGreen);
        }

        private void UpdateCollider()
        {
            int colliderWidth = Math.Max(24, (int)(_texture.Width * SpawnerScale * HitboxScale));
            int colliderHeight = Math.Max(24, (int)(_texture.Height * SpawnerScale * HitboxScale));
            Point colliderLocation = (_position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();

            _rectangleCollider.shape = new Rectangle(colliderLocation, new Point(colliderWidth, colliderHeight));
        }

        public Vector2 GetPosition()
        {
            return _position;
        }
    }
}