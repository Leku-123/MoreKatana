using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class RubyKatana : BaseGemKatana
    {
        public RubyKatana() : base(4) { }

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 18;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 1, 35);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<GemShards_Ruby>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SilverKatana>())
                .AddIngredient(ItemID.Ruby, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TungstenKatana>())
                .AddIngredient(ItemID.Ruby, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}