using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class SkyJumpDamageEffect : ModProjectile
    {
        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            //とりあえず瓶雲用にプロパティを調整
            Projectile.width = 60;
            Projectile.height = 24;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        // メモ
        // エクストラジャンプ中は透明かつ、小さ目のProjectileをプレイヤーの足元に毎フレーム発生させる？
        // (Projectileは一定時間後(大体Dustが消えるのに合わせて)消えるみたいな)
        // 未検証だがジャンプ発生源も取得できるので、雲は横幅を広くとる。
    }
}
