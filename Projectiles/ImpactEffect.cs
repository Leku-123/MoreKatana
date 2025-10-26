using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class ImpactEffect : ModProjectile
    {
        public Color ImpactColor;
        public float ImpactSize;

        public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 3;

        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > Main.projFrames[Projectile.type])
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity != Vector2.Zero)
                return false;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            position += Projectile.rotation.ToRotationVector2() * 188f * ImpactSize;
            Color color = ImpactColor with { A = 0 };
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, ImpactSize, SpriteEffects.None, 0);
            return false;
        }
    }
}