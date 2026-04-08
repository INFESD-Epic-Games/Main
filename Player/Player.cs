using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Items;
using Microsoft.Xna.Framework.Input;
using System;

namespace SpellFall.Character
{
    public class Player : GameObject
    {
        private const float PlayerScale = 0.5f;
        public RectangleCollider rectangleCollider { get; private set; }
        private GameObject _equippedWeapon;
        Vector2 lastDirection = Vector2.UnitY;
        private Vector2 _thrustInput = Vector2.Zero;
        private const int _dashDistance = 200;
        private float _dashCooldown = 5f; // seconds
        private float _dashTimer = 0f;
        private bool _canDash = true;

        private HealthBar _healthBar;
        public PlayerStats Stats { get; }
        // public int currentHealth = 100;
        Vector2 position;
        // private float luck {get; set;} = 1f;
        // private Loot loot = new Loot();
        // private KeyboardState previousKeyboardState;

        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float animationSpeed = 0.15f;
        private bool isMoving = false;

        private Texture2D walkNorth;
        private Texture2D walkSouth;
        private Texture2D walkEast;
        private Texture2D walkWest;

        private Texture2D currentTexture;

        public Player(Point Position)
        {
            Stats = new PlayerStats();
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);

            _healthBar = new HealthBar(Stats);
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            walkNorth = content.Load<Texture2D>("Walk_north");
            walkSouth = content.Load<Texture2D>("Walk_south");
            walkEast  = content.Load<Texture2D>("Walk_east");
            walkWest  = content.Load<Texture2D>("Walk_west");

            currentTexture = walkSouth;

            int colliderWidth = currentTexture.Width / 4;
            int colliderHeight = currentTexture.Height;
            rectangleCollider.shape.Size = new Point(colliderWidth, colliderHeight);
            rectangleCollider.shape.Location -= new Point(colliderWidth / 2, colliderHeight / 2);
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
                HealthBar.TakeDamage(10);
            
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


            isMoving = _thrustInput != Vector2.Zero;

            if (isMoving)
            {
                Vector2 inputDirection = _thrustInput;
                inputDirection.Normalize();
                lastDirection = inputDirection;

                position += inputDirection * Stats.Speed;

                if (Math.Abs(inputDirection.X) > Math.Abs(inputDirection.Y))
                {
                    if (inputDirection.X > 0)
                    {
                        currentTexture = walkEast;
                    }
                    else
                    {
                        currentTexture = walkWest;
                    }
                }
                else
                {
                    if (inputDirection.Y > 0)
                    {
                        currentTexture = walkSouth;
                    }
                    else
                    {
                        currentTexture = walkNorth;
                    }
                }

                animationTimer += deltaTime;
                if (animationTimer >= animationSpeed)
                {
                    currentFrame++;
                    if (currentFrame >= 4)
                        currentFrame = 0;

                    animationTimer = 0f;
                }
            }
            else
            {
                currentFrame = 0;
            }

            int colliderWidth = currentTexture.Width / 4;
            int colliderHeight = currentTexture.Height;
            rectangleCollider.shape.Location = new Point(
                (int)(position.X - colliderWidth / 2),
                (int)(position.Y - colliderHeight / 2)
            );

            HealthBar.SetPosition(rectangleCollider.shape);
            HealthBar.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int frameWidth = currentTexture.Width / 4; 
            int frameHeight = currentTexture.Height;

            Rectangle sourceRect = new Rectangle(
                currentFrame * frameWidth,
                0,
                frameWidth,
                frameHeight
            );
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            spriteBatch.Draw(
                currentTexture,
                position,
                sourceRect,
                Color.White,
                0f,
                origin,
                PlayerScale,
                SpriteEffects.None,
                0f
            );
            HealthBar.DrawHealthBar(spriteBatch);
            base.Draw(gameTime, spriteBatch);
        }

        public Rectangle GetPosition()
        {
            return rectangleCollider.shape;
        }

        // Gets the box the game uses to draw the player sprite on screen.
        public Rectangle GetVisualBounds() => new Rectangle(
            (int)(position.X - (currentTexture.Width / 8f) * PlayerScale),
            (int)(position.Y - (currentTexture.Height / 2f) * PlayerScale),
            (int)((currentTexture.Width / 4f) * PlayerScale),
            (int)(currentTexture.Height * PlayerScale)
        );

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