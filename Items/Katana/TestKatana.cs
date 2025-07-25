using MoreKatana.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana
{
    public class TestKatana : KatanaItem
    {
        public override void SetDefaultsItem()
        {
            Item.width = 46;
            Item.height = 48;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item1;
            Item.damage = 8;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(copper: 25);
            Item.MKItem().SetKatanaDefaults(Item, 0, false);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = MoreKatanaSounds.MuteSound;
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new(player.direction, 0), ModContent.ProjectileType<CustomDashSlash>(), Item.damage, Item.knockBack, player.whoAmI);
        }
    }
}
