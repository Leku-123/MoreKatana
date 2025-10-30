using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class PlatinumKatana : BaseOreKatana
    {
        public PlatinumKatana() : base(4, 500, 10f) { }

        public override int HeadID => ItemID.PlatinumHelmet;
        public override int BodyID => ItemID.PlatinumChainmail;
        public override int LegID => ItemID.PlatinumGreaves;

        public override void SetDefaultsItem()
        {
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.damage = 20;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(0, 0, 27);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}