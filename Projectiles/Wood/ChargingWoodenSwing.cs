using Microsoft.Xna.Framework;
using MoreKatana.Particles;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.Wood
{
    public class ChargingWoodenSwing : CustomSword
    {
        private float animationStoppedPoint;

        public override void SafeSendExtraAI(BinaryWriter writer) => writer.Write(animationStoppedPoint);
        public override void SafeReceiveExtraAI(BinaryReader reader) => animationStoppedPoint = reader.ReadSingle();

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.MKProj().ActivateCD = true;

            SwingEllipse = new(0.9f);
            ContinuousSwing = true; // 設定した全てのスイングを連続で行う
            FixedDirection = true; // プレイヤーと発射体の方向を固定する
            CreateSound = false; // デフォルトのサウンドを鳴らさない

            GetTextureValues();
        }

        // 全てのスイングデータを設定する
        // 設定したスイング以降は空にして発射体を消す
        public SwingData Upward => new SwingData(60, -0.4f, 0.5f, delay: 5f); // 振り上げ
        public SwingData Down => new SwingData(40, 0.8f, 0.1f, delay: 30f); // 振り下げ
        public override SwingData GetSwingData(int type) => type < 2 ? SwingData.SwingRegister(type, Upward, Down) : new SwingData();

        // 振り上げのアニメーション
        public float UpwardAnimation => CircOutEasing(Progress, 1);

        // 振り下ろしのアニメーション
        public CurveSegment prepare = new CurveSegment(SineOutEasing, 0f, 0f, -0.1f); // 予備動作
        public CurveSegment execute = new CurveSegment(CircOutEasing, 0.2f, -0.1f, 1f); // 振り下ろし
        public float SwingAnimation => PiecewiseAnimation(Progress, prepare, execute);

        // タイル接触時の反動のアニメーション
        public float RecoilAnimationDelay => MathHelper.SmoothStep(animationStoppedPoint, animationStoppedPoint - 0.05f, DelayProgress);

        public override float GetProgress(int type)
        {
            if (type == 0) // 振り上げ
                return UpwardAnimation;
            else if (!SwingStop) // 振り下ろし
                return SwingAnimation;
            else // 反動
                return RecoilAnimationDelay;
        }

        public override void AdditionalAI(int type, bool onDelay)
        {
            // プレイヤーのアイテム使用時間と発射体が消滅するまでの時間を延長する
            Owner.SetDummyItemTime(2);
            Projectile.timeLeft = 2;

            // 振り下ろし時以外はダメージを与えないようにする
            Projectile.friendly = false;

            if (!onDelay)
            {
                // 振り下ろし時
                if (type == 1)
                {
                    Projectile.friendly = true;

                    // サウンド
                    if (GetProgress(type) > 0f && !Projectile.MKProj().Bool[0])
                    {
                        Projectile.MKProj().Bool[0] =true;
                        SoundEngine.PlaySound(OwnerItem.MKItem().UseSound, Owner.Center);
                    }
                }
            }
            else
            {
                // 振り上げ時
                if (type == 0)
                {
                    if (!Projectile.MKProj().Bool[1])
                    {
                        Projectile.MKProj().Bool[1] = true;
                        SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                        DrawRing(Projectile.Center, [DustID.PlatinumCoin], 24, 4f);
                    }

                    if (DelayTimer > 1f)
                    {
                        // マウスを右クリックしている場合はディレイを延長する
                        // DelayTimerを更新し続けることでディレイを進ませない
                        if (Projectile.owner == Main.myPlayer && Main.mouseRight)
                        {
                            DelayTimer = 2f;
                            Projectile.netUpdate = true;
                        }
                    }

                    // 発射体の位置をランダムで揺らす
                    Projectile.Center += Main.rand.NextVector2Unit();
                }
            }
        }

        public override void SafeTileCollide(int type, Vector2 collisionPoint, float oldProgress)
        {
            // 振り下ろし時
            if (type == 1)
            {
                // 予備動作が終わっているかどうか確認する
                if (GetProgress(type) > 0f)
                {
                    // 現在の振りのポイントを取得
                    animationStoppedPoint = oldProgress;

                    // 振りを終了し、ディレイ(反動)へ
                    if (!SwingStop)
                    {
                        SwingStop = true;
                        Owner.ScreenShake(4, 10);
                        SoundEngine.PlaySound(SoundID.Dig, Owner.Center);

                        // パーティクル
                        ParticleHandler.SpawnParticle(new ImpactEffect(collisionPoint, -Vector2.UnitY, Color.White, new Vector2(0.3f), 10));
                    }

                    // トレイルを消す
                    KillPrims = true;

                    Projectile.netUpdate = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            for (int i = 0; i < 5; i++)
            {
                int newDust = Dust.NewDust(target.position, target.width, target.height, DustID.Torch);
                Main.dust[newDust].scale = Main.rand.NextFloat(0.9f, 1.75f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity.Y = -1f;
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(10));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(1f, 3f);
            }
            for (int i = 0; i < 6; i++)
            {
                int newDust = Dust.NewDust(target.position, target.width, target.height, DustID.Smoke, 0f, 0f, 150, default, 0.5f);
                Main.dust[newDust].fadeIn = 1.25f;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-2, -1));
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
            }
        }

        public override void DrawTrail(int type)
        {
            // 振り下ろし時の予備動作後からトレイルがスポーンする
            if (type == 1 && GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail, type: 2);
            }
        }
    }
}