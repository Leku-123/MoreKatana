using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class SilverKatana : BaseOreKatana
    {
        public SilverKatana() : base(3, 450, 10f) { }

        public override KatanaID ID => KatanaID.Silver;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 20;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(0, 0, 9);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SilverBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}