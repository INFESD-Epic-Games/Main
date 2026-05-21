using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpellFall.Sounds;
using System;
using System.Collections.Generic;
using WeaponBase = SpellFall.Weapons.Weapons;
using Microsoft.Xna.Framework.Audio;
using SpellFall.Enemies;
using SpellFall.Background;

namespace SpellFall.Character
{
    public class Player : GameObject
    {
        private const float PlayerScale = 0.5f;
        private const float ColliderWidthScale = 0.25f;
        private const float ColliderHeightScale = 0.45f;
        public RectangleCollider rectangleCollider { get; private set; }
        private WeaponBase _equippedWeapon;
        Vector2 lastDirection = Vector2.UnitY;
        private Vector2 _thrustInput = Vector2.Zero;
        private Vector2 _previousPosition;
        private const int _dashDistance = 200;
        private const float _dashDuration = 0.3f; // Duration in seconds
        private float _dashCooldown = 5f;
        private float _dashTimer = 0f;
        private bool _canDash = true;
        private bool _isDashing = false;
        private float _dashElapsedTime = 0f;
        private Vector2 _dashStartPosition;
        private Vector2 _dashEndPosition;
        private HealthBar _healthBar;
        public HealthBar HealthBar => _healthBar;
        private DashBar _dashBar;
        public PlayerStats Stats { get; }
        Vector2 position;
        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float animationSpeed = 0.15f;
        private bool isMoving = false;
        private Texture2D walkNorth;
        private Texture2D walkSouth;
        private Texture2D walkEast;
        private Texture2D walkWest;
        private Texture2D currentTexture;
        private SoundEffect _dashSfx;
        protected Map _map;
        protected readonly GameManager _gameManager;

        public float DashCooldownPercentage => _dashTimer / _dashCooldown;

        public Player(Point Position)
        {
            _gameManager = GameManager.GetGameManager();
            _map = _gameManager.Map;
            Stats = new PlayerStats();
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);
            _healthBar = new HealthBar(Stats);
            _dashBar = new DashBar(this);
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            walkNorth = content.Load<Texture2D>("Walk_north");
            walkSouth = content.Load<Texture2D>("Walk_south");
            walkEast = content.Load<Texture2D>("Walk_east");
            walkWest = content.Load<Texture2D>("Walk_west");
            _dashSfx = content.Load<SoundEffect>("Dash");

            currentTexture = walkSouth;
            int frameWidth = walkSouth.Width / 4;
            int frameHeight = walkSouth.Height;

            int colliderWidth = (int)(frameWidth * PlayerScale * ColliderWidthScale);
            int colliderHeight = (int)(frameHeight * PlayerScale * ColliderHeightScale);

            rectangleCollider.shape = new Rectangle(0, 0, colliderWidth, colliderHeight);
            UpdateCollider();
        }

        public override void HandleInput(InputManager inputManager)
        {
            _previousPosition = position;
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

            if (inputManager.IsKeyPress(Keys.Space))
                Dash();

            base.HandleInput(inputManager);
        }


        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Stats.DecreaseAttackCooldown();
            
            // Update cooldown timer
            if (!_canDash)
            {
                _dashTimer -= deltaTime;
                if (_dashTimer <= 0f)
                {
                    _canDash = true;
                }
            }

            // Handle dash animation
            if (_isDashing)
            {
                _dashElapsedTime += deltaTime;
                float progress = _dashElapsedTime / _dashDuration;

                if (progress >= 1f)
                {
                    // Dash complete
                    position = _dashEndPosition;
                    _isDashing = false;
                    progress = 1f;
                }
                else
                {
                    float easedProgress = 1f - (float)Math.Pow(1f - progress, 3f);
                    position = Vector2.Lerp(_dashStartPosition, _dashEndPosition, easedProgress);
                }
            }

            isMoving = _thrustInput != Vector2.Zero && !_isDashing;

            if (isMoving)
            {
                Vector2 inputDirection = _thrustInput;
                inputDirection.Normalize();
                lastDirection = inputDirection;

                Vector2 velocity = inputDirection * Stats.TotalSpeed;
                TryMove(velocity);

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

            UpdateCollider();

            _healthBar.SetPosition(GetVisualBounds());
            _healthBar.Update(gameTime);

            _dashBar.SetPosition(GetVisualBounds());
            
            CheckFieldOfView();
            
            base.Update(gameTime);
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Enemies.AlienSpawner)
            {
                _healthBar.SetPosition(GetVisualBounds());
            }

