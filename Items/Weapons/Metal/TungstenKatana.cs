using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Metal
{
    public class TungstenKatana : KatanaItem
    {
        public const int DashSlashDistance = 450;
        public const float DashSlashTime = 10f;

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
            Item.MKItem().AltDamage = 40;

            Item.value = Item.sellPrice(silver: 15, copper: 50);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 3;
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
                .AddIngredient(ItemID.TungstenBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}