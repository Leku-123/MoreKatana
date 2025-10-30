using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class GoldKatana : BaseOreKatana
    {
        public GoldKatana() : base(4, 500, 10f) { }

        public override int HeadID => ItemID.GoldHelmet;
        public override int BodyID => ItemID.GoldChainmail;
        public override int LegID => ItemID.GoldGreaves;
        public override int[] AltHeadIDs => [ItemID.AncientGoldHelmet];

        public override void SetDefaultsItem()
        {
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