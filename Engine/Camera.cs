using Microsoft.Xna.Framework;
using SpellFall.Character;

namespace SpellFall.Engine
{
    public class Camera
    {
        public Matrix Transform { get; private set; }

        public void Follow(Player target)
        {
            var position = Matrix.CreateTranslation(
                -target.GetPosition().X - (target.rectangleCollider.shape.Width / 2),
                -target.GetPosition().Y - (target.rectangleCollider.shape.Height / 2),
                0);

            var offset = Matrix.CreateTranslation(
                RenderManager.VirtualWidth / 2f,
                RenderManager.VirtualHeight / 2f,
                0);

            Transform = position * offset;
        }
    }
}