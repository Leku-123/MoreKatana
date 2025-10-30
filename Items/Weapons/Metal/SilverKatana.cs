using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class SilverKatana : BaseOreKatana
    {
        public SilverKatana() : base(3, 450, 10f) { }

        public override int HeadID => ItemID.SilverHelmet;
        public override int BodyID => ItemID.SilverChainmail;
        public override int LegID => ItemID.SilverGreaves;

        public override void SetDefaultsItem()
        {
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