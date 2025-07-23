using MoreKatana.Projectiles.Base;
using Terraria;

namespace MoreKatana.Projectiles
{
    public class GlobalSword : CustomSword
    {
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool AttackPattern(Item item, int type)
        {
            float x = Utils.SelectRandom(Main.rand, 1f, 1.3f);
            float y = Utils.SelectRandom(Main.rand, 0.7f, 0.9f);
            GetEllipse(x, y);

            float swingRange = Main.rand.NextFloat(0.6f, 0.7f);
            float num = Owner.itemAnimationMax / 3f;
            SwingStats(num * 2, swingRange, (1f - swingRange) / 2f, type % 2 != 0);

            DelayTimer = num;

            return base.AttackPattern(item, type);
        }

        public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool delay) => Owner.SetDummyItemTime(2);
    }
}