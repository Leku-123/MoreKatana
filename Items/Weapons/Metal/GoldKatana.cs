using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class GoldKatana : BaseOreKatana
    {
        public GoldKatana() : base(4, 500, 10f) { }

        public override KatanaID ID => KatanaID.Gold;

        public override void SetDefaultsItem()
        {
            Item.width = 50;
            Item.height = 60;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.damage = 18;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(0, 0, 18);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}