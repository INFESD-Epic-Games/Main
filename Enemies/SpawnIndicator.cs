using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;

namespace SpellFall.Enemies
{
    public class SpawnIndicator : GameObject
    {
        private const float SpawnIndicatorScale = 0.5f;

        private readonly Vector2 _position;
        private readonly float _durationSeconds;
        private readonly Action _onFinished;

        private Texture2D _texture;
        private float _elapsedSeconds;

        public SpawnIndicator(Vector2 position, float durationSeconds, Action onFinished)
        {
            _position = position;
            _durationSeconds = durationSeconds;
            _onFinished = onFinished;
            _elapsedSeconds = 0f;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("SpawnIndicator");

            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedSeconds >= _durationSeconds)
            {
                _onFinished?.Invoke();
                GameManager.GetGameManager().RemoveGameObject(this);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                return;
            }

            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _position,
                null,
                Color.White,
                0f,
                origin,
                SpawnIndicatorScale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }
    }
}