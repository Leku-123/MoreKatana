using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Metal
{
    public class ObsidianSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            AIType = ProjectileID.DD2SquireSonicBoom;
            Projectile.width = 80;
            Projectile.height = 48;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor.R = 255;
            return base.PreDraw(ref lightColor);
        }
    }
}
