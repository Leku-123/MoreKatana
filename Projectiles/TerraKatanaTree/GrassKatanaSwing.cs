using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassKatanaSwing : CustomSword
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

            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            float num = Owner.itemAnimationMax / 4f;
            SwingStats(num * 3, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            DelayTimer = (type != 2) ? num : Owner.itemAnimationMax;

            return base.AttackPattern(item, type);
        }

        public override float GetProgress(int type) => EaseFunction.EaseCubicOut.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            // 3振り目に葉を3wayで発射する
            if (type == 2 && Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

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