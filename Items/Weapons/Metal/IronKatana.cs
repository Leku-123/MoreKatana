using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class IronKatana : BaseOreKatana
    {
        public IronKatana() : base(2, 400, 10f) { }

        public override KatanaID ID => KatanaID.Iron;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 15;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(0, 0, 3, 60);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}