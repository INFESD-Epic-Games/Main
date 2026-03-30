using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Items;
using Microsoft.Xna.Framework.Input;

namespace SpellFall.Character
{
    public class Player : GameObject
    {
        public RectangleCollider rectangleCollider { get; private set; }
        private Texture2D _texture;
        private GameObject _equippedWeapon;
        float speed = 5f;
        Vector2 position;
        private float luck {get; set;} = 1f;
        private Loot loot = new Loot();
        private KeyboardState previousKeyboardState;

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

        public override void Update(GameTime gameTime)
        {
            Vector2 inputDirection = Vector2.Zero;

            var keyboardstate = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                inputDirection.Y -= 1;

            if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                inputDirection.Y += 1;

            if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                inputDirection.X -= 1;

            if (keyboardstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                inputDirection.X += 1;

            var current = Keyboard.GetState();

            if (current.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
            {
                var rarity = loot.GetRandomRarity(luck);
            }

            previousKeyboardState = current;

            if (inputDirection != Vector2.Zero)
            {
                inputDirection.Normalize();

                position += inputDirection * speed;
            }

            rectangleCollider.shape.Location = position.ToPoint();

            base.Update(gameTime);
        }
    }
}