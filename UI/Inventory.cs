using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpellFall.Character;
using SpellFall.Engine;
using SpellFall.Weapons;
using System;
using System.Collections.Generic;
using WeaponBase = SpellFall.Weapons.Weapons;

namespace SpellFall.UI
{
    public class Inventory
    {
        private const int SlotCount = 6;
        private const int SlotSize = 96;
        private const int SlotSpacing = 10;
        private const int InventoryPadding = 20;

        private readonly Player _player;
        private Texture2D _slotTexture;
        private Texture2D _whitePixel;
        private Dictionary<string, Texture2D> _weaponTextures;
        private MouseState _previousMouseState;
        private int _hoveredSlot = -1;
        private SpriteFont _tooltipFont;
        private Vector2 _position;
        private GraphicsDevice GraphicsDevice { get; set; }

        public Inventory(Player player)
        {
            _player = player;
            _weaponTextures = new Dictionary<string, Texture2D>();
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            _slotTexture = content.Load<Texture2D>("inventory slot");

            _tooltipFont = content.Load<SpriteFont>("IntroText");
            LoadWeaponTextures(content);
        }

        private void LoadWeaponTextures(ContentManager content)
        {
            string[] weaponAssets = { "BOOG", "Lbow", "Firebow", "Icebow", "Earthbow", "Poisonbow" };
            foreach (string asset in weaponAssets)
            {
                try
                {
                    _weaponTextures[asset] = content.Load<Texture2D>(asset);
                }
                catch { }
            }
        }

        public void Update(MouseState currentMouseState, int screenWidth, int screenHeight)
        {
            _hoveredSlot = -1;

            if (_slotTexture == null || _player == null)
            {
                return;
            }

            int totalWidth = (SlotCount * SlotSize) + ((SlotCount - 1) * SlotSpacing);
            _position = new Vector2((screenWidth - totalWidth) / 2f, screenHeight - SlotSize - InventoryPadding);

            Vector2 mousePos = currentMouseState.Position.ToVector2();

            for (int i = 0; i < SlotCount; i++)
            {
                Vector2 slotPos = _position + new Vector2(i * (SlotSize + SlotSpacing), 0);
                Rectangle slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, SlotSize, SlotSize);

                if (slotRect.Contains((int)mousePos.X, (int)mousePos.Y))
                {
                    _hoveredSlot = i;
                }
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_slotTexture == null || _player == null)
            {
                return;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                Vector2 slotPos = _position + new Vector2(i * (SlotSize + SlotSpacing), 0);
                WeaponBase weapon = _player.GetOwnedWeaponAtSlot(i + 1);

                DrawSlot(spriteBatch, slotPos, weapon, i == _hoveredSlot);
            }

            if (_hoveredSlot >= 0)
            {
                DrawTooltip(spriteBatch, _hoveredSlot);
            }
        }

        private void DrawSlot(SpriteBatch spriteBatch, Vector2 position, WeaponBase weapon, bool isHovered)
        {
            Color slotColor = Color.White;
            if (isHovered)
            {
                slotColor = Color.Yellow;
            }

            spriteBatch.Draw(_slotTexture, position, slotColor);

            if (weapon != null)
            {
                Color rarityColor = GetRarityColor(weapon.LootTierName);
                Texture2D weaponTexture = GetWeaponTexture(weapon);

                if (weaponTexture != null)
                {
                    Vector2 weaponCenter = position + new Vector2(SlotSize / 2f, SlotSize / 2f);
                    Vector2 weaponOrigin = new Vector2(weaponTexture.Width / 2f, weaponTexture.Height / 2f);
                    float weaponScale = Math.Min(SlotSize * 0.8f / weaponTexture.Width, SlotSize * 0.8f / weaponTexture.Height);

                    spriteBatch.Draw(
                        weaponTexture,
                        weaponCenter,
                        null,
                        rarityColor,
                        0f,
                        weaponOrigin,
                        weaponScale,
                        SpriteEffects.None,
                        0f
                    );
                }

                bool isEquipped = weapon == _player.EquippedWeapon;
                if (isEquipped)
                {
                    DrawEquippedIndicator(spriteBatch, position);
                }
            }
        }

        private void DrawEquippedIndicator(SpriteBatch spriteBatch, Vector2 position)
        {
            Vector2 indicatorPos = position + new Vector2(SlotSize - 15, 5);
            spriteBatch.DrawString(_tooltipFont, "E", indicatorPos, Color.Gold);
        }

        private void DrawTooltip(SpriteBatch spriteBatch, int slotIndex)
        {
            WeaponBase weapon = _player.GetOwnedWeaponAtSlot(slotIndex + 1);
            if (weapon == null)
            {
                return;
            }

            string tooltipText = $"{weapon.LootTierName} - {weapon.GetType().Name}";
            Vector2 mousePos = Mouse.GetState().Position.ToVector2();
            Vector2 tooltipPos = mousePos + new Vector2(15, 15);

            Vector2 textSize = _tooltipFont.MeasureString(tooltipText);
            Color rarityColor = GetRarityColor(weapon.LootTierName);

            float bgWidth = textSize.X + 20;
            float bgHeight = textSize.Y + 10;

            Texture2D whitePixel = CreateWhitePixel();
            spriteBatch.Draw(whitePixel, new Rectangle((int)tooltipPos.X - 5, (int)tooltipPos.Y - 5, (int)bgWidth + 10, (int)bgHeight + 10), Color.Black * 0.8f);
            spriteBatch.DrawString(_tooltipFont, tooltipText, tooltipPos, rarityColor);
        }

        private Texture2D CreateWhitePixel()
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }

        private static Color GetRarityColor(string tierName)
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

        private Texture2D GetWeaponTexture(WeaponBase weapon)
        {
            string weaponTypeName = weapon.GetType().Name;
            string textureKey = weaponTypeName switch
            {
                "StartingWeapon" => "BOOG",
                "Lbow" => "Lbow",
                "Firebow" => "Firebow",
                "Icebow" => "Icebow",
                "Earthbow" => "Earthbow",
                "Poisonbow" => "Poisonbow",
                _ => null
            };

            if (textureKey != null && _weaponTextures.TryGetValue(textureKey, out var texture))
            {
                return texture;
            }

            return null;
        }
    }
}
