using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class Empty : ModProjectile
    {
        public override string Texture => MoreKatana.EmptyTexture;
        public override void SetDefaults() => Projectile.timeLeft = 0;
        public override bool? CanDamage() => false;
    }
}