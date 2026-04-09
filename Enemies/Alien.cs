using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;
using SpellFall.Items;
using SpellFall.Character;
using SpellFall.Quests;

namespace SpellFall.Enemies
{
    public class Alien : GameObject
    {
        private const float MoveSpeed = 70f;
        private const float AlienScale = 0.5f;
        private const float HitboxScale = 0.4f;
        private const int MaxHealth = 20;
        private const int ContactDamage = 5;
        private const float ContactCooldownSeconds = 3f;

        private readonly GameManager _gameManager;
        private readonly RectangleCollider _rectangleCollider;

        private Texture2D _texture;
        private Texture2D _healthBarTexture;
        private Vector2 _position;
        private int _currentHealth;
        private bool _isDead;
        private float _contactCooldownTimer;
        private Random Randomnum = new Random();
        private Loot loot = new Loot();
        private int _frameWidth;
        private int _frameHeight;

        public Alien(Point startPosition)
        {
            _gameManager = GameManager.GetGameManager();
            _position = startPosition.ToVector2();
            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            _currentHealth = MaxHealth;
            _isDead = false;
            _contactCooldownTimer = 0f;
            SetCollider(_rectangleCollider);
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

            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            Vector2 directionToPlayer = playerPosition - _position;

            if (directionToPlayer != Vector2.Zero)
            {
                directionToPlayer.Normalize();
                _position += directionToPlayer * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            UpdateCollider();
            base.Update(gameTime);
        }

        public void RandomDropchance()
        {
            int rng = Randomnum.Next(0, 100);
            if (rng >= 90)
            {
                loot.GetRandomRarity(_gameManager.Player.Stats.Luck);
            }
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Arrow arrow)
            {
                _gameManager.RemoveGameObject(other);
                TakeDamage(arrow.Damage);
            }
            else if (other is Player && _contactCooldownTimer <= 0f)
            {
                _gameManager.Player.HealthBar.TakeDamage(ContactDamage);
                _contactCooldownTimer = ContactCooldownSeconds;
            }

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
                AlienScale,
                SpriteEffects.None,
                0f);

            DrawHealthBar(spriteBatch);

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
            _gameManager.RemoveGameObject(this);
            _gameManager.QuestManager.AddProgress("KillAliens", 1);
            RandomDropchance();
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            int barWidth = 40;
            int barHeight = 6;
            float healthPercentage = (float)_currentHealth / MaxHealth;
            int barX = (int)(_position.X - barWidth / 2f);
            int barY = (int)(_position.Y - (_frameHeight * AlienScale / 2f) - 16);

            spriteBatch.Draw(
                _healthBarTexture,
                new Rectangle(barX, barY, barWidth, barHeight),
                Color.DarkRed);

            spriteBatch.Draw(
                _healthBarTexture,
                new Rectangle(barX, barY, (int)(barWidth * healthPercentage), barHeight),
                Color.LimeGreen);
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

        private void UpdateCollider()
        {
            int colliderWidth = (int)(_frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(_frameHeight * AlienScale * HitboxScale);
            Point colliderLocation = (_position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();

            _rectangleCollider.shape = new Rectangle(colliderLocation, new Point(colliderWidth, colliderHeight));
        }
    }
}
