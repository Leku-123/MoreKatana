using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.Misc
{
    public class EnchantedKatanaSwing : CustomSword
    {
        public int flipCount;
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool SwingPattern(Item item, int type)
        {
            if (type == 0)
            {
                SwingEllipse = new(1.8f, 0.5f);
                SwingStats(Owner.itemAnimationMax, 1, 0f);
                flipCount = 1;
            }
            else if (type == 1)
            {
                SwingEllipse = new(1f);
                SwingStats(Owner.itemAnimationMax * 2, 3.25f, 0.25f);
                flipCount = 2;
            }

            return base.SwingPattern(item, type);
        }

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Owner.FlipEffect(Progress * 6 * flipCount); // フリップエフェクト

            if (type == 1)
            {
                if (Projectile.soundDelay <= 0)
                {
                    Projectile.soundDelay = 15 * Projectile.MaxUpdates;
                    SoundEngine.PlaySound(SoundID.Item1, Owner.Center);
                }
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);
    }
}
