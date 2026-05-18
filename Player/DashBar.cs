using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Character
{
    public class DashBar : GameObject
    {
        private readonly Player _player;
        private Rectangle _ownerBounds;
        private Texture2D _dashBarTexture;


        public DashBar(Player player)
        {
            _player = player;
        }

        public void SetPosition(Rectangle ownerBounds)
        {
            _ownerBounds = ownerBounds;
        }

        public void DrawDashBar(SpriteBatch spriteBatch)
        {
            if (_dashBarTexture == null)
            {
                _dashBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _dashBarTexture.SetData(new[] { Color.White });
            }

            int barWidth = 200;
            int barHeight = 8;

            Vector2 barPosition = new Vector2(
                _ownerBounds.Center.X - (barWidth / 2f),
                _ownerBounds.Y - barHeight
            );

            // Background
            spriteBatch.Draw(_dashBarTexture,
                new Rectangle((int)barPosition.X, (int)barPosition.Y, barWidth, barHeight),
                Color.DodgerBlue);

            // Foreground
            spriteBatch.Draw(_dashBarTexture,
                new Rectangle((int)barPosition.X, (int)barPosition.Y, (int)(barWidth * _player.DashCooldownPercentage), barHeight),
                Color.Gray);
        }

    }
}