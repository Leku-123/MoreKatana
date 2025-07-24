using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles
{
    public class MoreKatanaGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool SourceIsItemUse;

        public bool ActivateCD;

        private Item OwnerItem;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Player player = Main.player[projectile.owner];

            if (source is EntitySource_ItemUse)
                OwnerItem = player.ActiveItem();

            if (SourceIsItemUse)
            {
                if (source is not EntitySource_ItemUse)
                    projectile.Kill();
            }
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            Player player = Main.player[projectile.owner];

            if (ActivateCD)
                OwnerItem.MKItem().ActivateCooldown(player);
        }
    }
}