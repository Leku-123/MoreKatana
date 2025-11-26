using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Misc
{
    public class SkyKatana : KatanaItem
    {
        public const int FeatherCountInExtraJump = 6;

        public override LocalizedText FunctionText => base.FunctionText.WithFormatArgs(FeatherCountInExtraJump);

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Wind, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 58;

            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4f;
            Item.MKItem().AltDamage = 25;

            Item.shoot = ModContent.ProjectileType<SkyFeather>();
            Item.shootSpeed = 10f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().skyJumpEffect = true;
            player.MKPlayer().slowFallEffect = 2;
        }

        public override void ActiveSkill(Player player)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingSkyKatana>(), Item.MKItem().AltDamage, 20f, player.whoAmI);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => !player.IsUsingAlt();
    }
}
