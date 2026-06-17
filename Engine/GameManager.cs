using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Quests;
using SpellFall.UI;
using SpellFall.Background;
using SpellFall.Items;
using Microsoft.Xna.Framework.Media;

namespace SpellFall.Engine
{
    public class GameManager
    {
        private static GameManager gameManager;

        private List<GameObject> _gameObjects;
        private List<GameObject> _toBeRemoved;
        private List<GameObject> _toBeAdded;
        private ContentManager _content;


        public Random RNG { get; private set; }
        public Camera Camera { get; private set; }
        public Player Player { get; private set; }
        public TextBubble textBubble { get; private set; }
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public SoundManager SoundManager { get; private set; } = new SoundManager();
        private Song _battleMusic;
        private Song _overworldmusic;
        private bool _isBattleMusicPlaying = false;
        private TimeSpan _battleMusicTimer = TimeSpan.Zero;
        private TimeSpan _battleMusicBufferTime = TimeSpan.FromSeconds(1);
        public List<Map> Maps { get; } = new List<Map>();
        public Map CurrentMap { get; set; }
        private bool _isOverWorldMusicPlaying = false;

        public static GameManager GetGameManager()
        {
            if (gameManager == null)
                gameManager = new GameManager();
            return gameManager;
        }
        public GameManager()
        {
            _gameObjects = new List<GameObject>();
            _toBeRemoved = new List<GameObject>();
            _toBeAdded = new List<GameObject>();
            InputManager = new InputManager();
            Camera = new Camera();
            RNG = new Random();


            textBubble = new TextBubble();
            _toBeAdded.Add(textBubble);
            _toBeAdded.Add(new ControlLayoutOverlay());

            SoundManager = new SoundManager();
        }

        public void Initialize(ContentManager content, Game game, Player player)
        {
            ResetWorldState();
            Game = game;
            _content = content;
            Player = player;

            QuestManager = new QuestManager();
        }

        public void Load(ContentManager content)
        {
            _battleMusic = content.Load<Song>("Battle 1");
            _overworldmusic = content.Load<Song>("overworld music");

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load(content);
            }
        }

