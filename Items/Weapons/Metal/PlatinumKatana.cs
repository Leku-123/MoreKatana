using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class PlatinumKatana : BaseOreKatana
    {
        public PlatinumKatana() : base(4, 500, 10f) { }

        public override KatanaID ID => KatanaID.Platinum;

        public override void SetDefaultsItem()
        {
            Item.width = 50;
            Item.height = 60;
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