using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;

namespace SpellFall.Weapons
{
	public class StartingWeapon : GameObject
	{
		private const float BowScale = 0.45f;
		private readonly GameManager _gameManager;

		private Texture2D _bowTexture;
		private Point _target;
		private Vector2 _bowCenter;

		private readonly float _shotCooldownDuration = 0.2f;
		private float _shotCooldownTimer;

		public StartingWeapon()
		{
			_gameManager = GameManager.GetGameManager();
			_target = Point.Zero;
			_bowCenter = Vector2.Zero;
		}

		public override void Load(ContentManager content)
		{
			_bowTexture = content.Load<Texture2D>("BOOG");
			base.Load(content);
		}

		public override void HandleInput(InputManager inputManager)
		{
			Matrix inverseCamera = Matrix.Invert(_gameManager.Camera.Transform);
			Vector2 worldMouse = Vector2.Transform(inputManager.CurrentMouseState.Position.ToVector2(), inverseCamera);
			_target = worldMouse.ToPoint();
			_bowCenter = GetBowCenter();

			if (inputManager.LeftMousePress() && _shotCooldownTimer <= 0f)
			{
				Rectangle playerRect = _gameManager.Player.GetPosition();
				Vector2 aimDirection = LinePieceCollider.GetDirection(playerRect.Center, _target);
				_gameManager.AddGameObject(new Arrow(_bowCenter, aimDirection, 100f));
				_shotCooldownTimer = _shotCooldownDuration;
			}

			base.HandleInput(inputManager);
		}

		public override void Update(GameTime gameTime)
		{
			if (_shotCooldownTimer > 0f)
			{
				_shotCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Rectangle playerRect = _gameManager.Player.GetPosition();
			Vector2 aimDirection = LinePieceCollider.GetDirection(playerRect.Center, _target);
			float aimAngle = LinePieceCollider.GetAngle(aimDirection);
			_bowCenter = GetBowCenter();

			Vector2 bowOrigin = new Vector2(_bowTexture.Width / 2f, _bowTexture.Height / 2f);

			spriteBatch.Draw(
				_bowTexture,
				_bowCenter,
				null,
				Color.White,
				aimAngle,
				bowOrigin,
				BowScale,
				SpriteEffects.None,
				0f);

			base.Draw(gameTime, spriteBatch);
		}

		private Vector2 GetBowCenter()
		{
			Rectangle playerRect = _gameManager.Player.GetPosition();
			Vector2 aimDirection = LinePieceCollider.GetDirection(playerRect.Center, _target);
			float bowOffsetDistance = (playerRect.Width / 2f) + (_bowTexture.Height * BowScale * 0.15f);
			return playerRect.Center.ToVector2() + aimDirection * bowOffsetDistance;
		}
	}
}
