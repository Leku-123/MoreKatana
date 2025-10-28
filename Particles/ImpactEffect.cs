using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MoreKatana.Particles
{
    public class ImpactEffect : Particle
    {
        private Vector2 Direction;
        private Vector2 scaleMod;
        private int FrameCounter;
        private int Frame;
        private int MaxTime;
        private const int MaxFrames = 3;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public ImpactEffect(Vector2 position, Vector2 direction, Color color, Vector2 scale, int maxTime)
        {
            Position = position;
            Direction = direction;
            Color = color;
            scaleMod = scale;
            MaxTime = maxTime;
        }

        public override void Update()
        {
            if (Velocity != Vector2.Zero)
                Velocity = Vector2.Zero;

            Rotation = Direction.ToRotation();

            if (TimeActive > MaxTime)
                Kill();

            FrameCounter++;
            if (FrameCounter > MaxTime / 3)
            {
                FrameCounter = 0;
                Frame++;
                if (Frame > MaxFrames)
                    Kill();
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleHandler.GetTexture(Type);
            Vector2 position = Position + Rotation.ToRotationVector2() * 188f * scaleMod.X;
            Rectangle rectangle = texture.Frame(1, MaxFrames, 0, Frame);
            Vector2 origin = rectangle.Size() / 2f;
            spriteBatch.Draw(texture, position - Main.screenPosition, rectangle, Color, Rotation, origin, scaleMod, SpriteEffects.None, 0);
        }
    }
}