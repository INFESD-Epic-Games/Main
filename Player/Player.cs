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
        private const int _dashDistance = 200;
        private float _dashCooldown = 5f; // seconds
        private float _dashTimer = 0f;
        private bool _canDash = true;

        private HealthBar _healthBar;
        private const int _maxHealth = 100;
        public int currentHealth = 100;

        Vector2 position;

        public Player(Point Position)
        {
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);

            _healthBar = new HealthBar(_maxHealth);
        }

        // Placeholder player. Remove when updating
        public override void Load(ContentManager content)
        {
            base.Load(content);
            _texture = content.Load<Texture2D>("ship_body");
           rectangleCollider.shape.Size = _texture.Bounds.Size;
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
            
            // Temporary damage input for testing health bar
            // TODO: Remove when implementing actual damage sources
            if (inputManager.IsKeyPress(Keys.Down))
                _healthBar.TakeDamage(10);
            
            if(inputManager.IsKeyPress(Keys.Space))
                Dash();

            base.HandleInput(inputManager);
        }


        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Update cooldown timer
            if (!_canDash)
            {
                _dashTimer -= deltaTime;
                if (_dashTimer <= 0f)
                {
                    _canDash = true;
                }
            }


            if (_thrustInput != Vector2.Zero)
            {
                _thrustInput.Normalize();
                lastDirection = _thrustInput;

                position += _thrustInput * speed;
            }
            rectangleCollider.shape.Location = new Point(
                (int)(position.X - _texture.Width / 2),
                (int)(position.Y - _texture.Height / 2)
            );

            _healthBar.SetPosition(new Vector2(rectangleCollider.shape.X, rectangleCollider.shape.Y - 30));
            _healthBar.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _healthBar.DrawHealthBar(spriteBatch);
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

        private void Dash()
        {
            if (!_canDash)
                return;

            Vector2 dashDirection = _thrustInput != Vector2.Zero 
                ? Vector2.Normalize(_thrustInput) 
                : lastDirection;

            position += dashDirection * _dashDistance;

            // Start cooldown
            _canDash = false;
            _dashTimer = _dashCooldown;
        }
    }
}