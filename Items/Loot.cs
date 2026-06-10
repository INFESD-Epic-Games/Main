using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Character;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Weapons;

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
        private const float LootScale = 0.75f;
        private const float FrameDurationSeconds = 0.2f;
        private const int FrameCount = 3;
        private const float OpenedLifetimeSeconds = 5f;
        private const float SpinDurationSeconds = 3.2f;
        private const float RewardHoverDurationSeconds = 1.3f;
        private const float SpinFrameDurationSeconds = 0.15f;
        private const float SpinLoopSeconds = 1.25f;
        private const int SpinLaneCount = 5;
        private const float SpinStartOffsetY = 100f;
        private const float SpinEndOffsetY = -180f;
        private const float TierCircleScale = 1.5f;

        private readonly Random _rng = new Random();
        private readonly GameManager _gameManager;
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
        private readonly List<WeaponSpinEntry> _weaponSpinEntries = new List<WeaponSpinEntry>();
        private Texture2D _tierCircleTexture;
        private float _spinElapsed;
        private float _rewardHoverElapsed;
        private bool _rewardGranted;
        public bool IsCollected => _rewardGranted;
        private WeaponSpinEntry? _selectedReward;

        private readonly float _luck;

        public LootTier RolledTier { get; private set; }

        private readonly struct WeaponSpinEntry
        {
            public Type WeaponType { get; }
            public Texture2D Texture { get; }
            public float IconScale { get; }
            public float RewardScale { get; }

            public WeaponSpinEntry(Type weaponType, Texture2D texture, float rewardScale, float iconScale)
            {
                WeaponType = weaponType;
                Texture = texture;
                RewardScale = rewardScale;
                IconScale = iconScale;
            }
        }

        public static int GetWeaponDamageModifier(string tierName)
        {
            return tierName switch
            {
                "Rusty" => -5,
                "Common" => 5,
                "Uncommon" => 8,
                "Rare" => 12,
                "Epic" => 16,
                "Legendary" => 20,
                "Mythic" => 25,
                _ => 0,
            };
        }

        public Loot(Vector2 position, float luck)
        {
            _gameManager = GameManager.GetGameManager();
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
            TryLoadWeaponSpinTextures(content);
            _tierCircleTexture = CreateCircleTexture(content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService, 48);
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

                if (_spinElapsed < SpinDurationSeconds && _weaponSpinEntries.Count > 0)
                {
                    _spinElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else if (_selectedReward.HasValue && !_rewardGranted)
                {
                    _rewardHoverElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_rewardHoverElapsed >= RewardHoverDurationSeconds)
                    {
                        GrantRewardIfPossible();
                        _rewardGranted = true;
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Player && !_isOpening)
            {
                _isOpening = true;
                _spinElapsed = 0f;
                _rewardHoverElapsed = 0f;
                _rewardGranted = false;
                RollLootIfNeeded();
                SelectRewardWeapon();
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

            DrawWeaponSpinEffect(spriteBatch);

            base.Draw(gameTime, spriteBatch);
        }

        private void DrawWeaponSpinEffect(SpriteBatch spriteBatch)
        {
            if (!_isOpening || _weaponSpinEntries.Count == 0 || _spinElapsed >= SpinDurationSeconds)
            {
                DrawSelectedRewardHover(spriteBatch);
                return;
            }

            int frameIndex = (int)(_spinElapsed / SpinFrameDurationSeconds);
            for (int lane = 0; lane < SpinLaneCount; lane++)
            {
                float laneProgress = ((_spinElapsed / SpinLoopSeconds) + lane * 0.18f) % 1f;
                float yOffset = MathHelper.Lerp(SpinStartOffsetY, SpinEndOffsetY, laneProgress);
                float alpha = GetSpinAlpha(laneProgress);

                int entryIndex = (frameIndex + lane * 2) % _weaponSpinEntries.Count;
                WeaponSpinEntry entry = _weaponSpinEntries[entryIndex];
                Vector2 origin = new Vector2(entry.Texture.Width / 2f, entry.Texture.Height / 2f);
                Vector2 drawPosition = _position + new Vector2(0f, yOffset);

                spriteBatch.Draw(
                    entry.Texture,
                    drawPosition,
                    null,
                    Color.White * alpha,
                    0f,
                    origin,
                    entry.IconScale,
                    SpriteEffects.None,
                    0f);
            }
        }

        private void DrawSelectedRewardHover(SpriteBatch spriteBatch)
        {
            if (!_isOpening || !_selectedReward.HasValue || _spinElapsed < SpinDurationSeconds)
            {
                return;
            }

            WeaponSpinEntry reward = _selectedReward.Value;
            Color tierColor = GetTierColor(RolledTier?.Name);
            float hoverProgress = Math.Clamp(_rewardHoverElapsed / Math.Max(0.01f, RewardHoverDurationSeconds), 0f, 1f);
            float bob = (float)Math.Sin(_rewardHoverElapsed * 7f) * 6f;
            float alpha = 1f - Math.Max(0f, hoverProgress - 0.8f) / 0.2f;

            Vector2 iconPos = _position + new Vector2(0f, -132f + bob);
            Vector2 iconOrigin = new Vector2(reward.Texture.Width / 2f, reward.Texture.Height / 2f);
            if (_tierCircleTexture != null)
            {
                Vector2 circleOrigin = new Vector2(_tierCircleTexture.Width / 2f, _tierCircleTexture.Height / 2f);

                spriteBatch.Draw(
                    _tierCircleTexture,
                    iconPos,
                    null,
                    tierColor * Math.Clamp(alpha, 0f, 1f),
                    0f,
                    circleOrigin,
                    TierCircleScale,
                    SpriteEffects.None,
                    0f);
            }

            spriteBatch.Draw(
                reward.Texture,
                iconPos,
                null,
                Color.White * Math.Clamp(alpha, 0f, 1f),
                0f,
                iconOrigin,
                reward.RewardScale,
                SpriteEffects.None,
                0f);
        }

        private void TryLoadWeaponSpinTextures(ContentManager content)
        {
            AddSpinEntry(content, typeof(StartingWeapon), "BOOG", 0.45f);
            AddSpinEntry(content, typeof(Lbow), "Lbow", 4.9f);
            AddSpinEntry(content, typeof(Firebow), "Firebow", 4.9f);
            AddSpinEntry(content, typeof(Icebow), "Icebow", 4.9f);
            AddSpinEntry(content, typeof(Earthbow), "Earthbow", 4.9f);
            AddSpinEntry(content, typeof(Poisonbow), "Poisonbow", 4.9f);
        }

        private void AddSpinEntry(ContentManager content, Type weaponType, string textureName, float rewardScale)
        {
            try
            {
                Texture2D texture = content.Load<Texture2D>(textureName);
                float iconScale = rewardScale * 0.6f;
                _weaponSpinEntries.Add(new WeaponSpinEntry(weaponType, texture, rewardScale, iconScale));
            }
            catch
            {
                // Skip missing textures so loot still works even if one asset isn't available.
            }
        }

        private void SelectRewardWeapon()
        {
            _selectedReward = null;

            if (_gameManager.Player == null || _weaponSpinEntries.Count == 0)
            {
                return;
            }

            List<SpellFall.Weapons.Weapons> worldWeapons = _gameManager.GetObjectsOfType<SpellFall.Weapons.Weapons>();
            List<WeaponSpinEntry> candidates = new List<WeaponSpinEntry>();

            foreach (WeaponSpinEntry entry in _weaponSpinEntries)
            {
                if (entry.WeaponType == typeof(StartingWeapon))
                {
                    continue;
                }

                SpellFall.Weapons.Weapons worldWeapon = worldWeapons.Find(w => w.GetType() == entry.WeaponType);
                if (worldWeapon == null)
                {
                    continue;
                }

                if (_gameManager.Player.OwnsWeapon(worldWeapon))
                {
                    continue;
                }

                candidates.Add(entry);
            }

            if (candidates.Count == 0)
            {
                return;
            }

            _selectedReward = candidates[_rng.Next(candidates.Count)];
        }

        private void GrantRewardIfPossible()
        {
            if (!_selectedReward.HasValue || _gameManager.Player == null)
            {
                return;
            }

            List<SpellFall.Weapons.Weapons> worldWeapons = _gameManager.GetObjectsOfType<SpellFall.Weapons.Weapons>();
            SpellFall.Weapons.Weapons rewardWeapon = worldWeapons.Find(w => w.GetType() == _selectedReward.Value.WeaponType);
            if (rewardWeapon == null)
            {
                return;
            }

            if (_gameManager.Player.OwnsWeapon(rewardWeapon))
            {
                return;
            }

            string tierName = RolledTier?.Name ?? "Common";
            rewardWeapon.ApplyLootTier(tierName);
            _gameManager.Player.AddWeaponToInventory(rewardWeapon);
        }

        private static Color GetTierColor(string tierName)
        {
            return tierName switch
            {
                "Rusty" => new Color(122, 88, 60),
                "Common" => new Color(170, 170, 170),
                "Uncommon" => new Color(90, 200, 90),
                "Rare" => new Color(80, 140, 255),
                "Epic" => new Color(175, 95, 255),
                "Legendary" => new Color(255, 180, 60),
                "Mythic" => new Color(255, 90, 120),
                _ => Color.White,
            };
        }

        private static Texture2D CreateCircleTexture(IGraphicsDeviceService graphicsService, int diameter)
        {
            if (graphicsService?.GraphicsDevice == null)
            {
                return null;
            }

            Texture2D texture = new Texture2D(graphicsService.GraphicsDevice, diameter, diameter);
            Color[] pixels = new Color[diameter * diameter];
            float radius = diameter / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    int index = y * diameter + x;
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    pixels[index] = dist <= radius ? Color.White : Color.Transparent;
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private static float GetSpinAlpha(float progress)
        {
            if (progress < 0.2f)
            {
                return progress / 0.2f;
            }

            if (progress > 0.8f)
            {
                return (1f - progress) / 0.2f;
            }

            return 1f;
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