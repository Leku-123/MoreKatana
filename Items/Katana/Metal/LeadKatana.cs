using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class LeadKatana : KatanaItem
    {
        public override KatanaIndex KatanaID => KatanaIndex.LeadKatana;
        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 15;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(silver: 5, copper: 80);
            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {

        }

        public override void ActiveSkill(Player player)
        {
            Item.MKItem().ActivateCooldown(player);
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