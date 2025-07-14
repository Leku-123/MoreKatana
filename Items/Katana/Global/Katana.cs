using MoreKatana.System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.Global
{
    public class Katana : GlobalItem
    {

        public override void HoldItem(Item item, Player player)
        {
            if (item.type == ItemID.Katana)
                player.GetModPlayer<MKPlayer>().EquipKatana = true;
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (item.type == ItemID.Katana)
                player.GetModPlayer<MKPlayer>().EquipKatana = true;
        }

    }
}
