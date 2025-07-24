using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class TinKatana : KatanaItem
    {
        public override void SetDefaultsItem()
        {
            Item.width = 48;//アイテム判定の横幅（拾得する際に使用）
            Item.height = 54;//アイテム判定の縦幅（拾得する際に使用）

            Item.crit = 0;//デフォルトで4%クリティカル率をもらえる
            Item.useTime = 20;//アイテムを使用していると扱われる時間
            Item.useAnimation = 20;//アイテムのアニメーションを再生する時間

            Item.damage = 12;//与えるダメージ
            Item.knockBack = 5.5f;//与えるノックバック

            Item.value = Item.sellPrice(silver: 1, copper: 40);

            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void AddRecipes()
        {
            CreateRecipe()//レシピの登録開始
                .AddIngredient(ItemID.TinBar, 10)//錫インゴット１０個を
                .AddTile(TileID.Anvils)//金床で使うことで
                .Register();//製作可能にする
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 2;
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.friendly)
                target.damage -= 1;
        }
    }
}
