using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class TopazKatana : BaseGemKatana
    {
        public TopazKatana() : base(2) { }

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 15;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 0, 45);
            Item.rare = ItemRarityID.White;
            Item.shoot = ModContent.ProjectileType<GemShards_Topaz>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CopperKatana>())
                .AddIngredient(ItemID.Topaz, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TinKatana>())
                .AddIngredient(ItemID.Topaz, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}