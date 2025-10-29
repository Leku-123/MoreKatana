using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Misc
{
    public class SkyKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Sky;

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Wind, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 58;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4f;
            Item.MKItem().AltDamage = 1;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().skyKatanaJumpEffect = true;
            player.slowFall = true;
        }

        public override void ActiveSkill(Player player)
        {
            // プレイヤーの向きをマウスの方向に向けて、その方向にスカイ刀の発射体をスポーンさせる
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingSkyKatana>(), Item.MKItem().AltDamage, Item.knockBack * 2f, player.whoAmI);
        }

    }
}
