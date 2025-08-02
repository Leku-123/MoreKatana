using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class LeadKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Lead;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.UseSound = SoundID.Item1;

            Item.damage = 15;
            Item.knockBack = 6;
            Item.MKItem().AltDamage = 30;

            Item.value = Item.sellPrice(silver: 5, copper: 80);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 2;
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.Item71;
            Item.MKItem().ActivateCooldown(player);
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.MKItem().AltDamage, Item.knockBack, 400, 10f);
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