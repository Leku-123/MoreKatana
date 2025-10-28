using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraDestructionHoldout : ModProjectile
    {
        public const int AttackRange = 400;

        private bool aiming = true;

        private NPC target;

        private Vector2 teleportPos;

        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProj().SourceIsItemUse = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
                aiming = false;

            if (aiming) // ホールド時
            {
                // 発射体の回転をプレイヤーの動きによって若干揺れるようにする
                float xOffset = Owner.velocity.X * 0.04f * Owner.direction;
                float yOffset = Owner.velocity.Y * 0.02f * (Owner.direction == 1 ? -1f : 1f) * Owner.direction;
                float desiredArmAngle = (0.3f - MathHelper.PiOver2 + xOffset + yOffset) * Projectile.direction;
                Projectile.rotation = Projectile.rotation.AngleLerp(desiredArmAngle, 0.2f);

                // 発射体の位置
                Projectile.Center = Owner.Center + new Vector2(20 * Projectile.direction, 0);

                // 発射体の残り時間の延長
                Projectile.timeLeft = 2;

                // プレイヤーの保持する発射体のIDを更新して、プレイヤーの使用時間を延長する
                Owner.heldProj = Projectile.whoAmI;
                Owner.SetDummyItemTime(2);

                // アイテムローテーションと腕の回転を設定する
                Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
                Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation);

                // ヨライザーの目のエフェクト
                if (Owner.yoraiz0rEye < 2)
                    Owner.yoraiz0rEye = 2;

                // ターゲットのNPCを取得
                target = Owner.MKPlayer().MouseWorld.ClosestNPCAt(AttackRange);

                // テレポート位置のチェック
                TeleportCheck(target);

                // テレポートする位置からAttackRangeの範囲のダストをスポーンさせる
                for (int i = 0; i < 20; i++)
                {
                    Vector2 offset = new Vector2();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * AttackRange);
                    offset.Y += (float)(Math.Cos(angle) * AttackRange);
                    int newDust = Dust.NewDust(teleportPos + offset, 0, 0, TerraKatana.DustType, 0, 0, 100, default, 0.5f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].velocity = Owner.velocity;
                }

                // プレイヤーの足元からダストをスポーンさせる
                if (Owner.velocity.Y == 0 && !Owner.mount.Active)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        int newDust = Dust.NewDust(new Vector2(Owner.Center.X - Owner.width, Owner.Center.Y + Owner.height / 2), Owner.width * 2 - 3, 0, TerraKatana.DustType, 0, Main.rand.Next(-5, -2), 150, default, 0.5f);
                        Main.dust[newDust].fadeIn = 0.3f;
                        Main.dust[newDust].noGravity = true;
                    }
                }
            }
            else // 攻撃時
            {
                // テレポート位置とは逆方向にImpactEffectの演出をする
                Vector2 vector = Owner.SafeDirectionTo(teleportPos, Vector2.UnitY);
                Owner.CreateImpactEffect(Projectile.GetSource_FromThis(), Owner.Center, -vector, Projectile.owner, 1f, TerraKatana.TerraColor[0]);

                // めり込み防止の為にテレポート位置がプレイヤーより下にある場合はY位置を調節する
                bool lookingDownOn = false;
                if (teleportPos.Y > Owner.position.Y)
                    lookingDownOn = true;
                if (lookingDownOn)
                    teleportPos.Y -= Owner.height;

                // テレポートを実行
                Owner.Teleport(teleportPos, -1);
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Owner.whoAmI, teleportPos.X, teleportPos.Y, 1);

                // 攻撃のベースとなる発射体を発射
                if (Projectile.owner == Main.myPlayer)
                {
                    Owner.ScreenShake(10, 15);

                    int destructionBase = ModContent.ProjectileType<TerraDestructionBase>();
                    int ai0 = target != null && !Main.mouseLeft ? target.whoAmI : -1; // 引数にターゲットのインデックスを入れて、ベースの発射体内でターゲットの位置を更新できるようにする
                    Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.ActiveItem()), Owner.Center, Vector2.Zero, destructionBase, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0);
                }

                Projectile.Kill();
                return;
            }
        }

        private void TeleportCheck(NPC npc)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                // 基本的なテレポートの位置はマウスの位置かNPCの位置になる
                Vector2 v = Owner.MKPlayer().MouseWorld;
                if (npc != null && !Main.mouseLeft)
                    v = npc.Center;

                // テレポートの位置を保存
                Vector2 originalPos = v;
                originalPos = new Vector2(originalPos.X, originalPos.Y);

                teleportPos = originalPos;

                // テレポートの位置にタイルがあった場合
                if (Collision.SolidCollision(teleportPos, npc?.width / 2 ?? Owner.width, npc?.height / 2 ?? Owner.height))
                {
                    // プレイヤーからテレポートの位置までの最短タイル接触ポイントを計算する
                    Vector2 collisionpoint = MoreKatanaUtil.CollisionPoint(Owner.Center, v);
                    int x = (int)collisionpoint.X / 16;
                    int y = (int)collisionpoint.Y / 16;

                    // テレポート位置をワールド座標に変換してハーフブロックも対応する
                    Vector2 collisionPos = new Vector2(x, y).ToWorldCoordinates();
                    if (Framing.GetTileSafely(x, y + 1).IsHalfBlock)
                        collisionPos.Y += 8;

                    // テレポート位置を更新
                    originalPos = new Vector2(collisionPos.X, collisionPos.Y);
                }

                // 実際にテレポートが可能な位置なのかを確認し、テレポート位置を更新する
                if (originalPos.X > 50 && originalPos.X < (double)(Main.maxTilesX * 16 - 50) && originalPos.Y > 50 && originalPos.Y < (double)(Main.maxTilesY * 16 - 50))
                    teleportPos = originalPos;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[Owner.ActiveItem().type].Value;
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation);
            armPosition -= Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            float rotation = Owner.direction == 1 ? (float)Math.PI - 1.1f : 1.1f;
            Vector2 origin = Owner.direction == 1 ? new Vector2(8, 8) : new Vector2(8, texture.Height - 8);
            SpriteEffects spriteEffects = Owner.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // バックグロー
            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 2f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f);
                Main.EntitySpriteDraw(texture, armPosition + backglowOffset, null, Color.White with { A = 0 }, rotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            // 本体の描画
            Main.EntitySpriteDraw(texture, armPosition, null, Color.White, rotation, origin, Projectile.scale, spriteEffects, 0f);

            // 一定時間ごとに光るような演出を重ねる
            float progress = MoreKatanaUtil.SineInEasing(Main.GlobalTimeWrappedHourly % 2 / 2, 1);
            Main.EntitySpriteDraw(texture, armPosition, null, Color.White with { A = (byte)(255 * progress) }, rotation, origin, Projectile.scale, spriteEffects, 0f);

            // 剣先の光の描画
            Texture2D starTex = MoreKatanaTextures.StarSparkleTexture.Value;
            Color color = TerraKatana.TerraColor[0];
            float rot = Main.GlobalTimeWrappedHourly;
            Main.spriteBatch.Draw(starTex, armPosition - new Vector2(50 * Owner.direction, -23), null, color with { A = 0 }, rot, starTex.Size() / 2f, Projectile.scale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(starTex, armPosition - new Vector2(50 * Owner.direction, -23), null, color with { A = 0 }, -rot, starTex.Size() / 2f, Projectile.scale * 0.25f, SpriteEffects.None, 0f);

            // テレポート位置の描画
            Texture2D hitMark = ModContent.Request<Texture2D>(this.GetTexture("TerraHitMark")).Value;
            float num11 = (float)(Main.GlobalTimeWrappedHourly % 0.5 / 0.5);
            float num12 = num11;
            if (num12 > 0.5) num12 = 1f - num11;
            if (num12 < 0.0) num12 = 0.0f;
            float num15 = 1f + num11 * 0.75f;
            Main.spriteBatch.Draw(hitMark, teleportPos - Main.screenPosition, null, Color.White * num12, 0f, hitMark.Size() / 2, Projectile.scale * num15, SpriteEffects.None, 0f);

            return false;
        }
    }
}