using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Wood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Wood
{
    public class WoodenKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Wood;

        public override void SetDefaultsItem()
        {
            Item.width = 46;
            Item.height = 48;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 7;
            Item.knockBack = 5;
            Item.MKItem().AltDamage = 30;

            Item.value = Item.sellPrice(copper: 25);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void ActiveSkill(Player player)
        {
            // プレイヤーの向きをマウスの方向に向けて、その方向に木刀の発射体をスポーンさせる
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingWoodenSwing>(), Item.MKItem().AltDamage, Item.knockBack * 2f, player.whoAmI);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 12)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}