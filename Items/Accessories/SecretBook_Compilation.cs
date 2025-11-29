using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Accessories
{
    public class SecretBook_Compilation : ArtifactItem
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
            player.MKPlayer().FlashAttackDoubleTap = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SecretBook_BackflipSlash>()
                .AddIngredient<SecretBook_DesolateDive>()
                .AddIngredient<SecretBook_FlashAttack>()
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}