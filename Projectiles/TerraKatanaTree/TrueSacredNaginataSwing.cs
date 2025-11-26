using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TrueSacredNaginataSwing : CustomSword
    {
        public override string Texture => this.GetTexture();

        private int projCount;

        private Vector2 DirectionToProj => Utils.DirectionTo(Owner.MountedCenter, Projectile.Center);

        private CustomSwordPrimTrail SubTrail;

        public override void Initialize(int type)
        {
            SwordSize(126);

            if (type == 2) // スピン
            {
                Projectile.localNPCHitCooldown = OwnerItem.useAnimation / 3 * Projectile.MaxUpdates;
                SwingEllipse = new(0.5f);
                NoSpeedBonus = true;
                CreateSound = false;
                ImpactCharge = 4;
            }
            else  // 通常のスイング
            {
                Projectile.localNPCHitCooldown = -1;
                SwingEllipse = new(1f, 0.45f);
                NoSpeedBonus = false;
                Owner.ScreenShake(2, 3);
            }
        }

        // 全てのスイングデータを設定する
        public SwingData Down => new SwingData(OwnerItem.useAnimation / 2f, 0.6f, 0.2f); // 切り下げ
        public SwingData Up => new SwingData(OwnerItem.useAnimation, 0.6f, 0.2f, true); // 切り上げ
        public SwingData Spin => new SwingData(OwnerItem.useAnimation * 1.5f, 2.6f, 0.25f, delay: OwnerItem.useAnimation / 2f); // スピン
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up, Spin);

        // スピンのアニメーション
        public CurveSegment prepare = new CurveSegment(SineOutEasing, 0f, 0f, -0.05f);
        public CurveSegment spinning = new CurveSegment(LinearEasing, 0.2f, -0.05f, 1.05f);
        public float SpinAnimation => PiecewiseAnimation(Progress, prepare, spinning);
        public float SpinAnimationDelay => MathHelper.SmoothStep(1f, 1.01f, DelayProgress);

        public override float GetProgress(int type)
        {
            if (type == 2)
            {
                if (Progress != 1f)
                    return SpinAnimation;
                else
                    return SpinAnimationDelay;
            }
            else
                return GeneralSwingAnimation(Progress);
        }

        public override void AdditionalAI(int type, bool delay)
        {
            Owner.SetDummyItemTime(2);
            ChainPhysics();

            if (type == 2) // スピン時
            {
                if (!delay)
                {
                    // 振りかぶる時は敵にヒットしないようにする
                    Projectile.friendly = GetProgress(type) >= 0f;

                    // フリップエフェクト
                    Owner.FlipEffect(GetProgress(type) * 9f);

                    // 振りのサウンド
                    if (Projectile.soundDelay <= 0 && GetProgress(type) >= 0f)
                    {
                        Projectile.soundDelay = 15 * Projectile.MaxUpdates;
                        SoundEngine.PlaySound(OwnerItem.MKItem().UseSound, Owner.Center);
                    }

                    // スクリーンシェイクと追加のサウンド
                    if (Timer % (10 * Projectile.MaxUpdates) == 0)
                    {
                        Owner.ScreenShake(2, 3);
                        SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f }, Owner.Center);
                    }

                    // 発射体を発射する
                    if (Timer % (4 * Projectile.MaxUpdates) == 0)
                    {
                        float rad = -30 * projCount * Owner.direction; // 発射体ごとのスポーン位置の角度
                        float offset = 100f; // 発射体のスポーン位置のオフセット
                        Vector2 spawnPos = Owner.MountedCenter + (Projectile.velocity.RotatedBy((90 * -Owner.direction) + rad) * offset); // スポーン位置

                        // パーティクル
                        ParticleOrchestraSettings particleOrchestraSettings = default;
                        particleOrchestraSettings.PositionInWorld = spawnPos;
                        ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);
                       
                        if (Projectile.owner == Main.myPlayer)
                        {
                            float speed = 25f; // 発射体の速度
                            Vector2 vector = Vector2.Normalize(Owner.MKPlayer().MouseWorld - (spawnPos + Main.rand.NextVector2Unit() * 50)) * speed; // マウスの方向に向けてベロシティを調節する
                            int edgeProj = ModContent.ProjectileType<SacredEdge>();
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, vector, edgeProj, Projectile.damage / 2, 0, Projectile.owner);

                            // 発射した発射体のカウントを増やす
                            projCount++;
                        }
                    }
                }
                else
                {
                    Projectile.friendly = false;
                    Projectile.Opacity = 1 - CircOutEasing(DelayProgress, 1); // フェードアウトする
                    Owner.reuseDelay = 5;
                }
            }
            else
            {
                Owner.FlipEffect(0);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);

            for (int i = 0; i < 2; i++)
            {
                Vector2 vector = Main.rand.NextVector2Unit() * 200;
                int edgeProj = ModContent.ProjectileType<SacredEdge>();
                if (Owner.ownedProjectileCounts[edgeProj] < 6)
                {
                    if (Projectile.owner == Main.myPlayer)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + vector, -vector / 10, edgeProj, Projectile.damage / 3, 0, Projectile.owner);
                }
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);

        /// <summary>
        /// 刀身が長いためトレイルの横幅を小さくして、オフセットを先端に調節する
        /// 二種類のトレイルを重ねてスポーンさせる
        /// </summary>
        public override void DrawTrail(int type)
        {
            if (GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, Color.Gold, 30, (int)SwingTime);
                    SubTrail = new CustomSwordPrimTrail(Projectile, Color.Crimson * 0.3f, 30, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SubTrail);
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                Vector2 p = Projectile.Center - Owner.MountedCenter + (DirectionToProj * 50f);
                UpdateTrail(SwordTrail, GetProgress(type) >= 0.98f, point: p);
                UpdateTrail(SubTrail, GetProgress(type) >= 0.98f, point: p);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = Color.Gold * Projectile.Opacity;
            Color trailColor2 = Color.Crimson * Projectile.Opacity;

            if (Progress != 1f)
                DrawBackglow(texture, position, null, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());

            DrawBasicSword(texture, Projectile.Center);

            // 剣先にスパークルを描画する
            Vector2 offset = DirectionToProj * 80f;
            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset,
                glowColor * (1 - Progress), trailColor * (1 - Progress),
                0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset,
                glowColor * (1 - Progress), trailColor2 * (1 - Progress),
                0.5f, 0f, 0.1f, 0.9f, 1f, MathHelper.PiOver4, new Vector2(Projectile.scale * 1.2f), new Vector2(1f, 1f));

            // ひらひらリボン
            DrawChain(Projectile.GetAlpha(lightColor));
            return false;
        }

        private Vector2[] chainVels;
        private Vector2[] chainPoints;
        private void ChainPhysics()
        {
            int length = 12;
            if (chainVels != null)
            {
                for (int i = 0; i < chainVels.Length; i++)
                    chainVels[i] = (MathHelper.PiOver2 - (i * 0.01f)).ToRotationVector2() * 2f;
            }
            else
                chainVels = new Vector2[length];

            if (chainPoints != null)
            {
                Vector2 offset = DirectionToProj * 18f;
                chainPoints[0] = Projectile.Center + offset;

                for (int i = 1; i < chainPoints.Length; i++)
                {
                    chainPoints[i] += chainVels[i];
                    if (chainPoints[i].Distance(chainPoints[i - 1]) > 10)
                        chainPoints[i] = Vector2.Lerp(chainPoints[i], chainPoints[i - 1] + new Vector2(5, 0).RotatedBy(chainPoints[i - 1].AngleTo(chainPoints[i])), 0.4f);
                }
            }
            else
            {
                chainPoints = new Vector2[length];
                for (int i = 0; i < chainPoints.Length; i++)
                    chainPoints[i] = Projectile.Center;
            }
        }

        private void DrawChain(Color lightColor)
        {
            if (chainPoints != null)
            {
                for (int i = 0; i < chainPoints.Length - 1; i++)
                {
                    Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

                    int style = 0;
                    if (i == chainPoints.Length - 3)
                        style = 1;
                    if (i > chainPoints.Length - 3)
                        style = 2;

                    Rectangle frame = chainTex.Frame(1, 3, 0, style);
                    float rotation = chainPoints[i].AngleTo(chainPoints[i + 1]);
                    Vector2 stretch = new Vector2(0.3f + Utils.GetLerpValue(0, chainPoints.Length - 2, i, true) * 0.2f, chainPoints[i].Distance(chainPoints[i + 1]) / (frame.Height - 5));
                    Main.EntitySpriteDraw(chainTex, chainPoints[i] - Main.screenPosition, frame, lightColor.MultiplyRGBA(Color.Lerp(Color.DimGray, Color.White, (float)i / chainPoints.Length)), rotation - MathHelper.PiOver2, frame.Size() * new Vector2(0.5f, 0f), stretch, 0, 0);
                }
            }
        }
    }
}