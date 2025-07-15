using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class LeadKatana : KatanaItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;//アイテム判定の横幅（拾得する際に使用）
            Item.height = 54;//アイテム判定の縦幅（拾得する際に使用）

            Item.crit = 0;//デフォルトで4%クリティカル率をもらえる
            Item.useTime = 18;//アイテムを使用していると扱われる時間
            Item.useAnimation = 18;//アイテムのアニメーションを再生する時間

            Item.damage = 15;//与えるダメージ
            Item.knockBack = 6;//与えるノックバック

            Item.value = Item.sellPrice(silver: 5, copper: 80);

            Item.GetGlobalItem<MKItem>().SetKatanaDefaults(Item, 300, true);
        }

        public override void AddRecipes()
        {
            CreateRecipe()//レシピの登録開始
                .AddIngredient(ItemID.LeadBar, 10)//鉛インゴット１０個を
                .AddTile(TileID.Anvils)//金床で使うことで
                .Register();//製作可能にする
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<MKPlayer>().EquipLeadKatana = true;
        }
    }
}
