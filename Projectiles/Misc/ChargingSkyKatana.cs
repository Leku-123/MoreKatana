using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.Misc
{
    internal class ChargingSkyKatana : CustomSword
    {
        private float animationStoppedPoint;

        private bool impacted = false;

        public override void SafeSendExtraAI(BinaryWriter writer) => writer.Write(animationStoppedPoint);
        public override void SafeReceiveExtraAI(BinaryReader reader) => animationStoppedPoint = reader.ReadSingle();

        public override void DrawTrail(int dir, int type)
        {
            if (type == 1 && GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                bool kill = GetProgress(type) >= 0.90f || SwingStop;
                UpdateTrail(SwordTrail, kill, type: 2);
            }
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = -1;
            Projectile.MKProjectile().ActivateCD = true;

            ContinuousSwing = true;
            FixedDirection = true;
            CreateSound = false;
            GetTextureValues();
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(0.9f);

            //Note: SwingStatsのTimeを大きくすると多分振りが早くなる。
            switch (type)
            {
                case 0:
                    SwingStats(70, -0.4f, 0.5f, delay: 5f);
                    break;
                case 1:
                    SwingStats(50, 0.8f, 0.1f, delay: 30f);
                    return false;
            }
            return true;
        }

        public float UpwardAnimation => CircOutEasing(Progress, 1); // 振り上げのアニメーション

        public CurveSegment prepare = new CurveSegment(SineOutEasing, 0f, 0f, -0.1f); // 予備動作
        public CurveSegment execute = new CurveSegment(CircOutEasing, 0.2f, -0.1f, 1f); // 振り下ろし
        public float SwingAnimation => PiecewiseAnimation(Progress, prepare, execute); // 振り下ろしのアニメーション

        public float RecoilAnimationDelay => MathHelper.SmoothStep(animationStoppedPoint, animationStoppedPoint - 0.05f, DelayProgress); // タイル接触時の反動のアニメーション

        public override float GetProgress(int type) => type == 0 ? UpwardAnimation : !SwingStop ? SwingAnimation : RecoilAnimationDelay;

        public override void AdditionalAI(Item item, int type, bool onDelay)
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
                    if (GetProgress(type) > 0f && Projectile.localAI[0] == 0)
                    {
                        Projectile.localAI[0] = 1;
                        SoundEngine.PlaySound(SwordItem.MKItem().UseSound, Owner.Center);
                    }
                }
            }
            else
            {
                // 振り上げ時
                if (type == 0)
                {
                    if (Projectile.localAI[1] == 0)
                    {
                        Projectile.localAI[1] = 1;
                        SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                        DrawRing(Projectile.Center, [DustID.PlatinumCoin], 24, 4f);
                    }

                    // 発射体の位置をランダムで揺らす
                    Projectile.Center += Main.rand.NextVector2Unit();
                }
            }

            //(type == 1f && onDelay) 振ったタイミングは難しめ…

            if (SwingStop)// 振り下ろしが終了した、または何かしら衝突した時点で風を起こす（最後まで衝突しなかったら振り下ろし終了時に風を出す）
            {
                if (!impacted)
                {
                    impacted = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, new(Projectile.direction * 30f, 0f), ModContent.ProjectileType<SkyKatanaWindImpact>(), 1, 25f, Projectile.owner);
                }
            }
        }

        public override void SafeTileCollide(Item item, int type, Vector2 collisionPoint, float oldProgress)
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
                    }

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
    }
}
