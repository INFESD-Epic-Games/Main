using System;
using SpellFall.Character;
using SpellFall.Engine;
using SpellFall.Items;

namespace SpellFall.Weapons
{
    public abstract class Weapons : GameObject, ILootable
    {
        protected readonly GameManager _gameManager;
        protected PlayerStats OwnerStats;

        public bool IsEquipped { get; private set; }

        private readonly int _baseDamageBonus;
        private readonly float _baseFireRateBonus;
        private readonly float _baseLuckBonus;
        private readonly float _baseSpeedBonus;
        private readonly float _baseHealthRegenPerSecondBonus;
        private readonly float _baseHealthRegenDelaySecondsBonus;
        private int _lootDamageModifier;

        public string LootTierName { get; private set; } = "Unmodified";

        public int DamageBonus => Math.Max(0, _baseDamageBonus + _lootDamageModifier);
        public float FireRateBonus => _baseFireRateBonus;
        public float LuckBonus => _baseLuckBonus;
        public float SpeedBonus => _baseSpeedBonus;
        public float HealthRegenPerSecondBonus => _baseHealthRegenPerSecondBonus;
        public float HealthRegenDelaySecondsBonus => _baseHealthRegenDelaySecondsBonus;
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
            _baseDamageBonus = Math.Max(0, damageBonus);
            BaseCdFrames = Math.Max(0, baseCdFrames);
            _baseFireRateBonus = fireRateBonus;
            _baseLuckBonus = luckBonus;
            _baseSpeedBonus = speedBonus;
            _baseHealthRegenPerSecondBonus = healthRegenPerSecondBonus;
            _baseHealthRegenDelaySecondsBonus = healthRegenDelaySecondsBonus;
            _lootDamageModifier = 0;
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

        public void ApplyLootTier(string tierName)
        {
            string resolvedTier = string.IsNullOrWhiteSpace(tierName) ? "Unmodified" : tierName;
            int previousDamageBonus = DamageBonus;

            _lootDamageModifier = Loot.GetWeaponDamageModifier(resolvedTier);
            LootTierName = resolvedTier;

            if (OwnerStats != null)
            {
                int newDamageBonus = DamageBonus;
                OwnerStats.WeaponDamageBonus += newDamageBonus - previousDamageBonus;
            }
        }

        protected bool TryStartPrimaryAttackCooldown()
        {
            return OwnerStats != null && OwnerStats.TryStartAttack(BaseCdFrames);
        }
    }
}