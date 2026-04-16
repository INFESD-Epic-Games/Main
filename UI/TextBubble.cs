using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.UI
{
    public class TextBubble : GameObject
    {
        private Texture2D _texture;
        private SpriteFont _font;
        private string _text = "Hello, this is a text bubble!";
        private int _paddingX = 20;
        private int _paddingY = 90;
        private int _initialViewportWidth = 640;
        private int _initialViewportHeight = 480;
        private float _scaleX = 1f;
        private float _scaleY = 1f;
        private bool _isVisible = false;
        public Vector2 Position { get; set; }

        public TextBubble()
        {
            Position = new Vector2(_initialViewportWidth / 2f - 160, _initialViewportHeight / 3f * 2f);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("TextBubble");
            _font = content.Load<SpriteFont>("BubbleText");
            base.Load(content);
        }

        public void SetText(string text)
        {
            _text = text;
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        private void UpdateScale()
        {
            _scaleX = (float)RenderManager.VirtualWidth / _initialViewportWidth;
            _scaleY = (float)RenderManager.VirtualHeight / _initialViewportHeight;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateScale();

            if (_isVisible)
            {
                Hide();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!_isVisible)
            {
                return;
            }

            Func<Vector2, Vector2> worldPos = x => Vector2.Transform(x, Matrix.Invert(GameManager.GetGameManager().Camera.Transform));

            Vector2 targetScreenPosition = Position * new Vector2(_scaleX, _scaleY);
            Vector2 scaledPosition = worldPos(targetScreenPosition);
            Vector2 textPosition = scaledPosition + new Vector2(_paddingX * _scaleX, _paddingY * _scaleY);
            
            spriteBatch.Draw(_texture, scaledPosition, null, Color.White, 0f, Vector2.Zero, new Vector2(_scaleX, _scaleY) * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, _text, textPosition, Color.Black, 0f, Vector2.Zero, Math.Min(_scaleX, _scaleY) * 1.25f, SpriteEffects.None, 0f);
        }
    }
}