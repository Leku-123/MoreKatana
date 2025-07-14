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

            if (item.type == ItemID.Muramasa)
                player.GetModPlayer<MKPlayer>().EquipMuramasa = true;
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (item.type == ItemID.Katana)
                player.GetModPlayer<MKPlayer>().EquipKatana = true;

            if (item.type == ItemID.Muramasa)
                player.GetModPlayer<MKPlayer>().EquipMuramasa = true;
        }

        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            // ムラマサのヒトダマ発生処理
        }

        public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
        {
            // ムラマサのヒトダマ発生処理
        }
    }
}
