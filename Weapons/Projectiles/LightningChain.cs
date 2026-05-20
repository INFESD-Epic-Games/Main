using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Engine;
using SpellFall.Enemies;

namespace SpellFall.Weapons.Projectiles
{
    public class LightningChain : GameObject
    {
        private const float LightningThickness = 1.2f;

        private readonly Enemy _source;
        private readonly Enemy _target;
        private readonly float _durationSeconds;
        private Texture2D _texture;
        private Rectangle _sourceRectangle;
        private float _elapsedSeconds;

        public LightningChain(Enemy source, Enemy target, float durationSeconds)
        {
            _source = source;
            _target = target;
            _durationSeconds = durationSeconds;
            _elapsedSeconds = 0f;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("lightning");
            int halfHeight = _texture.Height / 2;
            _sourceRectangle = new Rectangle(0, 0, _texture.Width, halfHeight);

            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedSeconds >= _durationSeconds)
            {
                GameManager.GetGameManager().RemoveGameObject(this);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture == null || _target == null || _source == null)
            {
                return;
            }

            Vector2 sourcePos = _source.GetPosition();
            Vector2 targetPos = _target.GetPosition();

            Vector2 delta = targetPos - sourcePos;
            float length = delta.Length();
            if (length <= 0.1f) return;

            float rotation = (float)System.Math.Atan2(delta.Y, delta.X);
            Vector2 origin = new Vector2(0f, _sourceRectangle.Height / 2f);

            float scaleX = length / _sourceRectangle.Width;
            float scaleY = LightningThickness;

            spriteBatch.Draw(
                _texture,
                sourcePos,
                _sourceRectangle,
                Color.White,
                rotation,
                origin,
                new Vector2(scaleX, scaleY),
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }
    }
}
