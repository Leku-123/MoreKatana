using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Wood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.Wood
{
    public class WoodenKatana : KatanaItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 48;

            Item.useTime = 15;
            Item.useAnimation = 15;

            Item.damage = 8;
            Item.knockBack = 5;

            Item.value = Item.sellPrice(copper: 25);

            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void ActiveSkill(Player player)
        {
            Main.NewText("a");
            var source = player.GetSource_ItemUse(Item);
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingWoodenSwing>(), Item.damage * 3, Item.knockBack * 2f, player.whoAmI);
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