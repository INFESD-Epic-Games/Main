using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpellFall.Engine;

namespace SpellFall.UI;

public class GameOverScreen
{
    private SpriteFont _font;
    private Texture2D _pixel;
    private KeyboardState _previousKeyboardState;

    public event Action RestartRequested;
    public event Action ReturnToMenuRequested;

    public void Load(ContentManager content)
    {
        _font = content.Load<SpriteFont>("BubbleText");
        _pixel = content.Load<Texture2D>("white");
        _previousKeyboardState = Keyboard.GetState();
    }

    public void ResetInputState()
    {
        _previousKeyboardState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();

        bool restartPressed = currentKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter);
        bool menuPressed = currentKeyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M);

        if (restartPressed)
        {
            RestartRequested?.Invoke();
        }
        else if (menuPressed)
        {
            ReturnToMenuRequested?.Invoke();
        }

        _previousKeyboardState = currentKeyboardState;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_font == null || _pixel == null)
        {
            return;
        }

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, RenderManager.VirtualWidth, RenderManager.VirtualHeight),
            new Color(0, 0, 0, 190));

        string title = "GAME OVER";
        float titleScale = 5f;
        Vector2 titleSize = _font.MeasureString(title) * titleScale;
        Vector2 titlePosition = new Vector2(
            (RenderManager.VirtualWidth - titleSize.X) * 0.5f,
            RenderManager.VirtualHeight * 0.3f);

        spriteBatch.DrawString(
            _font,
            title,
            titlePosition,
            new Color(230, 70, 70),
            0f,
            Vector2.Zero,
            titleScale,
            SpriteEffects.None,
            0f);

        string subtitle = "Press ENTER to restart";
        string menuHint = "Press M to return to menu";

        float pulse = 0.65f + 0.35f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3f);
        Color hintColor = Color.White * pulse;

        float hintScale = 2.4f;
        Vector2 subtitleSize = _font.MeasureString(subtitle) * hintScale;
        Vector2 subtitlePosition = new Vector2(
            (RenderManager.VirtualWidth - subtitleSize.X) * 0.5f,
            RenderManager.VirtualHeight * 0.52f);

        Vector2 menuHintSize = _font.MeasureString(menuHint) * hintScale;
        Vector2 menuHintPosition = new Vector2(
            (RenderManager.VirtualWidth - menuHintSize.X) * 0.5f,
            subtitlePosition.Y + subtitleSize.Y + 20f);

        spriteBatch.DrawString(
            _font,
            subtitle,
            subtitlePosition,
            hintColor,
            0f,
            Vector2.Zero,
            hintScale,
            SpriteEffects.None,
            0f);

        spriteBatch.DrawString(
            _font,
            menuHint,
            menuHintPosition,
            hintColor,
            0f,
            Vector2.Zero,
            hintScale,
            SpriteEffects.None,
            0f);

        spriteBatch.End();
    }
}
