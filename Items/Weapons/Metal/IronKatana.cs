using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class IronKatana : BaseOreKatana
    {
        public IronKatana() : base(2, 400, 10f) { }

        public override int HeadID => ItemID.IronHelmet;
        public override int BodyID => ItemID.IronChainmail;
        public override int LegID => ItemID.IronGreaves;
        public override int[] AltHeadIDs => [ItemID.AncientIronHelmet];

        public override void SetDefaultsItem()
        {
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