using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class IceBoltProjectile : ModProjectile
    {
        private KatanaSlashPrimTrail trail;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.coldDamage = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        private bool primsCreated;
        public override void AI()
        {
            if (!primsCreated)
            {
                primsCreated = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    trail = new KatanaSlashPrimTrail(Projectile, Color.White);
                    MoreKatana.primitives.CreateTrail(trail);
                }
            }

        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
                trail?.OnDestroy();
        }
    }
}