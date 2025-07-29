using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class PlatinumKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Platinum;

        public override void SetDefaultsItem()
        {
            Item.width = 50;
            Item.height = 60;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.UseSound = SoundID.Item1;

            Item.damage = 18;
            Item.knockBack = 7;

            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 4;
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.Item71;
            Item.MKItem().ActivateCooldown(player);
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.damage, Item.knockBack, 500, 10f);
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