using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class TungstenKatana : KatanaItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;//アイテム判定の横幅（拾得する際に使用）
            Item.height = 54;//アイテム判定の縦幅（拾得する際に使用）

            Item.crit = 0;//デフォルトで4%クリティカル率をもらえる
            Item.useTime = 16;//アイテムを使用していると扱われる時間
            Item.useAnimation = 16;//アイテムのアニメーションを再生する時間

            Item.damage = 20;//与えるダメージ
            Item.knockBack = 6;//与えるノックバック

            Item.value = Item.sellPrice(silver: 15, copper: 50);
        }

        public override void AddRecipes()
        {
            CreateRecipe()//レシピの登録開始
                .AddIngredient(ItemID.TungstenBar, 10)//タングステンインゴット１０個を
                .AddTile(TileID.Anvils)//金床で使うことで
                .Register();//製作可能にする
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<MKPlayer>().EquipTungstenKatana = true;
        }
    }
}
