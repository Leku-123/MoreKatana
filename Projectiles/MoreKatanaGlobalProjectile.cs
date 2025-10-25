using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MoreKatana.Projectiles
{
    public class MoreKatanaGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool SourceIsItemUse;
        public bool ActivateCD;
        public bool DashProjectile;

        public bool[] Bool = new bool[Projectile.maxAI];

        private Item OwnerItem;

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            for (int i = 0; i < Projectile.maxAI; i++)
            {
                Bool[i] = binaryReader.ReadBoolean();
            }
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            for (int i = 0; i < Projectile.maxAI; i++)
            {
                binaryWriter.Write(Bool[i]);
            }
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Player player = Main.player[projectile.owner];

            if (source is EntitySource_ItemUse)
                OwnerItem = player.ActiveItem();

            if (SourceIsItemUse)
            {
                if (source is not EntitySource_ItemUse)
                {
                    ActivateCD = false;
                    projectile.Kill();
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];

            if (DashProjectile)
                player.MKPlayer().DashState = true;
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            Player player = Main.player[projectile.owner];

            if (ActivateCD)
                OwnerItem?.MKItem().ActivateCooldown(player);
        }
    }
}