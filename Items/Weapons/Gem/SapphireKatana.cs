using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class SapphireKatana : BaseGemKatana
    {
        public SapphireKatana() : base(3) { }

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 16;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 0, 67, 50);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<GemShards_Sapphire>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<IronKatana>())
                .AddIngredient(ItemID.Sapphire, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LeadKatana>())
                .AddIngredient(ItemID.Sapphire, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}