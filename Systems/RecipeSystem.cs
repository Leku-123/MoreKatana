using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Systems
{
    public class RecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.Katana)
                .AddIngredient(ItemID.IronBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}