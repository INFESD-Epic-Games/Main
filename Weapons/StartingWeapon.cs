using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Collision;
using SpellFall.Engine;
using SpellFall.Weapons.Projectiles;

namespace SpellFall.Weapons
{
	public class StartingWeapon : Weapons
	{
		private const float BowScale = 0.45f;

		private Texture2D _bowTexture;
		private Point _target;
		private Vector2 _bowCenter;

		public StartingWeapon() : base(damageBonus: 5, baseCdFrames: 36, fireRateBonus: 0.2f)
		{
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
			if (!IsEquipped)
			{
				base.HandleInput(inputManager);
				return;
			}

			Vector2 gameMouse = Game1.ScreenToGameCoordinates(inputManager.CurrentMouseState.Position.ToVector2());
			Matrix inverseCamera = Matrix.Invert(_gameManager.Camera.Transform);
			Vector2 worldMouse = Vector2.Transform(gameMouse, inverseCamera);
			_target = worldMouse.ToPoint();

			Rectangle playerRect = _gameManager.Player.GetPosition();
			Vector2 aimDirection = LinePieceCollider.GetDirection(playerRect.Center, _target);
			_bowCenter = GetBowCenter(aimDirection);

			if (inputManager.LeftMousePress() && TryStartPrimaryAttackCooldown())
			{
				int totalDamage = _gameManager.Player.Stats.TotalDamage;
				_gameManager.AddGameObject(new Arrow(_bowCenter, aimDirection, 320f, totalDamage));
			}

			base.HandleInput(inputManager);
		}

		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (!IsEquipped)
			{
				base.Draw(gameTime, spriteBatch);
				return;
			}

			Rectangle playerRect = _gameManager.Player.GetPosition();
			Vector2 aimDirection = LinePieceCollider.GetDirection(playerRect.Center, _target);
			float aimAngle = LinePieceCollider.GetAngle(aimDirection);
			_bowCenter = GetBowCenter(aimDirection);

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

		private Vector2 GetBowCenter(Vector2 aimDirection)
		{
			Rectangle playerRect = _gameManager.Player.GetVisualBounds();
			float bowOffsetDistance = playerRect.Width / 2f;
			return playerRect.Center.ToVector2() + aimDirection * bowOffsetDistance;
		}
	}
}
