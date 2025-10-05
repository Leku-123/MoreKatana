using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    /// <summary>
    /// スカイ刀の風エフェクト
    /// <br>攻撃時、風のエフェクトで敵を吹き飛ばす。
    /// </summary>
    public class SkyKatanaWindImpact : ModProjectile
    {
        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 360;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 1;
            Projectile.knockBack = 25f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }


        public override void AI()
        {
            //ToDo 向かい風の抵抗をそのうち考慮したい

            //float resist = 0.95f;

            //if (Main.windPhysics)
            //    resist *= Main.windSpeedTarget;

            Projectile.velocity *= 0.95f;
        }
    }
}
