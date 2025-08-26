using MoreKatana.Projectiles.Base;
using Terraria;

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
            float x = Utils.SelectRandom(Main.rand, 1f, 1.3f);
            float y = Utils.SelectRandom(Main.rand, 0.7f, 0.9f);
            GetEllipse(x, y);

            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            float num = Owner.itemAnimationMax / 4f;
            SwingStats(num * 3, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            DelayTimer = num;

            return base.SwingPattern(item, type);
        }

        public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Owner.ScreenShake(2, 3);
            }
        }
    }
}