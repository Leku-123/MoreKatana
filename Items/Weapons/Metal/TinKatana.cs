using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
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
            Item.UseSound = SoundID.Item1;

            Item.damage = 12;
            Item.knockBack = 5.5f;
            Item.MKItem().AltDamage = 24;

            Item.value = Item.sellPrice(silver: 1, copper: 40);
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
                .AddIngredient(ItemID.TinBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}