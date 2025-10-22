using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.Misc
{
    public class EnchantedKatanaSwing : CustomSword
    {
        public int flipCount;

        public override void Initialize(Item item, int type)
        {
            Projectile.localNPCHitCooldown = -1;
            GetTextureValues();
            TrailColor = Color.DeepSkyBlue;

            if (type == 0)
            {
                SwingEllipse = new(1.8f, 0.5f);
                flipCount = 1;
            }
            else if (type == 1)
            {
                SwingEllipse = new(1f);
                flipCount = 2;
                CreateSound = false;
            }
        }

        public SwingData Normal => new SwingData(SwordItem.useAnimation, 1, 0f);
        public SwingData Special => new SwingData(SwordItem.useAnimation * 2, 3.25f, 0.25f);
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Normal, Special);

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Owner.FlipEffect(GetProgress(type) * 6 * flipCount); // フリップエフェクト

            if (type == 1)
            {
                if (Projectile.soundDelay <= 0)
                {
                    Projectile.soundDelay = (int)(SwordItem.useAnimation / 1.5f + 1) * Projectile.MaxUpdates;
                    SoundEngine.PlaySound(SoundID.Item1, Owner.position);
                }
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);

        public override void DrawTrail(int type)
        {
            if (GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail, SwingStop, type: 0);
            }
        }
    }
}
