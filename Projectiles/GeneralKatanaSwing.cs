using MoreKatana.Projectiles.Base;
using Terraria;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles
{
    public class GeneralKatanaSwing : CustomSword
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
            SwingStats(Owner.itemAnimationMax, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Owner.ScreenShake(2, 2);
            }
        }
    }
}