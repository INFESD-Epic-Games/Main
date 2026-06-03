using System.Collections.Generic;
using SpellFall.Engine;
using SpellFall.Weapons;
using WeaponBase = SpellFall.Weapons.Weapons;

namespace SpellFall.Character
{
    public partial class Player
    {
        public bool EquipWeaponBySlot(int oneBasedSlot)
        {
            WeaponBase weapon = GetOwnedWeaponAtSlot(oneBasedSlot);
            if (weapon == null)
            {
                return false;
            }

            EquipWeapon(weapon);
            return true;
        }

        public void UnlockAllWeapons()
        {
            List<WeaponBase> worldWeapons = _gameManager.GetObjectsOfType<WeaponBase>();

            foreach (WeaponBase weapon in worldWeapons)
            {
                AddWeaponToInventory(weapon);
            }
        }
    }
}