using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class GoldKatana : KatanaItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;//アイテム判定の横幅（拾得する際に使用）
            Item.height = 60;//アイテム判定の縦幅（拾得する際に使用）

            Item.crit = 0;//デフォルトで4%クリティカル率をもらえる
            Item.useTime = 16;//アイテムを使用していると扱われる時間
            Item.useAnimation = 16;//アイテムのアニメーションを再生する時間

            Item.damage = 20;//与えるダメージ
            Item.knockBack = 7;//与えるノックバック

            Item.value = Item.sellPrice(silver: 20);

            Item.GetGlobalItem<MKItem>().SetKatanaDefaults(Item, 300, true);
        }

        public override void AddRecipes()
        {
            CreateRecipe()//レシピの登録開始
                .AddIngredient(ItemID.GoldBar, 10)//金インゴット１０個を
                .AddTile(TileID.Anvils)//金床で使うことで
                .Register();//製作可能にする
        }
    }
}
