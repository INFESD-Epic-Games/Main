using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Items
{
    public class LootTier
    {
        public string Name;
        public float BaseWeight;
        public float RarityFactor;

        public LootTier(string name, float weight, float rarityFactor)
        {
            Name = name;
            BaseWeight = weight;
            RarityFactor = rarityFactor;
        }
    }

    public class Loot : GameObject
    {
        private const float LootScale = 0.5f;
        private const float FrameDurationSeconds = 0.2f;
        private const int FrameCount = 3;
        private const float OpenedLifetimeSeconds = 4f;

        private readonly Random _rng = new Random();
        private readonly Vector2 _position;
        private readonly RectangleCollider _rectangleCollider;
        private readonly List<LootTier> lootTable = new List<LootTier>()
        {
            new LootTier("Rusty", 50f, 1f),
            new LootTier("Common", 30f, 1f),
            new LootTier("Uncommon", 15f, 2f),
            new LootTier("Rare", 8f, 3f),
            new LootTier("Epic", 4f, 4f),
            new LootTier("Legendary", 2f, 5f),
            new LootTier("Mythic", 1f, 6f),
        };

        private Texture2D _texture;
        private int _frameWidth;
        private int _frameHeight;
        private int _currentFrame;
        private float _frameTimer;
        private float _openedLifetime;
        private bool _isOpening;
        private bool _hasRolled;

        private readonly float _luck;

        public LootTier RolledTier { get; private set; }

        public Loot(Vector2 position, float luck)
        {
            _position = position;
            _luck = luck;
            _rectangleCollider = new RectangleCollider(new Rectangle(position.ToPoint(), Point.Zero));
            SetCollider(_rectangleCollider);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Treasure_chest_horizontal");
            _frameWidth = _texture.Width / FrameCount;
            _frameHeight = _texture.Height;
            UpdateCollider();

            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_isOpening)
            {
                _openedLifetime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_openedLifetime >= OpenedLifetimeSeconds)
                {
                    GameManager.GetGameManager().RemoveGameObject(this);
                    return;
                }

                _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_frameTimer >= FrameDurationSeconds && _currentFrame < FrameCount - 1)
                {
                    _frameTimer = 0f;
                    _currentFrame++;
                }
            }

            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Player && !_isOpening)
            {
                _isOpening = true;
                RollLootIfNeeded();
                
                // Add loot to inventory
            }

            base.OnCollision(other);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight / 2f);

            spriteBatch.Draw(
                _texture,
                _position,
                sourceRectangle,
                Color.White,
                0f,
                origin,
                LootScale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }

        float GetModifiedWeight(LootTier entry, float luck)
        {
            return entry.BaseWeight * (1 + luck * entry.RarityFactor * 1f);
        }

        public LootTier GetRandomRarity(float luck)
        {
            float totalWeight = 0f;

            List<float> modifiedWeights = new List<float>();

            foreach (var entry in lootTable)
            {
                float weight = GetModifiedWeight(entry, luck);
                modifiedWeights.Add(weight);
                totalWeight += weight;
            }

            double roll = _rng.NextDouble() * totalWeight;

            float cumulative = 0f;

            for (int i = 0; i < lootTable.Count; i++)
            {
                cumulative += modifiedWeights[i];

                if (roll <= cumulative)
                {
                    Console.WriteLine(lootTable[i].Name);
                    return lootTable[i];
                }
            }

            Console.WriteLine("fallback");
            return lootTable[0];
        }

        private void RollLootIfNeeded()
        {
            if (_hasRolled)
            {
                return;
            }

            RolledTier = GetRandomRarity(_luck);
            _hasRolled = true;
        }

        private void UpdateCollider()
        {
            int colliderWidth = Math.Max(24, (int)(_frameWidth * LootScale * 0.8f));
            int colliderHeight = Math.Max(24, (int)(_frameHeight * LootScale * 0.8f));
            Point colliderLocation = (_position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();

            _rectangleCollider.shape = new Rectangle(colliderLocation, new Point(colliderWidth, colliderHeight));
        }
    }
}