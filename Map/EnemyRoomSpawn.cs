using Microsoft.Xna.Framework;
using SpellFall.Enemies;
using SpellFall.Engine;

namespace SpellFall.Background
{
    public static class EnemyRoomSpawn
    {
        public static void SpawnEnemiesForRoom(Map room, int roomNumber)
        {
            if (room == null || room.EnemiesSpawned)
            {
                return;
            }

            room.EnemiesSpawned = true;

            // Example: Spawn different enemies based on the room number
            switch (roomNumber)
            {
                case 1:
                    // Use room position for spawning when player reference is not available here
                    Point projectileEnemyPosition = new Point(
                        (int)room.Position.X + 300,
                        (int)room.Position.Y + 300
                    );
                    GameManager.GetGameManager().AddGameObject(new Goblin(projectileEnemyPosition));
                    break;
                case 2:
                    Point spawnPosition = new Point(
                        (int)room.Position.X + 1000,
                        (int)room.Position.Y + 1000
                    );
                    GameManager.GetGameManager().AddGameObject(new Eye(spawnPosition));
                    break;
                case 3:
                    Point alienSpawnPosition = new Point(
                        (int)room.Position.X + 500,
                        (int)room.Position.Y + 500
                    );
                    GameManager.GetGameManager().AddGameObject(new Alien(alienSpawnPosition));
                    break;
            }
        }
    }
}