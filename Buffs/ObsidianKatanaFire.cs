using Microsoft.Xna.Framework;
using MoreKatana.Items.Weapons.Metal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Buffs
{
    public class ObsidianKatanaFire : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HeldItem.type != ModContent.ItemType<ObsidianKatana>())
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }

            if (!player.mount.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    int newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, DustID.HallowedWeapons, 0, Main.rand.Next(-5, -2), 150, Color.Red, 1f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                    newDust = Dust.NewDust(new Vector2(player.Center.X - player.width, player.Center.Y + player.height / 2), player.width * 2 - 3, 0, DustID.Torch, 0, Main.rand.Next(-5, -2), 150, default, 1f);
                    Main.dust[newDust].fadeIn = 0.3f;
                    Main.dust[newDust].noGravity = true;
                }
            }
        }

        public override bool RightClick(int buffIndex) => false;
    }
}