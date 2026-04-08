namespace SpellFall.Character
{
    public class PlayerStats
    {
        public int Level { get; private set; } = 1;

        public float Luck { get; set; } = 1f;

        public int MaxHealth { get; private set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public float HealthRegenPerSecond { get; set; } = 5f;
        public float HealthRegenDelaySeconds { get; set; } = 3f;

        public int Damage { get; set; } = 5;
        public float AttackSpeed { get; set; } = 1f;
        public float Speed { get; set; } = 5f;

        public PlayerStats()
        {
            CurrentHealth = MaxHealth;
        }

        public void IncreaseMaxHealth(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MaxHealth += amount;
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }
    }
}
