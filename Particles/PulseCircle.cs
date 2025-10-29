using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Particles
{
    public class PulseCircle : Particle
    {
        private EasingFunction easing;

        private Vector2 Direction;
        private Vector2 ScaleMod;
        private int MaxTime;
        private bool FullBright;
        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public PulseCircle(Vector2 position, Vector2 direction, Color color, Vector2 scale, int maxTime, EasingFunction mode = null, bool fullBright = false)
        {
            Position = position;
            Direction = direction;
            Color = color;
            ScaleMod = scale;
            MaxTime = maxTime;
            easing = mode ?? LinearEasing;
            FullBright = fullBright;
        }

        public override void Update()
        {
            if (Velocity != Vector2.Zero)
                Velocity = Vector2.Zero;

            Rotation = Direction.ToRotation();

            if (TimeActive > MaxTime)
                Kill();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleHandler.GetTexture(Type);
            float progress = easing((float)TimeActive / MaxTime, 1);
            Vector2 scale = 0.1f * ScaleMod * progress;
            Color color = Color * (!FullBright ? (1f - progress * 0.8f) : 1f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, color, Rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);
        }
    }
}