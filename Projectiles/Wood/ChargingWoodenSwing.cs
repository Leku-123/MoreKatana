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
<<<<<<< Updated upstream
            Projectile.localNPCHitCooldown = 20 * Projectile.MaxUpdates;
=======
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
            Projectile.MKProjectile().ActivateCD = true;
>>>>>>> Stashed changes
            continuousSwing = true;
            fixedDirection = true;
            GetTextureValues(this, item);

            if (type == 1)
<<<<<<< Updated upstream
                Projectile.tileCollide = true;
=======
            {
                Projectile.tileCollide = true;
                SoundEngine.PlaySound(SoundID.Item1, Owner.Center);
            }
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
            if (collision)
                Projectile.tileCollide = false;

=======
>>>>>>> Stashed changes
            if (!onDelay)
            {
                Projectile.friendly = type == 1;
            }
            else
            {
<<<<<<< Updated upstream
                if (type == 0)
                {
=======
                Projectile.friendly = false;

                if (type == 0)
                {
                    Projectile.Center += Main.rand.NextVector2Unit();

>>>>>>> Stashed changes
                    if (!attackable)
                    {
                        attackable = true;
                        SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);
                    }

                    if (Projectile.owner == Main.myPlayer && Main.mouseRight)
                        DelayTimer = 10;
<<<<<<< Updated upstream

                    Projectile.friendly = false;
                    Projectile.Center += Main.rand.NextVector2Unit() * 2f;
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                SoundEngine.PlaySound(SoundID.NPCHit42, Owner.Center);
=======
                SoundEngine.PlaySound(SoundID.Dig, Owner.Center);
>>>>>>> Stashed changes
            }
            return false;
        }

<<<<<<< Updated upstream
        public override void OnKill(int timeLeft)
        {
            //Projectile.OverhaulProjectile().ActivateCD = true;
=======
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
            for (int i = 0; i < 3; i++)
            {
                int newDust = Dust.NewDust(target.position, target.width, target.height, DustID.Smoke, 0.0f, 0f, 150, new Color(), 0.5f);
                Main.dust[newDust].fadeIn = 1.25f;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = new Vector2(0f, Main.rand.Next(-2, -1));
                Main.dust[newDust].velocity = Main.dust[newDust].velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(0.5f, 2f);
            }
>>>>>>> Stashed changes
        }
    }
}