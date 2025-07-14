using MoreKatana.System;
using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    internal class CopperKatana : Basic_Katana
    {
        public override void SetDefaults()
        {
            Item.width = 48;//アイテム判定の横幅（拾得する際に使用）
            Item.height = 54;//アイテム判定の縦幅（拾得する際に使用）

            Item.crit = 0;//デフォルトで4%クリティカル率をもらえる
            Item.useTime = 18;//アイテムを使用していると扱われる時間
            Item.useAnimation = 18;//アイテムのアニメーションを再生する時間

            Item.damage = 10;//与えるダメージ
            Item.knockBack = 6;//与えるノックバック

            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe()//レシピの登録開始
                .AddIngredient(ItemID.CopperBar, 10)//銅インゴット１０個を
                .AddTile(TileID.Anvils)//金床で使うことで
                .Register();//製作可能にする
        }

        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 11));
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<MKPlayer>().EquipCopperKatana = true;
        }
    }
}
