using System;
using SpellFall.Character;
using SpellFall.Engine;

namespace SpellFall.Weapons
{
    public abstract class Weapons : GameObject
    {
        protected readonly GameManager _gameManager;
        protected PlayerStats OwnerStats;

        public int DamageBonus { get; }
        public float FireRateBonus { get; }
        public int BaseCdFrames { get; }

        protected Weapons(int damageBonus, int baseCdFrames, float fireRateBonus = 0f)
        {
            _gameManager = GameManager.GetGameManager();
            DamageBonus = Math.Max(0, damageBonus);
            BaseCdFrames = Math.Max(0, baseCdFrames);
            FireRateBonus = fireRateBonus;
        }

        public virtual void OnEquip(PlayerStats stats)
        {
            OwnerStats = stats;
            if (OwnerStats != null)
            {
                OwnerStats.WeaponDamageBonus += DamageBonus;
                OwnerStats.WeaponFireRateBonus += FireRateBonus;
            }
        }

        public virtual void OnUnequip()
        {
            if (OwnerStats != null)
            {
                OwnerStats.WeaponDamageBonus = Math.Max(0, OwnerStats.WeaponDamageBonus - DamageBonus);
                OwnerStats.WeaponFireRateBonus = Math.Max(0f, OwnerStats.WeaponFireRateBonus - FireRateBonus);
                OwnerStats = null;
            }
        }

        protected bool TryStartPrimaryAttackCooldown()
        {
            return OwnerStats != null && OwnerStats.TryStartAttack(BaseCdFrames);
        }
    }
}