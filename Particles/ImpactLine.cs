using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MoreKatana.Particles
{
    public class ImpactLine : Particle
    {
        private readonly Entity etity = null;

        private Color InitialColor;
        private Vector2 scaleMod;
        private Vector2 Offset;
        private int MaxTime;

        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public ImpactLine(Vector2 position, Vector2 velocity, Color color, Vector2 scale, int timeLeft, Entity attatchedEntity = null)
        {
            Position = position;
            Velocity = velocity;
            InitialColor = color;
            scaleMod = scale;
            MaxTime = timeLeft;
            etity = attatchedEntity;

            if (etity != null)
                Offset = Position - etity.Center;
        }

        public override void Update()
        {
            float opacity = (float)Math.Sin(TimeActive / (float)MaxTime * MathHelper.Pi);
            Color = InitialColor * opacity;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Position, Color.ToVector3() / 2f);

            if (etity != null)
            {
                if (!etity.active)
                {
                    Kill();
                    return;
                }
                Position = etity.Center + Offset;
                Offset += Velocity;
            }

            if (TimeActive > MaxTime)
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float progress = (float)Math.Sin(TimeActive / (float)MaxTime * MathHelper.Pi);
            Vector2 scale = new Vector2(0.5f, progress) * scaleMod;
            Vector2 offset = Vector2.Zero;
            Texture2D tex = ParticleHandler.GetTexture(Type);
            Vector2 origin = new Vector2(tex.Width / 2, tex.Height);

            if (TimeActive > MaxTime / 2)
            {
                offset = Vector2.UnitX.RotatedBy(Rotation - MathHelper.PiOver2) * tex.Height * scale.Y;
                origin.Y = 0;
            }

            spriteBatch.Draw(tex, Position + offset - Main.screenPosition, null, Color * (progress / 5 + 0.8f), Rotation, origin, scale, SpriteEffects.None, 0);
        }
    }
}