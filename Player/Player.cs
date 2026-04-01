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
        public RectangleCollider rectangleCollider { get; private set; }
        private Texture2D _texture;
        private GameObject _equippedWeapon;
        float speed = 5f;
        Vector2 position;
        private float luck {get; set;} = 1f;
        private Loot loot = new Loot();
        private KeyboardState previousKeyboardState;

        enum Direction
        {
            Down,
            Up,
            Left,
            Right
        }

        private Direction currentDirection = Direction.Down;
        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float animationSpeed = 0.15f;
        private bool isMoving = false;
        private int frameWidth = 64;
        private int frameHeight = 64;

        private Texture2D walkNorth;
        private Texture2D walkSouth;
        private Texture2D walkEast;
        private Texture2D walkWest;

        private Texture2D currentTexture;

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
            walkNorth = content.Load<Texture2D>("Walk_north");
            walkSouth = content.Load<Texture2D>("Walk_south");
            walkEast  = content.Load<Texture2D>("Walk_east");
            walkWest  = content.Load<Texture2D>("Walk_west");

            currentTexture = walkSouth;
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
                0.5f,
                SpriteEffects.None,
                0f
            );
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

            // var current = Keyboard.GetState();

            // if (current.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
            // {
            //     var rarity = loot.GetRandomRarity(luck);
            // }

            // previousKeyboardState = current;

            isMoving = inputDirection != Vector2.Zero;

            if (isMoving)
            {
                inputDirection.Normalize();
                position += inputDirection * speed;
                if (Math.Abs(inputDirection.X) > Math.Abs(inputDirection.Y))
                {
                    if (inputDirection.X > 0)
                    {
                        currentDirection = Direction.Right;
                        currentTexture = walkEast;
                    }
                    else
                    {
                        currentDirection = Direction.Left;
                        currentTexture = walkWest;
                    }
                }
                else
                {
                    if (inputDirection.Y > 0)
                    {
                        currentDirection = Direction.Down;
                        currentTexture = walkSouth;
                    }
                    else
                    {
                        currentDirection = Direction.Up;
                        currentTexture = walkNorth;
                    }
                }
            }

            animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isMoving)
            {
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
            rectangleCollider.shape.Location = position.ToPoint();

            base.Update(gameTime);
        }
    }
}