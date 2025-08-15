using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.ZenithKatanaTree
{
    public class EnchantedKatanaBeam : ModProjectile
    {
        public int CollidingLeft = 3;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.EnchantedBeam);
            Projectile.aiStyle = ProjAIStyleID.Beam;
            AIType = ProjectileID.EnchantedBeam;

            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[2] != 0f)
                Projectile.tileCollide = false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (CollidingLeft <= 0)
                return true;

            Projectile.direction *= -1;
            Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(90f));

            CollidingLeft--;
            return false;
        }
    }
}
