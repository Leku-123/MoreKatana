using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class KatanaSlashHoldout : ModProjectile
    {
        public const int AttackRange = 500;
        public const float LifeTime = 60;

        private bool slash;

        private NPC target;

        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 9999;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProj().ActivateCD = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(slash);
        public override void ReceiveExtraAI(BinaryReader reader) => slash = reader.ReadBoolean();

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            // 右クリックを離したら攻撃する
            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
                slash = true;

            if (!slash) // ホールド時
            {
                // 発射体の残り時間の延長
                Projectile.timeLeft = (int)LifeTime;

                // 攻撃範囲に円形のダストをスポーンさせる
                for (int i = 0; i < 40; i++)
                {
                    Vector2 offset = new Vector2();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * AttackRange);
                    offset.Y += (float)(Math.Cos(angle) * AttackRange);
                    int newDust = Dust.NewDust(Owner.Center + offset - new Vector2(4, 4), 0, 0, DustID.GemDiamond, 0, 0, 100, default, 0.5f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity = Owner.velocity;
                    if (Main.rand.NextBool(3))
                        Main.dust[newDust].velocity += Vector2.Normalize(offset) * -5f;
                }

                // プレイヤーの足元からダストをスポーンさせる
                if (Owner.velocity.Y == 0 && !Owner.mount.Active)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        int newDust = Dust.NewDust(new Vector2(Owner.Center.X - Owner.width, Owner.Center.Y + Owner.height / 2), Owner.width * 2 - 3, 0, DustID.GemDiamond, 0, Main.rand.Next(-5, -2), 150, default, 0.2f);
                        Main.dust[newDust].fadeIn = 0.3f;
                        Main.dust[newDust].noGravity = true;
                    }
                }

                // ターゲットの取得はプレイヤーからAttackRange内でマウスとの距離が一番近いNPCとなる
                target = MoreKatanaUtil.ClosestNPCfromTwoPoints(Owner.Center, Owner.MKPlayer().MouseWorld, AttackRange);
                if (target != null)
                {
                    // ターゲットに目印となるダストをスポーンさせる
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
            else // 攻撃時
            {
                if (target != null)
                {
                    if (Projectile.timeLeft == (int)LifeTime - 1)
                    {
                        float speed = 15f;
                        Vector2 vector = Projectile.SafeDirectionTo(target.Center, Vector2.UnitY);

                        for (int i = 0; i < 12; i++)
                        {
                            int newDust = Dust.NewDust(Owner.MountedCenter, 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                            Main.dust[newDust].velocity -= Vector2.Normalize(vector) * 2f;
                            Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                            Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                        }

                        if (Projectile.owner == Main.myPlayer)
                        {
                            Owner.ScreenShake(10, 15);
                            SoundEngine.PlaySound(MoreKatanaSounds.SlashEffect, Owner.position);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector * speed, ModContent.ProjectileType<KatanaSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        }

                        Projectile.netUpdate = true;
                    }

                    // KatanaSlashがスポーンしている場合は不透明度を0に調節してプレイヤー同様に見えなくさせる
                    float opacity = 1 - MoreKatanaUtil.CircInEasing((float)(1 - Projectile.timeLeft / LifeTime), 1);
                    if (Owner.ownedProjectileCounts[ModContent.ProjectileType<KatanaSlash>()] == 0)
                        Projectile.Opacity = opacity;
                    else
                        Projectile.Opacity = 0;
                }
                else // ターゲットがいない場合、そのまま発射体を消滅させる
                {
                    MoreKatanaUtil.DrawRing(Owner.Center - new Vector2(27 * Projectile.direction, -17), DustID.GemDiamond, 24, 2.5f);
                    Projectile.Kill();
                    return;
                }
            }

            // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);

            // アイテムローテーションと腕の回転を設定する
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

            // 発射体の回転をプレイヤーの動きによって若干揺れるようにする
            float xOffset = Owner.velocity.X * 0.04f * Owner.direction;
            float yOffset = Owner.velocity.Y * 0.02f * (Owner.direction == 1 ? -1f : 1f) * Owner.direction;
            float desiredArmAngle = (0.3f - MathHelper.PiOver2 + xOffset + yOffset) * Projectile.direction;
            Projectile.rotation = Projectile.rotation.AngleLerp(desiredArmAngle, 0.2f);

            // 発射体の位置
            Projectile.Center = Owner.Center + new Vector2(20 * Projectile.direction, 0);

            // ヨライザーの目のエフェクト
            if (Owner.yoraiz0rEye < 2)
                Owner.yoraiz0rEye = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[ItemID.Katana].Value;
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation);
            armPosition -= Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            float rotation = Owner.direction == 1 ? (float)Math.PI - 1.1f : 1.1f;
            Vector2 origin = Owner.direction == 1 ? new Vector2(8, 8) : new Vector2(8, texture.Height - 8);
            SpriteEffects spriteEffects = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // バックグロー
            // 攻撃時は描画しない
            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 2f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f);
                if (!slash)
                    Main.EntitySpriteDraw(texture, armPosition + backglowOffset, null, Color.White with { A = 0 }, rotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            // 本体の描画
            Main.EntitySpriteDraw(texture, armPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, spriteEffects, 0f);

            // 剣先の光の描画
            // 攻撃時は描画しない
            Texture2D starTex = MoreKatanaTextures.StarSparkleTexture.Value;
            Color color = Color.White * 0.3f;
            float rot = Main.GlobalTimeWrappedHourly;
            if (!slash)
            {
                Main.spriteBatch.Draw(starTex, armPosition - new Vector2(50 * Owner.direction, -23), null, color with { A = 0 }, rot, starTex.Size() / 2f, 0.15f, 0, 0);
                Main.spriteBatch.Draw(starTex, armPosition - new Vector2(50 * Owner.direction, -23), null, color with { A = 0 }, -rot, starTex.Size() / 2f, 0.25f, 0, 0);
            }

            return false;
        }
    }
}