using MoreKatana.Items.Weapons.Misc;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Items.Placeables
{
    public class SamuraiStatueGift : SamuraiStatue
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.createTile = ModContent.TileType<Tiles.SamuraiStatueGiftDecorative>();
            Item.accessory = true;
            Item.hasVanityEffects = true;
        }

        public override void UpdateEquip(Player player) => ShrineEffect(player);

        public override void UpdateVanity(Player player) => ShrineEffect(player);

        private void ShrineEffect(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.MKPlayer().ForgottenAltarEffect = 30;
                player.MKPlayer().ForgottenAltarMusicOverride = 30;
            }
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