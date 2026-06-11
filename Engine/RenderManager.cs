using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Engine
{
    public class RenderManager
    {
        public const int VirtualWidth = 1920;
        public const int VirtualHeight = 1080;

        private RenderTarget2D _gameRenderTarget;
        private Rectangle _gameDestinationRect;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _gameRenderTarget = new RenderTarget2D(graphicsDevice, VirtualWidth, VirtualHeight);
            UpdateDestinationRect(graphicsDevice);
        }

        public void UpdateDestinationRect(GraphicsDevice graphicsDevice)
        {
            int backBufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            float scale = System.Math.Min(backBufferWidth / (float)VirtualWidth, backBufferHeight / (float)VirtualHeight);

            int drawWidth = (int)(VirtualWidth * scale);
            int drawHeight = (int)(VirtualHeight * scale);
            int drawX = (backBufferWidth - drawWidth) / 2;
            int drawY = (backBufferHeight - drawHeight) / 2;

            _gameDestinationRect = new Rectangle(drawX, drawY, drawWidth, drawHeight);
        }

        public void BeginWorld(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetRenderTarget(_gameRenderTarget);
            graphicsDevice.Clear(Color.Black);
        }

        public void PresentWorld(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_gameRenderTarget, _gameDestinationRect, Color.White);
            spriteBatch.End();
        }

        public Vector2 ScreenToGameCoordinates(Vector2 screenPosition)
        {
            if (_gameDestinationRect.Width <= 0 || _gameDestinationRect.Height <= 0)
            {
                return screenPosition;
            }

            float gameX = (screenPosition.X - _gameDestinationRect.X) * (VirtualWidth / (float)_gameDestinationRect.Width);
            float gameY = (screenPosition.Y - _gameDestinationRect.Y) * (VirtualHeight / (float)_gameDestinationRect.Height);
            return new Vector2(gameX, gameY);
        }
    }
}
