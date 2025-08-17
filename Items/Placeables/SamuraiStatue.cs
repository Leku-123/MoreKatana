using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Placeables
{
    public class SamuraiStatue : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.SamuraiStatue>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
              .AddIngredient(ItemID.StoneBlock, 50)
              .AddTile(TileID.HeavyWorkBench)
              .Register();
        }
    }
}