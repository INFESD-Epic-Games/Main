using Microsoft.Xna.Framework;
using SpellFall.Engine;
using SpellFall.Enemies;

namespace SpellFall.Items
{
    // Temporary testing helper: drops a chest when all alive enemies are cleared.
    public class EnemyClearChestTestSpawner : GameObject
    {
        private readonly GameManager _gameManager;
        private bool _hadAnyEnemyLastFrame;

        public EnemyClearChestTestSpawner()
        {
            _gameManager = GameManager.GetGameManager();
            _hadAnyEnemyLastFrame = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (_gameManager.Player == null)
            {
                base.Update(gameTime);
                return;
            }

            bool hasAnyEnemy = HasAnyAliveEnemy();
            if (_hadAnyEnemyLastFrame && !hasAnyEnemy && !HasActiveChest())
            {
                Vector2 chestPosition = _gameManager.Player.GetPosition().Center.ToVector2() + new Vector2(0f, -96f);
                _gameManager.AddGameObject(new Loot(chestPosition, _gameManager.Player.Stats.TotalLuck));
            }

            _hadAnyEnemyLastFrame = hasAnyEnemy;
            base.Update(gameTime);
        }

        private static bool HasAnyAliveEnemy()
        {
            foreach (Enemy enemy in Enemy.GetActiveEnemies())
            {
                if (enemy.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasActiveChest()
        {
            return _gameManager.GetObjectsOfType<Loot>().Count > 0;
        }
    }
}