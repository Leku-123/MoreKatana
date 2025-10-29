using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MoreKatana.Particles
{
    public class MiniGlowParticle : Particle
    {
        public override bool UseAdditiveBlend => true;

        public float Opacity;
        private Color ColorFire;
        private Color ColorFade;
        private bool Lighted;
        public int Variant;

        public MiniGlowParticle(Vector2 position, Vector2 speed, Color colorFire, Color colorFade, float scale = 1f, float opacity = 1f, bool lighted = false)
        {
            Position = position;
            Scale = scale;
            ColorFire = colorFire;
            ColorFade = colorFade;
            Opacity = opacity;
            Velocity = speed;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Lighted = lighted;
            Variant = Main.rand.Next(2);
        }

        public override void Update()
        {
            Velocity *= 0.85f;

            if (Opacity > 90)
            {
                if (!Lighted)
                    Lighting.AddLight(Position, Color.ToVector3() * 0.1f);
                Scale += 0.01f;
                Opacity -= 3;
            }
            else
            {
                Scale *= 0.975f;
                Opacity -= 2;
            }
            if (Opacity < 0)
                Kill();

            Color = Color.Lerp(ColorFire, ColorFade, MathHelper.Clamp((float)(255 - Opacity - 100) / 80, 0f, 1f)) * (Opacity / 255f);
            if (Lighted)
                Color = Color.MultiplyRGBA(Lighting.GetColor(Position.ToTileCoordinates()));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleHandler.GetTexture(Type);
            Rectangle rectangle = new Rectangle(0, texture.Height / 2 * Variant, texture.Width, texture.Height / 2);
            spriteBatch.Draw(texture, Position - Main.screenPosition, rectangle, Color * Opacity, Rotation, texture.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}