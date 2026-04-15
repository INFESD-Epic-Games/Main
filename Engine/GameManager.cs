using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Quests;
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
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public SoundManager SoundManager { get; private set; } = new SoundManager();
        private Song _battleMusic;
        private Song _overworldmusic;
        private bool _isBattleMusicPlaying = false;

        private bool _isOverWorldMusicPlaying = false;

        public static GameManager GetGameManager()
        {
            if(gameManager == null)
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
            SoundManager = new SoundManager();
        }

        public void Initialize(ContentManager content, Game game, Player player)
        {
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
                for (int j = i+1; j < _gameObjects.Count; j++)
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

            foreach(GameObject obj in _gameObjects)
            {
                if (obj is SpellFall.Enemies.Alien alien)
                {
                    Vector2 screenPos = Vector2.Transform(
                        alien.GetPosition(),
                        Camera.Transform
                    );

                    if (screenPos.X >= 0 && screenPos.X <= Game.GraphicsDevice.Viewport.Width &&
                        screenPos.Y >= 0 && screenPos.Y <= Game.GraphicsDevice.Viewport.Height)
                    {
                        enemyOnScreen = true;
                        break;
                    }
                }
                if (obj is SpellFall.Enemies.AlienSpawner alienSpawner)
                {
                    Vector2 screenPos = Vector2.Transform(
                        alienSpawner.GetPosition(),
                        Camera.Transform
                    );

                    if (screenPos.X >= 0 && screenPos.X <= Game.GraphicsDevice.Viewport.Width &&
                        screenPos.Y >= 0 && screenPos.Y <= Game.GraphicsDevice.Viewport.Height)
                    {
                        enemyOnScreen = true;
                        break;
                    }
                }
            }

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
            }
            
            else if (!enemyOnScreen && _isBattleMusicPlaying)
            {
                MediaPlayer.Stop();
                _isBattleMusicPlaying = false;
                MediaPlayer.Play(_overworldmusic);
                MediaPlayer.IsRepeating = true;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) 
        {
            // spriteBatch.Begin();
            spriteBatch.Begin(transformMatrix: Camera.Transform);
            foreach (GameObject gameObject in _gameObjects)
            {
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

        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, Game.GraphicsDevice.Viewport.Width),
                RNG.Next(0, Game.GraphicsDevice.Viewport.Height));
        }
    }
}