            base.OnCollision(other);
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
            _healthBar.DrawHealthBar(spriteBatch);
            _dashBar.DrawDashBar(spriteBatch);
            base.Draw(gameTime, spriteBatch);
        }

        public Rectangle GetPosition()
        {
            return rectangleCollider.shape;
        }

        // Gets the box the game uses to draw the player sprite on screen.
        public Rectangle GetVisualBounds() => GetSpriteBounds();

        public void EquipWeapon(WeaponBase weapon)
        {
            _equippedWeapon?.OnUnequip();
            _equippedWeapon = weapon;
            _equippedWeapon?.OnEquip(Stats);
        }

        private void Dash()
        {
            if (!_canDash)
                return;

            _dashSfx.Play();
            Vector2 dashDirection = _thrustInput != Vector2.Zero
                ? Vector2.Normalize(_thrustInput)
                : lastDirection;

            Vector2 dashStep = dashDirection * (_dashDistance / 10f);

            for (int i = 0; i < 10; i++)
            {
                TryMove(dashStep);
            }
          
            // Setup dash animation
            _dashStartPosition = position;
            _dashEndPosition = position + (dashDirection * _dashDistance);
            _isDashing = true;
            _dashElapsedTime = 0f;

            // Start cooldown
            _canDash = false;
            _dashTimer = _dashCooldown;
        }

        private Rectangle GetSpriteBounds()
        {
            int frameWidth = currentTexture.Width / 4;
            int frameHeight = currentTexture.Height;
            int scaledWidth = (int)(frameWidth * PlayerScale);
            int scaledHeight = (int)(frameHeight * PlayerScale);

            return new Rectangle(
                (int)(position.X - scaledWidth / 2f),
                (int)(position.Y - scaledHeight / 2f),
                scaledWidth,
                scaledHeight);
        }

        private void UpdateCollider()
        {
            int colliderWidth = (int)(rectangleCollider.shape.Width == 0
                ? (currentTexture.Width / 4) * PlayerScale * ColliderWidthScale
                : rectangleCollider.shape.Width);

            int colliderHeight = (int)(rectangleCollider.shape.Height == 0
                ? currentTexture.Height * PlayerScale * ColliderHeightScale
                : rectangleCollider.shape.Height);

            Point colliderLocation = (position - new Vector2(colliderWidth / 2f, colliderHeight / 2f)).ToPoint();

            rectangleCollider.shape = new Rectangle(
                colliderLocation,
                new Point(colliderWidth, colliderHeight)
            );
        }

        private void TryMove(Vector2 velocity)
        {
            var _map = _gameManager.map();
            if (_map == null)
            {
                position += velocity;
                return;
            }

            int width = rectangleCollider.shape.Width;
            int height = rectangleCollider.shape.Height;

        
            Vector2 newPosX = new Vector2(position.X + velocity.X, position.Y);
            if (!_map.IsColliding(newPosX - new Vector2(width / 2f, height / 2f), width, height))
            {
                position.X += velocity.X;
            }

            Vector2 newPosY = new Vector2(position.X, position.Y + velocity.Y);
            if (!_map.IsColliding(newPosY - new Vector2(width / 2f, height / 2f), width, height))
            {
                position.Y += velocity.Y;
            }
           
        }
        public void SetMap(Map map)
        {
            _map = map;
        }

        private void CheckFieldOfView()
        {
            List<IWatchable> watchables = GameManager.GetGameManager().GetObjectsOfType<IWatchable>();
    
            float halfAngleCos = (float)Math.Cos(MathHelper.ToRadians(90f) / 2f);
            float maxRadiusSquared = 750f * 750f;

            Vector2 forwardDir = lastDirection;
            if (forwardDir != Vector2.Zero) forwardDir.Normalize();

            foreach (IWatchable watchable in watchables)
            {
                if (watchable is not Enemy enemy)
                {
                    watchable.IsWatched = false;
                    continue;
                }

                Vector2 toTarget = enemy.GetPosition() - position;
                float distanceSquared = toTarget.LengthSquared();

                if (distanceSquared > maxRadiusSquared)
                {
                    watchable.IsWatched = false;
                    continue;
                }
        
                if (distanceSquared == 0)
                {
                    watchable.IsWatched = true;
                    continue;
                }

                Vector2 toTargetNormalized = toTarget / (float)Math.Sqrt(distanceSquared);
    
                float dotProduct = Vector2.Dot(forwardDir, toTargetNormalized);

                if (dotProduct >= halfAngleCos)
                {
                    watchable.IsWatched = true;
                    continue;
                }
        
                watchable.IsWatched = false;
            }
        }
    }
}