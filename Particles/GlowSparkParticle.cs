using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MoreKatana.Particles
{
    public class GlowSparkParticle : Particle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public bool QuickShrink;
        public bool Glowing;
        public Vector2 Squash = new Vector2(0.5f, 1.6f);
        public int MaxTime;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public GlowSparkParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int maxTime, float scale, Color color, Vector2 squash, bool quickShrink = false, bool glow = true)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            MaxTime = maxTime;
            Color = InitialColor = color;
            Squash = squash;
            QuickShrink = quickShrink;
            Glowing = glow;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            if (TimeActive >= MaxTime)
                Kill();

            Scale *= 0.95f;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(MaxTime != 0 ? TimeActive / (float)MaxTime : 0, 3D));
            Velocity *= 0.95f;
            if (QuickShrink)
            {
                Squash.X *= 0.8f;
                Squash.Y *= 1.12f;
            }
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = Squash * Scale;
            Texture2D texture = ParticleHandler.GetTexture(Type);

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            if (Glowing)
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.White, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
        }
    }
}