using System.Collections.Generic;
using SpellFall.Background;
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

        public void HandleCheatInput(InputManager inputManager)
        {
            if (inputManager.IsKeyPress(Microsoft.Xna.Framework.Input.Keys.B))
            {
                UnlockAllWeapons();
            }

            if (inputManager.IsKeyPress(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                ToggleCurrentMapGates();
            }
        }

        private void ToggleCurrentMapGates()
        {
            Map currentMap = _gameManager.CurrentMap;
            if (currentMap == null)
            {
                return;
            }

            List<Gate> gates = _gameManager.GetObjectsOfType<Gate>();
            bool hasOpenGate = false;

            foreach (Gate gate in gates)
            {
                if (gate.Room != currentMap)
                {
                    continue;
                }

                if (gate.IsOpen || gate.PermanentlyOpen)
                {
                    hasOpenGate = true;
                    break;
                }
            }

            if (!hasOpenGate)
            {
                _gameManager.OpenGatesForMap(currentMap, permanent: true);
                return;
            }

            foreach (Gate gate in gates)
            {
                if (gate.Room != currentMap)
                {
                    continue;
                }

                gate.SetPermanentlyOpen(false);
                gate.Deactivate();
                gate.Close();
            }
        }
    }
}