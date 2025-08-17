using MoreKatana.Items.Weapons.Misc;
using Terraria.ModLoader;

namespace MoreKatana.Items.Placeables
{
    public class SamuraiStatueGift : SamuraiStatue
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.createTile = ModContent.TileType<Tiles.SamuraiStatueGift>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SamuraiStatue>())
                .AddIngredient(ModContent.ItemType<EnchantedKatana>())
                .Register();
        }
    }
}