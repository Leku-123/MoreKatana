using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class DiamondKatana : BaseGemKatana
    {
        public DiamondKatana() : base(4) { }

        public override KatanaID ID => KatanaID.Diamond;

        public override void SetDefaultsItem()
        {
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.damage = 19;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 1, 80, 0);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<GemShards_Diamond>();
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GoldKatana>())
                .AddIngredient(ItemID.Diamond, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PlatinumKatana>())
                .AddIngredient(ItemID.Diamond, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}