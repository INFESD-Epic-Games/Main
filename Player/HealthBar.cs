using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Character
{
    public class HealthBar: GameObject
    {
        public int maxHealth { get; private set; }
        private int _currentHealth;
        public int currentHealth { get { return _currentHealth; } }
        private readonly PlayerStats _playerStats;
        private Rectangle _ownerBounds;
        private Texture2D _healthBarTexture;

        private float _regenTimer = 0f;
        private float _regenBuffer = 0f;

        public HealthBar(PlayerStats playerStats)
        {
            _playerStats = playerStats;
            maxHealth = playerStats.MaxHealth;
            _currentHealth = playerStats.CurrentHealth;
        }

        public void SetPosition(Rectangle ownerBounds)
        {
            _ownerBounds = ownerBounds;
        }

        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0)
                _currentHealth = 0;

            _playerStats.CurrentHealth = _currentHealth;
            _regenTimer = 0f; // Reset regen timer so it waits again
            _regenBuffer = 0f;
        }
        public void IncreaseMaxHealth(int amount)
        {
            maxHealth += amount;

            _currentHealth += amount;

            if (_currentHealth > maxHealth)
                _currentHealth = maxHealth;
        }
        
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Only regenerate if not at max health
            if (_currentHealth >= maxHealth)
            {
                base.Update(gameTime);
                return;
            }
            // Wait before regenerating (delay after taking damage)
            _regenTimer += deltaTime;
            float regenDelay = _playerStats.HealthRegenDelaySeconds;
            if (_regenTimer >= regenDelay)
            {
                float regenRate = _playerStats.HealthRegenPerSecond;
                _regenBuffer += regenRate * deltaTime;
                int healthToAdd = (int)_regenBuffer;
                if (healthToAdd > 0)
                {
                    _currentHealth += healthToAdd;
                    _regenBuffer -= healthToAdd;
                }

                if (_currentHealth > maxHealth)
                    _currentHealth = maxHealth;
                _playerStats.CurrentHealth = _currentHealth;
            }

            base.Update(gameTime);
        }

        public void DrawHealthBar(SpriteBatch spriteBatch)
        {
            if (_healthBarTexture == null)
            {
                _healthBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _healthBarTexture.SetData(new[] { Color.White });
            }

            int barWidth = 200;
            int barHeight = 20;

            float healthPercentage = (float)currentHealth / maxHealth;

            Vector2 barPosition = new Vector2(
                _ownerBounds.Center.X - (barWidth / 2f),
                _ownerBounds.Y + 150 // als jullie dit te hoog of te laag vinden, pas het aan of een betere berekening gebruiken ik heb geen idee
            );

            // Background
            spriteBatch.Draw(_healthBarTexture,
                new Rectangle((int)barPosition.X, (int)barPosition.Y, barWidth, barHeight),
                Color.DarkRed);

            // Foreground
            spriteBatch.Draw(_healthBarTexture,
                new Rectangle((int)barPosition.X, (int)barPosition.Y, (int)(barWidth * healthPercentage), barHeight),
                Color.LimeGreen);
        }   

    }
}