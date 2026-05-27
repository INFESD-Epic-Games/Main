using System;
using SpellFall.Character;
using SpellFall.Engine;

namespace SpellFall.Weapons
{
    public abstract class Weapons : GameObject
    {
        protected readonly GameManager _gameManager;
        protected PlayerStats OwnerStats;

        public bool IsEquipped { get; private set; }

        public int DamageBonus { get; }
        public float FireRateBonus { get; }
        public float LuckBonus { get; }
        public float SpeedBonus { get; }
        public float HealthRegenPerSecondBonus { get; }
        public float HealthRegenDelaySecondsBonus { get; }
        public int BaseCdFrames { get; }

        protected Weapons(
            int damageBonus,
            int baseCdFrames,
            float fireRateBonus = 0f,
            float luckBonus = 0f,
            float speedBonus = 0f,
            float healthRegenPerSecondBonus = 0f,
            float healthRegenDelaySecondsBonus = 0f)
        {
            _gameManager = GameManager.GetGameManager();
            DamageBonus = Math.Max(0, damageBonus);
            BaseCdFrames = Math.Max(0, baseCdFrames);
            FireRateBonus = fireRateBonus;
            LuckBonus = luckBonus;
            SpeedBonus = speedBonus;
            HealthRegenPerSecondBonus = healthRegenPerSecondBonus;
            HealthRegenDelaySecondsBonus = healthRegenDelaySecondsBonus;
        }

        public virtual void OnEquip(PlayerStats stats)
        {
            OwnerStats = stats;
            IsEquipped = true;
            if (OwnerStats != null)
            {
                OwnerStats.WeaponDamageBonus += DamageBonus;
                OwnerStats.WeaponFireRateBonus += FireRateBonus;
                OwnerStats.WeaponLuckBonus += LuckBonus;
                OwnerStats.WeaponSpeedBonus += SpeedBonus;
                OwnerStats.WeaponHealthRegenPerSecondBonus += HealthRegenPerSecondBonus;
                OwnerStats.WeaponHealthRegenDelaySecondsBonus += HealthRegenDelaySecondsBonus;
            }
        }

        public virtual void OnUnequip()
        {
            IsEquipped = false;
            if (OwnerStats != null)
            {
                OwnerStats.WeaponDamageBonus = Math.Max(0, OwnerStats.WeaponDamageBonus - DamageBonus);
                OwnerStats.WeaponFireRateBonus = Math.Max(0f, OwnerStats.WeaponFireRateBonus - FireRateBonus);
                OwnerStats.WeaponLuckBonus -= LuckBonus;
                OwnerStats.WeaponSpeedBonus -= SpeedBonus;
                OwnerStats.WeaponHealthRegenPerSecondBonus -= HealthRegenPerSecondBonus;
                OwnerStats.WeaponHealthRegenDelaySecondsBonus -= HealthRegenDelaySecondsBonus;
                OwnerStats = null;
            }
        }

        protected bool TryStartPrimaryAttackCooldown()
        {
            return OwnerStats != null && OwnerStats.TryStartAttack(BaseCdFrames);
        }
    }
}