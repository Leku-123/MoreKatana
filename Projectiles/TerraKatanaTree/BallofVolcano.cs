using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class BallofVolcano : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BallofFire);
            Projectile.aiStyle = ProjAIStyleID.Bounce;
            AIType = ProjectileID.BallofFire;

            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source) => Projectile.ai[0] += 1;
    }
}
