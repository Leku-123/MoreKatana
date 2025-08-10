using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class GoldKatana : KatanaItem
    {
        public const int DashSlashDistance = 500;
        public const float DashSlashTime = 10f;

        public override KatanaID ID => KatanaID.Gold;

        public override void SetDefaultsItem()
        {
            Item.width = 50;
            Item.height = 60;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.UseSound = SoundID.Item1;

            Item.damage = 20;
            Item.knockBack = 7;
            Item.MKItem().AltDamage = 40;

            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 4;
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
                .AddIngredient(ItemID.GoldBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}