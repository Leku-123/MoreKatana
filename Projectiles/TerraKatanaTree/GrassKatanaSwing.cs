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
        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = -1;

            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            GetTextureValues();
        }

        public override SwingData GetSwingData(int type)
        {
            float swingTime = OwnerItem.useAnimation;
            if (type == 2)
                swingTime *= 2;
            return new SwingData(swingTime, Main.rand.NextFloat(0.7f, 0.8f), backspin: type % 2 != 0);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f); // 振りのアニメーション
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.25f, 0.95f, 0.05f); // 減衰のアニメーション
        public override float GetProgress(int type)
        {
            if (type == 2)
                return PiecewiseAnimation(Progress, execute, unwind);
            else
                return GeneralSwingAnimation(Progress);
        }

        public override void AdditionalAI(int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

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
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, vector, ProjectileID.BladeOfGrass, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
                    }
                }
            }
        }
    }
}