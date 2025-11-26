using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class AmethystKatana : BaseGemKatana
    {
        public AmethystKatana() : base(2) { }

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 14;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 0, 22, 50);
            Item.rare = ItemRarityID.White;
            Item.shoot = ModContent.ProjectileType<GemShards_Amethyst>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CopperKatana>()
                .AddIngredient(ItemID.Amethyst, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient<TinKatana>()
                .AddIngredient(ItemID.Amethyst, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}