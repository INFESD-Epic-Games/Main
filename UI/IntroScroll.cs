using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpellFall.Engine;

namespace SpellFall.UI;

public class IntroScroll
{
    private enum StoryRevealMode
    {
        PanelByPanel,
        LeftToRight
    }

    private enum IntroPhase
    {
        Scrolling,
        ImageRevealing
    }

    private readonly string[] _lines =
    {
        "The world is wrong.",
        "It wasn't destroyed.",
        "It was rewritten.",
        "The world doesn't follow its rules anymore.",
        "Neither do its people.",
        "You wake up after years of nothing.",
        "",
        "",
        "Your journey begins now."
    };

    private readonly Vector2[] _stars;
    private readonly float[] _starSpeeds;

    private SpriteFont _font;
    private Texture2D _pixel;
    private Texture2D _storyTexture;
    private KeyboardState _previousKeyboardState;

    private float _scrollY;
    private float _scrollSpeed = 200f;
    private float _storyRevealTimer;
    private IntroPhase _phase = IntroPhase.Scrolling;
    private const float StoryTextScale = 5f;
    private const float StoryLineSpacingOffset = 10f;
    private const float HintTextScale = 2.6f;
    private readonly StoryRevealMode _revealMode = StoryRevealMode.PanelByPanel;
    private const int StoryPanelCount = 3;
    private const float PanelRevealSeconds = 1.8f;
    private const float SmoothRevealSeconds = 4.8f;
    private const float RevealHoldSeconds = 1.5f;
    private const float ScrollSpeedReferenceHeight = 1080f;

    public bool IsFinished { get; private set; }

    public IntroScroll()
    {
        const int starCount = 140;
        _stars = new Vector2[starCount];
        _starSpeeds = new float[starCount];

        Random random = new Random(1337);
        for (int i = 0; i < starCount; i++)
        {
            _stars[i] = new Vector2(
                random.Next(0, RenderManager.VirtualWidth),
                random.Next(0, RenderManager.VirtualHeight));

            _starSpeeds[i] = 14f + (float)random.NextDouble() * 26f;
        }
    }

    public void Load(ContentManager content)
    {
        _font = content.Load<SpriteFont>("BubbleText");
        _pixel = content.Load<Texture2D>("white");
        _storyTexture = content.Load<Texture2D>("Story");
    }

    public void Reset()
    {
        IsFinished = false;
        _scrollY = RenderManager.VirtualHeight + 100f;
        _storyRevealTimer = 0f;
        _phase = IntroPhase.Scrolling;
        _previousKeyboardState = Keyboard.GetState();
    }

    public void Update(GameTime gameTime)
    {
        if (IsFinished || _font == null)
        {
            return;
        }

        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_phase == IntroPhase.Scrolling)
        {
            float heightScale = RenderManager.VirtualHeight / ScrollSpeedReferenceHeight;
            _scrollY -= _scrollSpeed * heightScale * deltaSeconds;

            float lineStep = GetStoryLineStep();
            float totalTextHeight = _lines.Length * lineStep;
            if (_scrollY + totalTextHeight < 0f)
            {
                // Scroll complete; if we have an image, transition to reveal phase
                if (_storyTexture != null)
                {
                    _phase = IntroPhase.ImageRevealing;
                    _storyRevealTimer = 0f;
                }
                else
                {
                    IsFinished = true;
                    return;
                }
            }
        }

        if (_phase == IntroPhase.ImageRevealing)
        {
            _storyRevealTimer += deltaSeconds;

            float revealDuration = _revealMode == StoryRevealMode.PanelByPanel
                ? PanelRevealSeconds * StoryPanelCount
                : SmoothRevealSeconds;

            _storyRevealTimer = Math.Min(_storyRevealTimer, revealDuration + RevealHoldSeconds);
        }

        for (int i = 0; i < _stars.Length; i++)
        {
            Vector2 star = _stars[i];
            star.Y += _starSpeeds[i] * deltaSeconds;

            if (star.Y > RenderManager.VirtualHeight)
            {
                star.Y = 0;
            }

            _stars[i] = star;
        }

        KeyboardState currentKeyboardState = Keyboard.GetState();

