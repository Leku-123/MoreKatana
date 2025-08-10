using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class LeadKatana : KatanaItem
    {
        public const int DashSlashDistance = 400;
        public const float DashSlashTime = 10f;

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

            // クールダウンを有効化
            Item.MKItem().ActivateCooldown(player);

            // ダッシュ切り
            player.CreateDashSlash(player.GetSource_ItemUse(Item), Item.MKItem().AltDamage, Item.knockBack, DashSlashDistance, DashSlashTime);
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