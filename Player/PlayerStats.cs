namespace SpellFall.Character
{
    public class PlayerStats
    {
        public int Level { get; private set; } = 1;

        public float Luck { get; set; } = 1f;
        public float WeaponLuckBonus { get; set; } = 0f;
        public float TotalLuck => System.MathF.Max(0f, Luck + WeaponLuckBonus);

        public int MaxHealth { get; private set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public float HealthRegenPerSecond { get; set; } = 5f;
        public float WeaponHealthRegenPerSecondBonus { get; set; } = 0f;
        public float TotalHealthRegenPerSecond => System.MathF.Max(0f, HealthRegenPerSecond + WeaponHealthRegenPerSecondBonus);
        public float HealthRegenDelaySeconds { get; set; } = 3f;
        public float WeaponHealthRegenDelaySecondsBonus { get; set; } = 0f;
        public float TotalHealthRegenDelaySeconds => System.MathF.Max(0f, HealthRegenDelaySeconds + WeaponHealthRegenDelaySecondsBonus);

        public int Damage { get; set; } = 5;
        public int WeaponDamageBonus { get; set; } = 0;
        public int TotalDamage => Damage + WeaponDamageBonus;
        public float FireRate { get; set; } = 1f;
        public float WeaponFireRateBonus { get; set; } = 0f;
        public float TotalFireRate => FireRate + WeaponFireRateBonus;
        public float Speed { get; set; } = 5f;
        public float WeaponSpeedBonus { get; set; } = 0f;
        public float TotalSpeed => System.MathF.Max(0f, Speed + WeaponSpeedBonus);

        public int AttackCdFrames { get; private set; } = 0;
        public bool CanAttack => AttackCdFrames <= 0;

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

        public bool TryStartAttack(int baseCdFrames)
        {
            if (!CanAttack)
            {
                return false;
            }

            if (baseCdFrames <= 0)
            {
                AttackCdFrames = 0;
                return true;
            }

            float rate = TotalFireRate <= 0f ? 0.01f : TotalFireRate;
            AttackCdFrames = (int)System.MathF.Ceiling(baseCdFrames / rate);
            return true;
        }

        public void DecreaseAttackCooldown()
        {
            if (AttackCdFrames > 0)
            {
                AttackCdFrames--;
            }
        }

    }
}
