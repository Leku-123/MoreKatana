using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class TinKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Tin;
        public override void SetDefaultsItem()
        {
            Item.width = 44;
            Item.height = 50;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.damage = 12;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(silver: 1, copper: 40);
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
                .AddIngredient(ItemID.TinBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}