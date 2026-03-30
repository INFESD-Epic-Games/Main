using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;

namespace SpellFall.Enemies
{
    public class Alien : GameObject
    {
        private const float MoveSpeed = 40f;
        private const float AlienScale = 1.2f;
        private const float HitboxScale = 0.6f;

        private readonly GameManager _gameManager;
        private readonly RectangleCollider _rectangleCollider;

        private Texture2D _texture;
        private Vector2 _position;

        public Alien(Point startPosition)
        {
            _gameManager = GameManager.GetGameManager();
            _position = startPosition.ToVector2();
            _rectangleCollider = new RectangleCollider(new Rectangle(startPosition, Point.Zero));
            SetCollider(_rectangleCollider);
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("alien");

            UpdateCollider();
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 playerPosition = _gameManager.Player.GetPosition().Center.ToVector2();
            Vector2 directionToPlayer = playerPosition - _position;

            if (directionToPlayer != Vector2.Zero)
            {
                directionToPlayer.Normalize();
                _position += directionToPlayer * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            UpdateCollider();
            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            // dit moet nog veranderd worden, wanneer er meer wapens en projectiles zijn.
            if (other is Arrow)
            {
                _gameManager.RemoveGameObject(this);
            }

            base.OnCollision(other);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int frameIndex = GetFrameIndex(_gameManager.Player.GetPosition().Center.ToVector2());
            int frameWidth = _texture.Width / 4;
            int frameHeight = _texture.Height;
            Rectangle sourceRectangle = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            spriteBatch.Draw(
                _texture,
                _position,
                sourceRectangle,
                Color.White,
                0f,
                origin,
                AlienScale,
                SpriteEffects.None,
                0f);

            base.Draw(gameTime, spriteBatch);
        }

        private int GetFrameIndex(Vector2 playerPosition)
        {
            bool isRightOfPlayer = _position.X >= playerPosition.X;
            bool isAbovePlayer = _position.Y < playerPosition.Y;

            if (isRightOfPlayer && isAbovePlayer)
            {
                return 0;
            }

            if (!isRightOfPlayer && isAbovePlayer)
            {
                return 1;
            }

            if (!isRightOfPlayer && !isAbovePlayer)
            {
                return 2;
            }

            return 3;
        }

        private void UpdateCollider()
        {
            int frameWidth = _texture == null ? 0 : _texture.Width / 4;
            int frameHeight = _texture == null ? 0 : _texture.Height;

            int colliderWidth = (int)(frameWidth * AlienScale * HitboxScale);
            int colliderHeight = (int)(frameHeight * AlienScale * HitboxScale);
            Point colliderLocation = (_position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();

            _rectangleCollider.shape = new Rectangle(colliderLocation, new Point(colliderWidth, colliderHeight));
        }
    }
}
