using MoreKatana.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Systems
{
    public class RecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.DirtBlock)
                .AddIngredient(ItemID.Katana)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void PostSetupContent()
        {
            int[] convert = ItemID.Sets.ShimmerTransformToItem;

            convert[ItemID.BrokenHeroSword] = ModContent.ItemType<BrokenHeroKatana>();
            convert[ModContent.ItemType<BrokenHeroKatana>()] = ItemID.BrokenHeroSword;
        }
    }
}