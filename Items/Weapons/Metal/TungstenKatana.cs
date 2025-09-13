using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class TungstenKatana : BaseOreKatana
    {
        public TungstenKatana() : base(3, 450, 10f) { }

        public override KatanaID ID => KatanaID.Tungsten;

        public override void SetDefaultsItem()
        {
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.damage = 22;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(0, 0, 13, 50);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TungstenBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}