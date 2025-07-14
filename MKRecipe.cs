using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MKRecipe : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe recipe;

            //デフォルト刀のレシピ追加
            recipe = Recipe.Create(ItemID.Katana);
            recipe.AddIngredient(ItemID.IronBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
