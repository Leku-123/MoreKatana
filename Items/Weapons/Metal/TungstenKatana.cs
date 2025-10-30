using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class TungstenKatana : BaseOreKatana
    {
        public TungstenKatana() : base(3, 450, 10f) { }

        public override int HeadID => ItemID.TungstenHelmet;
        public override int BodyID => ItemID.TungstenChainmail;
        public override int LegID => ItemID.TungstenGreaves;

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