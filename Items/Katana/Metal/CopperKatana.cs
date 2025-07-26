using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class CopperKatana : KatanaItem
    {
        public override KatanaIndex KatanaID => KatanaIndex.CopperKatana;
        public override void SetDefaultsItem()
        {
            Item.width = 44;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.damage = 10;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(silver: 1);
            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 1;
        }

        public override void ActiveSkill(Player player)
        {
            Item.MKItem().ActivateCooldown(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CopperBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}