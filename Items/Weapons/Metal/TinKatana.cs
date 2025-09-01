using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class TinKatana : BaseOreKatana
    {
        public TinKatana() : base(1, 350, 10f) { }

        public override KatanaID ID => KatanaID.Tin;

        public override void SetDefaultsItem()
        {
            Item.width = 44;
            Item.height = 50;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.damage = 12;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(0, 0, 1, 35);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TinBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}