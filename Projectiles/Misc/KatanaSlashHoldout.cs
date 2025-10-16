using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class KatanaSlashHoldout : ModProjectile
    {
        public const int AttackRange = 500;

        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 9999;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.MKProjectile().ActivateCD = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.noItems || player.CCed || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
            {
                Projectile.Kill();
                return;
            }

            // 発射体の基本位置
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);

            // 発射体の残り時間の延長
            Projectile.timeLeft = 2;

            // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);

            // ヨライザーの目のエフェクト
            if (player.yoraiz0rEye < 2)
                player.yoraiz0rEye = 2;

            for (int i = 0; i < 40; i++)
            {
                Vector2 offset = new Vector2();
                double angle = Main.rand.NextDouble() * 2d * Math.PI;
                offset.X += (float)(Math.Sin(angle) * AttackRange);
                offset.Y += (float)(Math.Cos(angle) * AttackRange);
                int newDust = Dust.NewDust(player.Center + offset - new Vector2(4, 4), 0, 0, DustID.GemDiamond, 0, 0, 100, default, 0.5f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity = player.velocity;
                if (Main.rand.NextBool(3))
                    Main.dust[newDust].velocity += Vector2.Normalize(offset) * -5f;
            }

            NPC target = player.Center.ClosestNPCAt(AttackRange);
            if (target != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = new Vector2();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * 10);
                    offset.Y += (float)(Math.Cos(angle) * 10);
                    int newDust = Dust.NewDust(target.Center + offset, 0, 0, DustID.GemDiamond, 0, 0, 100, default, 0.5f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity = target.velocity;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<KatanaSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}