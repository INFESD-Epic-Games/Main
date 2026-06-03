using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpellFall.Enemies
{
    public abstract partial class Enemy
    {
        private const float HitFlashDurationSeconds = 0.28f;
        private const float HitFlashAlpha = 1f;
        private const float HitFlashGlowScale = 1.16f;
        private const float StatusFlashGlowScale = 1.08f;
        private const float StatusFlashAlpha = 0.5f;

        private float _hitFlashTimer = 0f;
        private float _statusFlashTimer = 0f;
        private Color _statusFlashColor = Color.CornflowerBlue;

        protected void DrawEnemySprite(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color tint,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects spriteEffects,
            float layerDepth)
        {
            spriteBatch.Draw(
                texture,
                position,
                sourceRectangle,
                tint,
                rotation,
                origin,
                scale,
                spriteEffects,
                layerDepth);

            if (_hitFlashTimer > 0f)
            {
                float flashStrength = Math.Clamp(_hitFlashTimer / HitFlashDurationSeconds, 0f, 1f) * HitFlashAlpha;
                Color flashTint = Color.Lerp(tint, Color.White, Math.Min(1f, flashStrength * 1.15f));
                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRectangle,
                    flashTint,
                    rotation,
                    origin,
                    scale,
                    spriteEffects,
                    layerDepth);

                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRectangle,
                    Color.White * (flashStrength * 0.55f),
                    rotation,
                    origin,
                    scale * HitFlashGlowScale,
                    spriteEffects,
                    layerDepth);
            }

            if (_statusFlashTimer > 0f)
            {
                float statusStrength = StatusFlashAlpha;
                Color statusTint = Color.Lerp(tint, _statusFlashColor, 0.85f);
                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRectangle,
                    statusTint,
                    rotation,
                    origin,
                    scale,
                    spriteEffects,
                    layerDepth);

                spriteBatch.Draw(
                    texture,
                    position,
                    sourceRectangle,
                    _statusFlashColor * statusStrength,
                    rotation,
                    origin,
                    scale * StatusFlashGlowScale,
                    spriteEffects,
                    layerDepth);
            }
        }

        public void ApplyHitFlash(float durationSeconds = HitFlashDurationSeconds)
        {
            _hitFlashTimer = Math.Max(_hitFlashTimer, Math.Max(0f, durationSeconds));
        }

        public void ApplyStatusFlash(Color color, float durationSeconds)
        {
            _statusFlashColor = color;
            _statusFlashTimer = Math.Max(_statusFlashTimer, Math.Max(0f, durationSeconds));
        }

        protected void UpdateVisualEffects(float dt)
        {
            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= dt;
                if (_hitFlashTimer <= 0f)
                {
                    _hitFlashTimer = 0f;
                }
            }

            if (_statusFlashTimer > 0f)
            {
                _statusFlashTimer -= dt;
                if (_statusFlashTimer <= 0f)
                {
                    _statusFlashTimer = 0f;
                }
            }
        }
    }
}