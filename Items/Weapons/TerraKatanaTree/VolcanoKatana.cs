using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class VolcanoKatana : KatanaItem
    {
        private bool Bomber = true;

        private readonly int[] VolcanoBuffImmune = [BuffID.Frostburn, BuffID.Chilled, BuffID.Frozen, BuffID.Frostburn2];

        public override KatanaID ID => KatanaID.Volcano;

        public override void SetDefaultsItem()
        {
            Item.width = 56;
            Item.height = 70;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.UseSound = SoundID.Item1;

            Item.damage = 40;
            Item.knockBack = 6.5f;
            Item.MKItem().AltDamage = 40;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.shoot = ModContent.ProjectileType<BallofVolcano>();
            Item.shootSpeed = 10f;

            Item.MKItem().SetKatanaDefaults(Item, 60, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            foreach (int i in VolcanoBuffImmune)
            {
                player.buffImmune[i] = true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //AltFunctionUse()やAltFunctionUseItem()は条件をいじっていないためここでは動作しない。
            if (player.altFunctionUse != 0) return false;

            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.DD2_BetsysWrathShot;

            // プレイヤーの向きをマウスの方向に向けて、その方向にホールド発射体をスポーンさせる
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<VolcanoKatanaHoldUp>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 2; i++)
            {
                int moyasu = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[moyasu].noGravity = true;
                Main.dust[moyasu].velocity.X *= 2f;
                Main.dust[moyasu].velocity.Y *= 2f;
            }
        }

        public override bool CanUseItem(Player player) => Bomber = base.CanUseItem(player);

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
                target.AddBuff(BuffID.OnFire, 60 * 3);

            if (hit.Damage >= 1)
                if (player.ZoneSnow)
                    hit.Damage *= 2;

            if (Bomber && Main.myPlayer == player.whoAmI && (target == null || target.HittableForOnHitRewards()))
            {
                Vector2 center = target.Center;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), center.X, center.Y, 0f, -1f * player.gravDir, ProjectileID.Volcano, Item.damage, Item.knockBack, player.whoAmI, 0f, 2f, 0f);
                Bomber = false;
            }
        }
    }
}