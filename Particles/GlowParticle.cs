using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MoreKatana.Particles
{
    public class GlowParticle : Particle
    {
        private float Opacity;
        private Color GlowColor;
        public int MaxTime;
        private int FadeTime;
        private float FadeRate => 1f / FadeTime;
        private float MinimumOpacity => 0.6f;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public GlowParticle(Vector2 position, Vector2 velocity, Color color, float scale, int maxTime, int fadeTime = 30)
        {
            Position = position;
            Velocity = velocity;
            GlowColor = color;
            Scale = scale;
            MaxTime = maxTime;
            FadeTime = fadeTime;
        }

        public override void Update()
        {
            if (TimeActive >= MaxTime)
                Opacity -= FadeRate;
            else if (TimeActive < FadeTime && Opacity < MinimumOpacity)
                Opacity += FadeRate;
            else
                Opacity = MinimumOpacity + (float)Math.Sin((TimeActive - FadeTime) / 30f) * 0.3f;

            Color = GlowColor * Opacity;
            Lighting.AddLight(Position, Color.R / 255f, Color.G / 255f, Color.B / 255f);
            Velocity = Velocity.RotatedByRandom(0.03f) * 0.99f;
            Scale *= 0.99f;

            if (Opacity <= 0f)
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch) => spriteBatch.Draw(ParticleHandler.GetTexture(Type), Position - Main.screenPosition, null, Color, Rotation, ParticleHandler.GetTexture(Type).Size() / 2, Scale, SpriteEffects.None, 0f);
    }
}