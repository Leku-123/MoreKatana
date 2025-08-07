using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Dusts
{
    public class PixelDust : ModDust
    {
        public override string Texture => null;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 1, 1);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.99f;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 0.95f;

            if (dust.scale < 0.75f)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, Vector2.One, dust.scale, SpriteEffects.None, 0f);
            return true;
        }
    }
}