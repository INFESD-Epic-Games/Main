using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Character
{
    public class Player : GameObject
    {
        public RectangleCollider rectangleCollider { get; private set; }
        private Texture2D _texture;

        public Player(Point Position)
        {
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
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
    }
}