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

            switch (roomNumber)
            {
                case 1:
                    Point leftEyePosition = new Point(
                        (int)room.Position.X + 400,
                        (int)room.Position.Y + 800
                    );
                    Point rightEyePosition = new Point(
                        (int)room.Position.X + 1800,
                        (int)room.Position.Y + 1000
                    );

                    Point leftTopGoblinPosition = new Point(
                        (int)room.Position.X + 400,
                        (int)room.Position.Y + 400
                    );
                    Point rightTopGoblinPosition = new Point(
                        (int)room.Position.X + 2000,
                        (int)room.Position.Y + 400
                    );
                    Point leftBottomGoblinPosition = new Point(
                        (int)room.Position.X + 400,
                        (int)room.Position.Y + 1500
                    );
                    Point rightBottomGoblinPosition = new Point(
                        (int)room.Position.X + 1800,
                        (int)room.Position.Y + 1500
                    );

                    GameManager.GetGameManager().AddGameObject(new Eye(leftEyePosition));
                    GameManager.GetGameManager().AddGameObject(new Eye(rightEyePosition));
                    GameManager.GetGameManager().AddGameObject(new Goblin(leftTopGoblinPosition));
                    GameManager.GetGameManager().AddGameObject(new Goblin(rightTopGoblinPosition));
                    GameManager.GetGameManager().AddGameObject(new Goblin(leftBottomGoblinPosition));
                    GameManager.GetGameManager().AddGameObject(new Goblin(rightBottomGoblinPosition));
                    break;
                case 2:
                    Point topSpawnerPosition = new Point(
                        (int)room.Position.X + 1700,
                        (int)room.Position.Y + 700
                    );
                    Point bottomSpawnerPosition = new Point(
                        (int)room.Position.X + 1700,
                        (int)room.Position.Y + 1100
                    );

                    GameManager.GetGameManager().AddGameObject(new AlienSpawner(topSpawnerPosition));
                    GameManager.GetGameManager().AddGameObject(new AlienSpawner(bottomSpawnerPosition));
                    break;
                case 3:
                    Point bishopSpawnPosition = new Point(
                        (int)room.Position.X + 1500,
                        (int)room.Position.Y + 900
                    );
                    Point leftGhostSpawnPosition = new Point(
                        (int)room.Position.X + 1300,
                        (int)room.Position.Y + 600
                    );
                    Point rightGhostSpawnPosition = new Point(
                        (int)room.Position.X + 1300,
                        (int)room.Position.Y + 1200
                    );
                    Point weepingAngelSpawnPosition = new Point(
                        (int)room.Position.X + 1200,
                        (int)room.Position.Y + 900
                    );

                    GameManager.GetGameManager().AddGameObject(new Bishop(bishopSpawnPosition));
                    GameManager.GetGameManager().AddGameObject(new Ghost(leftGhostSpawnPosition));
                    GameManager.GetGameManager().AddGameObject(new Ghost(rightGhostSpawnPosition));
                    GameManager.GetGameManager().AddGameObject(new WeepingAngel(weepingAngelSpawnPosition));
                    break;
            }
        }
    }
}