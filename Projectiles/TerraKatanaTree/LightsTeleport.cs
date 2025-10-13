using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class LightsTeleport : ModProjectile
    {
        private Vector2 teleportPos;

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            teleportPos = Projectile.Center;

            Player player = Main.player[Projectile.owner];
            player.Teleport(teleportPos, 1);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, teleportPos.X, teleportPos.Y, 1);
        }
    }
}