using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Wood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.Wood
{
    public class WoodenKatana : KatanaItem
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
            Item.MKItem().SetKatanaDefaults(Item, 0);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = MoreKatanaSounds.MuteSound;
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingWoodenSwing>(), Item.damage * 5, Item.knockBack * 2f, player.whoAmI);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}