using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;

namespace SpellFall.Weapons.Projectiles
{
    public class Arrow : GameObject
    {
        private const float ArrowScale = 0.4f;
        private const float ArrowHitboxRatio = 0.2f;
        private readonly GameManager _gameManager;

        private Texture2D _texture;
        private readonly CircleCollider _circleCollider;
        private readonly Vector2 _velocity;
        private readonly float _rotation;
        private readonly float _maxLifetime;
        private float _lifetime;
        public int Damage { get; }

        public Arrow(Vector2 location, Vector2 direction, float speed, int damage, float maxLifetime = 10f)
        {
            _gameManager = GameManager.GetGameManager();
            _circleCollider = new CircleCollider(location, 8f);
            SetCollider(_circleCollider);
            _velocity = direction * speed;
            _rotation = LinePieceCollider.GetAngle(direction);
            Damage = Math.Max(0, damage);
            _maxLifetime = maxLifetime;
            _lifetime = 0f;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("PIJL");
            _circleCollider.Radius = _texture.Width * ArrowScale * ArrowHitboxRatio;
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            _circleCollider.Center += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifetime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_lifetime > _maxLifetime)
            {
                _gameManager.RemoveGameObject(this);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 arrowOrigin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(
                _texture,
                _circleCollider.Center,
                null,
                Color.White,
                _rotation,
                arrowOrigin,
                ArrowScale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }
    }
}
