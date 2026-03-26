using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpellFall.Character
{
    public class Player : GameObject
    {
        public RectangleCollider rectangleCollider { get; private set; }
        private Texture2D _texture;
        private GameObject _equippedWeapon;

        float speed = 5f;
        Vector2 lastDirection = Vector2.UnitY;
        private Vector2 _thrustInput = Vector2.Zero;
        private Vector2 velocity = Vector2.Zero;

        Vector2 position;

        public Player(Point Position)
        {
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);
        }

        // Placeholder player. Remove when updating
        public override void Load(ContentManager content)
        {
            base.Load(content);
            _texture = content.Load<Texture2D>("ship_body");
           rectangleCollider.shape.Size = _texture.Bounds.Size;
           rectangleCollider.shape.Location -= new Point(_texture.Bounds.Width / 2, _texture.Bounds.Height / 2);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, rectangleCollider.GetBoundingBox(), Color.White);
            base.Draw(gameTime, spriteBatch);
        }

        public Rectangle GetPosition()
        {
            return rectangleCollider.shape;
        }
        public void EquipWeapon(GameObject weapon)
        {
            _equippedWeapon = weapon;
        }

        public override void HandleInput(InputManager inputManager)
        {
            _thrustInput = Vector2.Zero;

            if (inputManager.IsKeyDown(Keys.W))
                _thrustInput.Y -= 1;
            if (inputManager.IsKeyDown(Keys.S))
                _thrustInput.Y += 1;
            if (inputManager.IsKeyDown(Keys.A))
                _thrustInput.X -= 1;
            if (inputManager.IsKeyDown(Keys.D))
                _thrustInput.X += 1;

            base.HandleInput(inputManager);
        }


        public override void Update(GameTime gameTime)
        {
            if (_thrustInput != Vector2.Zero)
            {
                _thrustInput.Normalize();
                lastDirection = _thrustInput;

                position += _thrustInput * speed;
            }
            rectangleCollider.shape.Location = position.ToPoint();

            base.Update(gameTime);
        }
    }
}