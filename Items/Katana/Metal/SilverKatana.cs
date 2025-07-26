using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class SilverKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Silver;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.UseSound = SoundID.Item1;
            Item.damage = 22;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(silver: 10);
            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 3;
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.Item71;
            Item.MKItem().ActivateCooldown(player);
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.damage, Item.knockBack, 450, 10f);
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