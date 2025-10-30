using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class CopperKatana : BaseOreKatana
    {
        public CopperKatana() : base(1, 350, 10f) { }

        public override int HeadID => ItemID.CopperHelmet;
        public override int BodyID => ItemID.CopperChainmail;
        public override int LegID => ItemID.CopperGreaves;

        public override void SetDefaultsItem()
        {
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 10;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(0, 0, 0, 90);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CopperBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}