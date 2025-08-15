using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.Wood
{
    public class ChargingWoodenSwing : CustomSword
    {
        private bool attackable;

        private CustomSwordPrimTrail trail;

        public override void DrawTrail(int dir)
        {
            if (Timer != 0f && attackable)
            {
                if (!primsCreated)
                {
                    primsCreated = true;
                    trail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(trail);
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    trail.TextureType = 2;
                    trail.Direction = Owner.direction * -dir;
                    trail.PrimCenter = Owner.MountedCenter;
                    trail.Points.Add(Projectile.Center - Owner.MountedCenter);

                    if (progress >= 0.98f)
                        trail?.OnDestroy();
                }
            }
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
            Projectile.MKProjectile().ActivateCD = true;

            ContinuousSwing = true;
            FixedDirection = true;
            GetTextureValues(this, item);

            if (type == 1)
                SoundEngine.PlaySound(SoundID.Item1, Owner.Center);
        }

        public override bool AttackPattern(Item item, int type)
        {
            GetEllipse(0.9f, 0.9f);

            switch (type)
            {
                case 0:
                    SwingStats(60, -0.4f, 0.5f);
                    DelayTimer = 10;
                    break;
                case 1:
                    SwingStats(30, 0.7f, 0.2f);
                    DelayTimer = 30;
                    return false;
            }
            return true;
        }

        public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            // プレイヤーのアイテム使用時間と発射体が消滅するまでの時間を延長する
            Owner.SetDummyItemTime(2);
            Projectile.timeLeft = 2;

            if (!onDelay)
            {
                // 振り下ろす時にダメージを与える
                Projectile.friendly = type == 1;
            }
            else
            {
                // ディレイではダメージを与えない
                Projectile.friendly = false;

                // 振り上げ時
                if (type == 0)
                {
                    // 攻撃可能なことを音とダストで知らせる
                    if (!attackable)
                    {
                        attackable = true;
                        SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                        MoreKatanaUtil.DrawRing(Projectile.Center, [DustID.PlatinumCoin], 24, 4f);
                    }

                    // マウスを右クリックしている場合はディレイを延長する
                    if (Projectile.owner == Main.myPlayer && Main.mouseRight)
                        DelayTimer = 10;

                    // 発射体の位置をランダムで揺らす
                    Projectile.Center += Main.rand.NextVector2Unit();
                }
            }
        }

        public override void SafeTileCollide(Item item, int type)
        {
            // 振り下ろし時
            if (type == 1)
            {
                timerStop = true; // タイマーをストップさせ振りの動きを止める
                Owner.ScreenShake(4, 10);
                SoundEngine.PlaySound(SoundID.Dig, Owner.Center);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
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
                int newDust = Dust.NewDust(target.position, target.width, target.height, DustID.Smoke, 0.0f, 0f, 150, default, 0.5f);
                Main.dust[newDust].fadeIn = 1.25f;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-2, -1));
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
            }
        }
    }
}