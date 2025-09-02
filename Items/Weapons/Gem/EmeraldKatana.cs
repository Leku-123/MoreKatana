using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class EmeraldKatana : BaseGemKatana
    {
        public EmeraldKatana() : base(3) { }

        public override KatanaID ID => KatanaID.Emerald;

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 17;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 0, 90);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<GemShards_Emerald>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SilverKatana>())
                .AddIngredient(ItemID.Emerald, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TungstenKatana>())
                .AddIngredient(ItemID.Emerald, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}