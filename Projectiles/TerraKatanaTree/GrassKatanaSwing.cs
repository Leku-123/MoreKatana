using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassKatanaSwing : CustomSword
    {
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool SwingPattern(Item item, int type)
        {
            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            float swingTime = (type != 2) ? Owner.itemAnimationMax : Owner.itemAnimationMax * 2;
            SwingStats(swingTime, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public CurveSegment unwindMore = new CurveSegment(LinearEasing, 0.25f, 0.95f, 0.05f);
        public override float GetProgress(int type)
        {
            if (type == 2)
                return PiecewiseAnimation(Progress, execute, unwindMore);
            else
                return PiecewiseAnimation(Progress, execute, unwind);
        }

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (GetProgress(type) >= 0.95f)
                KillPrims = true;

            // 3振り目に葉を3wayで発射する
            if (type == 2 && Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Owner.ScreenShake(2, 2);
                SoundEngine.PlaySound(SoundID.Grass, Owner.Center);

                if (Projectile.owner == Main.myPlayer)
                {
                    float deg = 10; // 発射体1つごとの角度
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vector = Vector2.Normalize(Projectile.velocity).RotatedBy(MathHelper.ToRadians(deg) * i);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, vector, ProjectileID.BladeOfGrass, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
                    }
                }
            }
        }
    }
}