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
            Item.MKItem().AltDamage = 50;

            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<EnchantedKatanaBeam>();
            Item.shootSpeed = 9.5f;

            Item.MKItem().SetKatanaDefaults(Item, 60, combo: 2);
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
            int dashTime = 16;
            int p = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, player.SafeDirectionTo(player.MKPlayer().MouseWorld), ModContent.ProjectileType<GeneralDashSlash>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI, 400, dashTime);
            GeneralDashSlash dash = (GeneralDashSlash)Main.projectile[p].ModProjectile;
            dash.action = delegate (Projectile projectile)
            {
                projectile.alpha = 255;

                Player player = dash.Owner;
                if (dash.Timer == 0)
                {
                    player.UpdateRotation(2, player.direction, dashTime);

                    if (projectile.owner == Main.myPlayer)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            float deg = 8;
                            Vector2 velocity = projectile.velocity.Normalized() * Item.shootSpeed * 2f;
                            Vector2 vector = velocity.RotatedBy(MathHelper.ToRadians(deg) * i);
                            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, vector, Item.shoot, projectile.damage, 0, projectile.owner);
                        }
                    }
                }

                if (projectile.timeLeft == dashTime)
                {
                    if (projectile.owner == Main.myPlayer)
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, projectile.velocity.Normalized(), ModContent.ProjectileType<GeneralKatanaSwing>(), projectile.damage, projectile.knockBack, projectile.owner);
                }
            };
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => Item.MKItem().AttackType == 0 && !player.IsUsingAlt();

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