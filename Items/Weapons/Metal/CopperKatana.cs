using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class CopperKatana : BaseOreKatana
    {
        public CopperKatana() : base(1, 350, 10f) { }

        public override KatanaID ID => KatanaID.Copper;

        public override void SetDefaultsItem()
        {
            Item.width = 44;
            Item.height = 50;
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