        public void HandleInput(InputManager inputManager)
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.HandleInput(this.InputManager);
            }
        }

        public void CheckCollision()
        {
            // Checks once for every pair of 2 GameObjects if the collide.
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                for (int j = i + 1; j < _gameObjects.Count; j++)
                {
                    if (_gameObjects[i].CheckCollision(_gameObjects[j]))
                    {
                        _gameObjects[i].OnCollision(_gameObjects[j]);
                        _gameObjects[j].OnCollision(_gameObjects[i]);
                    }
                }
            }

        }

        public void Update(GameTime gameTime)
        {
            if (GameState.InMainMenu) return;
            if (GameState.IsPaused) return;
            if (GameState.InGameOver) return;

            InputManager.Update();

            // Handle input
            HandleInput(InputManager);


            // Update
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Update(gameTime);
            }

            Camera.Follow(Player);


            // Check Collission
            CheckCollision();

            foreach (GameObject gameObject in _toBeAdded)
            {
                gameObject.Load(_content);
                _gameObjects.Add(gameObject);
            }
            _toBeAdded.Clear();

            foreach (GameObject gameObject in _toBeRemoved)
            {
                gameObject.Destroy();
                _gameObjects.Remove(gameObject);
            }
            _toBeRemoved.Clear();

            bool enemyOnScreen = false;
            int viewWidth = RenderManager.VirtualWidth;
            int viewHeight = RenderManager.VirtualHeight;

            // foreach(GameObject obj in _gameObjects)
            // {
            //     if (obj is SpellFall.Enemies.Enemy enemy)
            //     {
            //         Vector2 screenPos = Vector2.Transform(
            //             enemy.GetPosition(),
            //             Camera.Transform
            //         );

            //         if (screenPos.X >= 0 && screenPos.X <= viewWidth &&
            //             screenPos.Y >= 0 && screenPos.Y <= viewHeight)
            //         {
            //             enemyOnScreen = true;
            //             break;
            //         }
            //     }
            // }

            if (!enemyOnScreen && !_isOverWorldMusicPlaying)
            {
                MediaPlayer.Play(_overworldmusic);
                MediaPlayer.IsRepeating = true;
                _isOverWorldMusicPlaying = true;
            }


            else if (enemyOnScreen && !_isBattleMusicPlaying)
            {
                MediaPlayer.Play(_battleMusic);
                MediaPlayer.IsRepeating = true;
                _isBattleMusicPlaying = true;
                _battleMusicTimer = TimeSpan.Zero;
            }

            else if (enemyOnScreen && _isBattleMusicPlaying)
            {
                _battleMusicTimer = TimeSpan.Zero;
            }

            else if (!enemyOnScreen && _isBattleMusicPlaying)
            {
                if (_battleMusicTimer > _battleMusicBufferTime)
                {
                    MediaPlayer.Stop();
                    _isBattleMusicPlaying = false;
                    _battleMusicTimer = TimeSpan.Zero;
                    MediaPlayer.Play(_overworldmusic);
                    MediaPlayer.IsRepeating = true;
                }
                else
                {
                    _battleMusicTimer += gameTime.ElapsedGameTime;
                }
            }

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: Camera.Transform);

            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject is not Map)
                {
                    continue;
                }

                gameObject.Draw(gameTime, spriteBatch);
            }

            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject is not ControlLayoutOverlay)
                {
                    continue;
                }

                gameObject.Draw(gameTime, spriteBatch);
            }

            // Draw the remaining world objects on top of the map.
            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject is Map || gameObject is TextBubble || gameObject is ControlLayoutOverlay)
                {
                    continue;
                }

                gameObject.Draw(gameTime, spriteBatch);
            }

            // Draw UI dialogue on top so it is never hidden behind map/entities.
            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject is not TextBubble)
                {
                    continue;
                }

                gameObject.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Add a new GameObject to the GameManager. 
        /// The GameObject will be added at the start of the next Update step. 
        /// Once it is added, the GameManager will ensure all steps of the game loop will be called on the object automatically. 
        /// </summary>
        /// <param name="gameObject"> The GameObject to add. </param>
        public void AddGameObject(GameObject gameObject)
        {
            _toBeAdded.Add(gameObject);
        }

        /// <summary>
        /// Remove GameObject from the GameManager. 
        /// The GameObject will be removed at the start of the next Update step and its Destroy() mehtod will be called.
        /// After that the object will no longer receive any updates.
        /// </summary>
        /// <param name="gameObject"> The GameObject to Remove. </param>
        public void RemoveGameObject(GameObject gameObject)
        {
            _toBeRemoved.Add(gameObject);
        }

        private void ResetWorldState()
        {
            SpellFall.Enemies.Enemy.ResetActiveEnemies();
            _gameObjects.Clear();
            _toBeRemoved.Clear();
            _toBeAdded.Clear();
            Maps.Clear();
            CurrentMap = null;
            textBubble?.Hide();
            textBubble?.SetText(string.Empty);
            _toBeAdded.Add(textBubble);
            _toBeAdded.Add(new ControlLayoutOverlay());
            Player = null;

            _isBattleMusicPlaying = false;
            _isOverWorldMusicPlaying = false;
            _battleMusicTimer = TimeSpan.Zero;
        }

        public void ClearWorldState()
        {
            ResetWorldState();
        }

        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, RenderManager.VirtualWidth),
                RNG.Next(0, RenderManager.VirtualHeight));
        }

        public int GetAlienCount(bool includePendingChanges = true)
        {
            int alienCount = 0;

            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject is SpellFall.Enemies.Alien)
                {
                    alienCount++;
                }
            }

            if (includePendingChanges)
            {
                foreach (GameObject gameObject in _toBeAdded)
                {
                    if (gameObject is SpellFall.Enemies.Alien)
                    {
                        alienCount++;
                    }
                }

                foreach (GameObject gameObject in _toBeRemoved)
                {
                    if (gameObject is SpellFall.Enemies.Alien)
                    {
                        alienCount--;
                    }
                }
            }

            return Math.Max(0, alienCount);
        }

        public List<T> GetObjectsOfType<T>() where T : class
        {
            List<T> result = new List<T>();

            foreach (var o in _gameObjects)
            {
                if (o is T matchedObject)
                {
                    result.Add(matchedObject);
                }
            }

            return result;
        }

        /// <summary>
        /// Open or activate all gates that belong to the given map.
        /// If <c>permanent</c> is true the gates will be set permanently open,
        /// otherwise they will be activated so their normal enemy-based logic applies.
        /// </summary>
        public void OpenGatesForMap(Map map, bool permanent = true)
        {
            if (map == null) return;

            var gates = GetObjectsOfType<SpellFall.Background.Gate>();
            foreach (var gate in gates)
            {
                if (gate.Room == map)
                {
                    if (permanent)
                        gate.SetPermanentlyOpen(true);
                    else
                        gate.Activate();
                }
            }
        }

        public void CloseGatesForMap(Map map)
        {
            if (map == null) return;

            var gates = GetObjectsOfType<SpellFall.Background.Gate>();
            foreach (var gate in gates)
            {
                if (gate.Room == map)
                {
                    gate.Deactivate();
                }
            }
        }

        /// <summary>
        /// Spawn a loot chest at the given world position.
        /// </summary>
        public void SpawnLootChest(Vector2 position, float luck)
        {
            AddGameObject(new Loot(position, luck));
        }

    }

}