        bool continuePressed =
            (currentKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter)) ||
            (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space));

        if (continuePressed)
        {
            IsFinished = true;
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
            new Color(5, 5, 10));

        for (int i = 0; i < _stars.Length; i++)
        {
            Vector2 star = _stars[i];
            int size = i % 3 == 0 ? 2 : 1;
            Color color = i % 5 == 0 ? new Color(255, 245, 200) : Color.White;
            spriteBatch.Draw(_pixel, new Rectangle((int)star.X, (int)star.Y, size, size), color);
        }

        if (_phase == IntroPhase.Scrolling)
        {
            float currentY = _scrollY;
            for (int i = 0; i < _lines.Length; i++)
            {
                string line = _lines[i];
                Vector2 size = _font.MeasureString(line) * StoryTextScale;
                Vector2 linePosition = new Vector2((RenderManager.VirtualWidth - size.X) * 0.5f, currentY);
                spriteBatch.DrawString(
                    _font,
                    line,
                    linePosition,
                    new Color(255, 220, 120),
                    0f,
                    Vector2.Zero,
                    StoryTextScale,
                    SpriteEffects.None,
                    0f);
                currentY += GetStoryLineStep();
            }
        }
        else if (_phase == IntroPhase.ImageRevealing)
        {
            DrawRevealingStoryImage(spriteBatch);
        }

        float pulse = 0.55f + 0.45f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3.2f);
        Color hintColor = Color.White * pulse;
        string hint = "Press ENTER or SPACE to continue";
        Vector2 hintSize = _font.MeasureString(hint) * HintTextScale;
        Vector2 hintPosition = new Vector2(
            (RenderManager.VirtualWidth - hintSize.X) * 0.5f,
            RenderManager.VirtualHeight - hintSize.Y - 24f);
        spriteBatch.DrawString(
            _font,
            hint,
            hintPosition,
            hintColor,
            0f,
            Vector2.Zero,
            HintTextScale,
            SpriteEffects.None,
            0f);

        spriteBatch.End();
    }

    private void DrawRevealingStoryImage(SpriteBatch spriteBatch)
    {
        int targetWidth = RenderManager.VirtualWidth;
        int targetHeight = RenderManager.VirtualHeight;
        Rectangle source = CalculateCoverSourceRect(_storyTexture.Width, _storyTexture.Height, targetWidth, targetHeight);

        if (_revealMode == StoryRevealMode.LeftToRight)
        {
            float progress = MathHelper.Clamp(_storyRevealTimer / SmoothRevealSeconds, 0f, 1f);
            int sourceWidth = Math.Max(1, (int)(source.Width * progress));
            int destinationWidth = Math.Max(1, (int)(targetWidth * progress));

            Rectangle partialSource = new Rectangle(source.X, source.Y, sourceWidth, source.Height);
            Rectangle partialDestination = new Rectangle(0, 0, destinationWidth, targetHeight);
            spriteBatch.Draw(_storyTexture, partialDestination, partialSource, Color.White);
            return;
        }

        int panelWidth = source.Width / StoryPanelCount;
        for (int panelIndex = 0; panelIndex < StoryPanelCount; panelIndex++)
        {
            if (_storyRevealTimer < PanelRevealSeconds * (panelIndex + 1))
            {
                continue;
            }

            int sourceX = source.X + (panelIndex * panelWidth);
            int sourcePanelWidth = panelIndex == StoryPanelCount - 1 ? source.Right - sourceX : panelWidth;
            int destinationX = (int)(targetWidth * ((sourceX - source.X) / (float)source.Width));
            int destinationPanelWidth = panelIndex == StoryPanelCount - 1
                ? targetWidth - destinationX
                : (int)(targetWidth * (sourcePanelWidth / (float)source.Width));

            Rectangle panelSource = new Rectangle(sourceX, source.Y, sourcePanelWidth, source.Height);
            Rectangle panelDestination = new Rectangle(destinationX, 0, destinationPanelWidth, targetHeight);
            spriteBatch.Draw(_storyTexture, panelDestination, panelSource, Color.White);
        }
    }

    private static Rectangle CalculateCoverSourceRect(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        float sourceAspect = sourceWidth / (float)sourceHeight;
        float targetAspect = targetWidth / (float)targetHeight;

        if (sourceAspect > targetAspect)
        {
            int croppedWidth = Math.Max(1, (int)(sourceHeight * targetAspect));
            int offsetX = (sourceWidth - croppedWidth) / 2;
            return new Rectangle(offsetX, 0, croppedWidth, sourceHeight);
        }

        int croppedHeight = Math.Max(1, (int)(sourceWidth / targetAspect));
        int offsetY = (sourceHeight - croppedHeight) / 2;
        return new Rectangle(0, offsetY, sourceWidth, croppedHeight);
    }

    private float GetStoryLineStep()
    {
        return (_font.LineSpacing + StoryLineSpacingOffset) * StoryTextScale;
    }
}