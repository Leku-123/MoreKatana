using Microsoft.Xna.Framework;
using MoreKatana.Projectiles;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Misc
{
    public class EnchantedKatana : KatanaItem
    {
        public static int[] EnchantedDustType = [DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink];
        public static Color EnchantedDamageColor = new(150, 60, 255, 255);

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Arcane, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 54;
            Item.height = 56;

            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4.25f;
            Item.MKItem().AltDamage = 20;

            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<EnchantedKatanaBeam>();
            Item.shootSpeed = 9.5f;

            Item.MKItem().SetKatanaDefaults(Item, 60, type: ModContent.ProjectileType<EnchantedKatanaSwing>());
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.MKPlayer().enchantedHurtEffect = true;
        }

        public override void ActiveSkill(Player player)
        {
            // サウンド
            SoundEngine.PlaySound(MoreKatanaSounds.SwordSlash_2, player.Center);

            // クールダウンを有効化
            Item.MKItem().ActivateCooldown(player);

            // ダッシュ切りの発射体
            // 挙動を追加します
            int p = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.SafeDirectionTo(player.MKPlayer().MouseWorld), ModContent.ProjectileType<GeneralDashSlash>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI, 400, 16);
            GeneralDashSlash dash = (GeneralDashSlash)Main.projectile[p].ModProjectile;
            dash.action = delegate (Projectile projectile)
            {
                // 最寄りのNPC
                NPC target = projectile.Center.ClosestNPCAt(500f, false);

                // ダッシュ中に最寄りのターゲットが検出された場合、定期的に発射体を発射
                if (dash.Timer % 4 == 0 && dash.Owner.velocity.Length() > 2f
                && target != null && projectile.timeLeft > (int)dash.DashTime)
                {
                    // サウンド
                    SoundEngine.PlaySound(SoundID.Item4, player.Center);

                    // ターゲットの方向に発射体を発射
                    if (projectile.owner == Main.myPlayer)
                    {
                        Vector2 vel = projectile.SafeDirectionTo(target.Center, Vector2.UnitY) * Item.shootSpeed;
                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, vel, ModContent.ProjectileType<EnchantedKatanaBeam>(), projectile.damage / 2, 0, projectile.owner);
                    }
                }
            };
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            velocity = new Vector2(player.direction, 0) * Item.shootSpeed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!player.IsUsingAlt())
            {
                Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage / 2, 0f, player.whoAmI);
                Projectile.NewProjectile(source, player.MountedCenter, -velocity, type, damage / 2, 0f, player.whoAmI);
            }
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                int newDust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Utils.SelectRandom(Main.rand, EnchantedDustType));
                Main.dust[newDust].noGravity = true;
            }
        }
    }
}
