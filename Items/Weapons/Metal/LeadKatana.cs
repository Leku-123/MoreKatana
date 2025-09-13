using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class LeadKatana : BaseOreKatana
    {
        public LeadKatana() : base(2, 400, 10f) { }

        public override KatanaID ID => KatanaID.Lead;

        public override void SetDefaultsItem()
        {
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 17;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(0, 0, 5, 40);
            base.SetDefaultsItem();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}