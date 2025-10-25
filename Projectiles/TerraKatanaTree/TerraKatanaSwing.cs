using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraKatanaSwing : CustomSword
    {
        public override void Initialize(Item item, int type)
        {
            // 1振りで同じターゲットに2回ヒットしないようにする
            Projectile.localNPCHitCooldown = -1;

            // サイズとトレイルの色の設定
            SwordSize(66, 74);
            TrailColor = Color.LimeGreen;

            // 振りのタイプごと細かく設定することがあるのでswitch文でそれぞれを設定する
            switch (type)
            {
                case 0: // 通常の切り下げ
                    SwingEllipse = new(1.4f, 0.9f);
                    Owner.ScreenShake(2, 8);
                    break;
                case 1: // 早めの切り上げ
                case 2: // 早めの切り下げ
                    SwingEllipse = new(1f, 0.7f);
                    Owner.ScreenShake(2, 3);
                    break;
                case 3: // 大振りの切り下げ
                    SwingEllipse = new(1.9f, 0.9f);
                    CreateSound = false;
                    ImpactCharge = 5;
                    break;
                case 4: // 突き
                    SwingEllipse = new(1f);
                    NoSpeedBonus = true;
                    break;
                default:
                    break;
            }
        }

        // 全てのスイングデータを設定する
        public SwingData StandardDown => new SwingData(SwordItem.useAnimation * 1.2f, 0.8f); // 通常の切り下げ
        public SwingData FastUp => new SwingData(SwordItem.useAnimation * 0.6f, 0.6f, backspin: true); // 早めの切り上げ
        public SwingData FastDown => new SwingData(SwordItem.useAnimation * 0.8f, 0.65f, 0.2f, delay: 10f); // 早めの切り下げ
        public SwingData LargeDown => new SwingData(SwordItem.useAnimation * 2.5f, 0.65f, 0.2f); // 大振りの切り下げ
        public SwingData Thrusting => new SwingData(SwordItem.useAnimation * 1.5f, 0f, 0.5f, delay: 10f); // 突き
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, StandardDown, FastUp, FastDown, LargeDown, Thrusting);

        // 大きな振りのアニメーション
        public CurveSegment Prepare => new CurveSegment(SineOutEasing, 0f, 0f, -0.05f); // 少しの振りかぶり
        public CurveSegment Swing => new CurveSegment(SineOutEasing, 0.35f, Prepare.EndingHeight, 1.03f); // 振る
        public CurveSegment Recovery => new CurveSegment(LinearEasing, 0.65f, Swing.EndingHeight, 0.02f); // 減衰
        public float LargeSwingAnimation => PiecewiseAnimation(Progress, Prepare, Swing, Recovery);

        // 突きのアニメーション
        public CurveSegment Thrust => new CurveSegment(CircInEasing, 0f, 0.15f, 0.85f); // 突き
        public CurveSegment Hold => new CurveSegment(SineBumpEasing, 0.3f, Thrust.EndingHeight, 0.2f); // 少しのホールド
        public CurveSegment Retract => new CurveSegment(PolyOutEasing, 0.7f, Hold.EndingHeight, -1f, 2); // 手元に引き戻す
        public CurveSegment Correct => new CurveSegment(PolyInEasing, 0.85f, Retract.EndingHeight, 0.17f); // 位置の調節
        internal float ThrustAnimation => PiecewiseAnimation(Progress, Thrust, Hold, Retract, Correct);

        public override float GetProgress(int type)
        {
            if (type == 3) // 大振りの切り下げ
                return LargeSwingAnimation;
            else if (type == 4) // 突き
                return ThrustAnimation;
            else // それ以外はテンプレートのものを適用する
                return GeneralSwingAnimation(Progress);
        }

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            // プレイヤーのアイテム使用時間を延長する
            Owner.SetDummyItemTime(2);

            // 元のスイング内で処理されてはいるが、一応正規化されたものを使う
            Vector2 normalized = Vector2.Normalize(Projectile.velocity);

            switch (type)
            {
                case 0:
                    if (!Projectile.MKProj().Bool[0])
                    {
                        Projectile.MKProj().Bool[0] = true;
                        SoundEngine.PlaySound(SoundID.Item60, Owner.Center);

                        // 衝撃波を発射する
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int wave = ModContent.ProjectileType<TerraWave>();
                            float speed = 25f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, normalized * speed, wave, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        }
                    }
                    break;
                case 1:
                case 2:
                    if (!Projectile.MKProj().Bool[0])
                    {
                        Projectile.MKProj().Bool[0] = true;
                        SoundEngine.PlaySound(SoundID.Item4, Owner.Center);

                        for (int i = 0; i < 12; i++)
                        {
                            int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Terra, 0f, 0f, 100, default, 1.5f);
                            Main.dust[newDust].scale *= Main.rand.NextFloat(1, 2.5f);
                            Main.dust[newDust].noGravity = true;
                            Main.dust[newDust].velocity = normalized * 8f;
                            Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(60));
                            Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
                        }

                        if (Projectile.owner == Main.myPlayer)
                        {
                            int edge = ModContent.ProjectileType<TerraEdge>();
                            int orb = ModContent.ProjectileType<TerraOrb>();

                            int dir = type == 1 ? -1 : 1;
                            float distance = 50f;
                            float speed = 15f;

                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + normalized * distance, normalized * speed, edge, Projectile.damage, Projectile.knockBack, Projectile.owner, (int)TerraEdge.AttackType.Homing);

                            Vector2 startRot = normalized.RotatedBy(MathHelper.ToRadians(60 * dir * -Owner.direction)) * distance;
                            Vector2 velocity = normalized.RotatedBy(MathHelper.ToRadians(60 * dir * Owner.direction)) * speed;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + startRot, velocity, orb, Projectile.damage, Projectile.knockBack, Projectile.owner, dir);
                        }
                    }
                    break;
                case 3:
                    if (GetProgress(type) > 0f)
                    {
                        if (!Projectile.MKProj().Bool[0])
                        {
                            Projectile.MKProj().Bool[0] = true;
                            Owner.ScreenShake(3, 15);
                            SoundEngine.PlaySound(item.MKItem().UseSound, Owner.Center);
                        }
                    }

                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.whoAmI == Projectile.whoAmI || !proj.active || proj.owner != Projectile.owner || proj.type != ModContent.ProjectileType<TerraOrb>())
                            continue;

                        if (Projectile.Colliding(Projectile.Hitbox, proj.Hitbox))
                        {
                            Owner.velocity = Vector2.Zero;
                            if (proj.ai[2] == (float)TerraOrb.State.Holding)
                            {
                                proj.ai[2] = (float)TerraOrb.State.Firing;
                                proj.netUpdate = true;
                                ImpactChargeLaunch();
                            }
                        }
                    }
                    break;
                case 4:
                    // SwingEllipseのX値を調節することで疑似的に突きの挙動にする
                    SwingEllipse = new(0.8f + (0.5f * GetProgress(type)), 1);

                    // 突きの頂点に向かってスケールを大きくする
                    Projectile.scale = 1f + (0.5f * GetProgress(type));

                    if (GetProgress(type) > 0.4f)
                    {
                        if (!Projectile.MKProj().Bool[0])
                        {
                            Projectile.MKProj().Bool[0] = true;
                            SoundEngine.PlaySound(SoundID.Item28, Owner.Center);
                            SoundEngine.PlaySound(SoundID.Item60, Owner.Center);
                            Owner.ScreenShake(15, 5);

                            float distance = 100f;
                            float speed = 25f;
                            Vector2 center = Projectile.Center + normalized * distance;

                            if (Projectile.owner == Main.myPlayer)
                            {
                                int beam = ModContent.ProjectileType<TerraBeam>();
                                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), center, normalized * speed, beam, Projectile.damage, Projectile.knockBack, Projectile.owner);
                            }

                            for (int i = 1; i <= 3; i++)
                            {
                                for (int j = 0; j < 60; j++)
                                {
                                    Vector2 vector2 = Vector2.UnitX * -Projectile.width / 2f;
                                    vector2 += Utils.RotatedBy(Vector2.UnitY, j * Math.PI / 30f) * new Vector2(30f * i, 15f * i);
                                    vector2 = Utils.RotatedBy(vector2, normalized.ToRotation() - Math.PI / 2f) * 1.3f;
                                    int newDust = Dust.NewDust(center + vector2 + (normalized * 60 * i), 0, 0, DustID.Terra, 0f, 0f, 160, default, 2f);
                                    Main.dust[newDust].noGravity = true;
                                    Main.dust[newDust].velocity = Projectile.velocity * 0.5f;
                                    Main.dust[newDust].velocity = Vector2.Normalize(Projectile.Center - Projectile.velocity * 3f - Main.dust[newDust].position) * 1.5f;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override void DrawTrail(int type)
        {
            // 突き以外はトレイルを描画する
            if (GetProgress(type) >= 0f && Timer > 1f && type != 4)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[SwordItem.type].Value;

            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = TrailColor * Projectile.Opacity;

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = SwingDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            if (Progress != 1f)
                DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), spriteEffects | spriteEffects2);

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            // 剣先にスパークルを描画する
            Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));
            return false;
        }
    }
}