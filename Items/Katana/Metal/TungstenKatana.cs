using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class TungstenKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Tungsten;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.UseSound = SoundID.Item1;
            Item.damage = 20;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(silver: 15, copper: 50);
            Item.MKItem().SetKatanaDefaults(Item, 0, true);
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
                .AddIngredient(ItemID.TungstenBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}