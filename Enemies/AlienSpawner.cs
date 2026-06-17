using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;
using Microsoft.Xna.Framework.Audio;

namespace SpellFall.Enemies
{
    public class AlienSpawner : Enemy
    {
        private const float SpawnerScale = 0.75f;
        private const float HitboxScale = 0.5f;
        private const int MaxHealth = 100;
        private const float SpawnIntervalSeconds = 8f;
        private const int SpawnCountPerWave = 3;
        private const int MaxAliensInGame = 20;
        private const float SpawnIndicatorDurationSeconds = 2f;
        private readonly Random _rng;
        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private float _spawnTimer;
        private SoundEffect _enemyDeathSFX;

        public AlienSpawner(Point startPosition)
            : base(startPosition, MaxHealth)
        {
            _rng = new Random();
            _spawnTimer = SpawnIntervalSeconds;
        }

        protected override SoundEffect DeathSoundEffect => _enemyDeathSFX;

        public static AlienSpawner CreateQuestSpawner()
        {
            int mapWidthTiles = 16;
            int mapHeightTiles = 16;
            int tileSizeOnScreen = 32 * 4;

            int randomTileX = Random.Shared.Next(1, mapWidthTiles - 1);
            int randomTileY = Random.Shared.Next(1, mapHeightTiles - 1);
            int tileCenterX = (randomTileX * tileSizeOnScreen) + (tileSizeOnScreen / 2);
            int tileCenterY = (randomTileY * tileSizeOnScreen) + (tileSizeOnScreen / 2);

            return new AlienSpawner(new Point(tileCenterX, tileCenterY));
        }

        protected override bool CanBePushedByEnemies => false;

        public override void Load(ContentManager content)
        {
            _enemyDeathSFX = content.Load<SoundEffect>("Enemy Death");
            _texture = content.Load<Texture2D>("AlienSpawner");
            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive)
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

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            DrawEnemySprite(
                spriteBatch,
                _texture,
                _position,
                null,
                Color.White,
                0f,
                origin,
                SpawnerScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(
                spriteBatch,
                ref _healthBarTexture,
                _texture.Height * SpawnerScale,
                CurrentHealth,
                MaxHealthValue,
                56,
                8);

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

        protected override void UpdateCollider()
        {
            int colliderWidth = Math.Max(24, (int)(_texture.Width * SpawnerScale * HitboxScale));
            int colliderHeight = Math.Max(24, (int)(_texture.Height * SpawnerScale * HitboxScale));
            UpdateCenteredCollider(colliderWidth, colliderHeight);
        }
    }
}