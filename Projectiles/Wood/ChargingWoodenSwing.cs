using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.Wood
{
    public class ChargingWoodenSwing : CustomSword
    {
        private bool attackable;
        private bool collision;

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = 20 * Projectile.MaxUpdates;
            continuousSwing = true;
            fixedDirection = true;
            GetTextureValues(this, item);

            if (type == 1)
                Projectile.tileCollide = true;
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
            Owner.SetDummyItemTime(2);

            if (collision)
                Projectile.tileCollide = false;

            if (!onDelay)
            {
                Projectile.friendly = type == 1;
            }
            else
            {
                if (type == 0)
                {
                    if (!attackable)
                    {
                        attackable = true;
                        SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                    }

                    if (Projectile.owner == Main.myPlayer && Main.mouseRight)
                        DelayTimer = 10;

                    Projectile.friendly = false;
                    Projectile.Center += Main.rand.NextVector2Unit() * 2f;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            base.OnTileCollide(oldVelocity);

            if (!collision)
            {
                collision = true;
                Owner.ScreenShake(4, 10);
                SoundEngine.PlaySound(SoundID.NPCHit42, Owner.Center);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            //Projectile.OverhaulProjectile().ActivateCD = true;
        }
    }
}