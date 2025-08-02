using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class CopperKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Copper;

        public override void SetDefaultsItem()
        {
            Item.width = 44;
            Item.height = 50;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.UseSound = SoundID.Item1;

            Item.damage = 10;
            Item.knockBack = 6;
            Item.MKItem().AltDamage = 20;

            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 1;
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.Item71;
            Item.MKItem().ActivateCooldown(player);
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.MKItem().AltDamage, Item.knockBack, 350, 10f);
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