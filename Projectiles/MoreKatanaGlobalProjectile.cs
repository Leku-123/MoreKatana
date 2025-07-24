using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatanaGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool SourceIsItemUse;
    
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Player player = Main.player[projectile.owner];

            if (SourceIsItemUse)
            {
                if (source is not EntitySource_ItemUse_WithAmmo && source is not EntitySource_ItemUse)
                    projectile.Kill();
            }
        }
    }
}