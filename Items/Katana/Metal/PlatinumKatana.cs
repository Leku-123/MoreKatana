using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class PlatinumKatana : KatanaItem
    {
        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.damage = 18;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(silver: 30);
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
                .AddIngredient(ItemID.PlatinumBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}