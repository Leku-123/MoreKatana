using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Accessories
{
    public class SecretBook_DesolateDive : ArtifactItem
    {
        public override void SetDefaultsItem()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.sellPrice(silver: 90);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.MKPlayer().BackflipSlashDoubleTap = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book)
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}