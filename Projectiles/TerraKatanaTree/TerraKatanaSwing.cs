using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraKatanaSwing : CustomSword
    {
        public override void Initialize(Item item, int type)
        {
            Projectile.localNPCHitCooldown = -1;

            switch (type)
            {
                case 0:
                    SwingEllipse = new(1.4f, 0.9f);
                    Owner.ScreenShake(2, 5);
                    break;
                case 1:
                    SwingEllipse = new(1.1f, 0.5f);
                    Owner.ScreenShake(2, 3);
                    break;
                case 2:
                    SwingEllipse = new(1.3f, 0.7f);
                    Owner.ScreenShake(2, 3);
                    break;
                case 3:
                    SwingEllipse = new(1.9f, 0.9f);
                    CreateSound = false;
                    ImpactCharge = 5;
                    break;
                case 4:
                    SwingEllipse = Vector2.One;
                    Projectile.scale = 1.5f;
                    break;
            }

            GetTextureValues();
        }

        public override void DrawTrail(int type)
        {
            if (GetProgress(type) >= 0f && Timer > 1f && type != 4)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                bool? kill = null;
                if (type == 3)
                    kill = GetProgress(type) > 0.99f;

                UpdateTrail(SwordTrail, kill);
            }
        }

        public SwingData Down => new SwingData(SwordItem.useAnimation * 1.2f, 0.8f);
        public SwingData Up => new SwingData(SwordItem.useAnimation * 0.6f, 0.6f, backspin: true);
        public SwingData Down2 => new SwingData(SwordItem.useAnimation, 0.6f, 0.2f);
        public SwingData Down3 => new SwingData(SwordItem.useAnimation * 1.5f, 0.6f, 0.2f, delay: SwordItem.useAnimation * 0.8f);
        public SwingData Thrust => new SwingData(SwordItem.useAnimation * 1.5f, 0f, 0.5f);

        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up, Down2, Down3, Thrust);

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public float NormalAnimation => PiecewiseAnimation(Progress, execute, unwind);

        public CurveSegment prepare = new CurveSegment(SineOutEasing, 0f, 0f, -0.1f);
        public CurveSegment execute2 = new CurveSegment(LinearEasing, 0.7f, -0.1f, 1.1f);
        public float NormalAnimation2 => PiecewiseAnimation(Progress, prepare, execute2);
        public float SpinAnimation2Delay => MathHelper.SmoothStep(1f, 1.02f, DelayProgress);

        public CurveSegment thrust = new CurveSegment(SineOutEasing, 0f, 0f, -0.1f);
        public CurveSegment back = new CurveSegment(SineOutEasing, 0.5f, -0.1f, 1f);
        public float ThrustAnimation => PiecewiseAnimation(Progress, thrust, back);

        public override float GetProgress(int type)
        {
            if (type == 3)
            {
                if (Progress != 1f)
                    return NormalAnimation2;
                else
                    return SpinAnimation2Delay;
            }
            else if (type == 4)
            {
                return ThrustAnimation;
            }
            else
                return NormalAnimation;
        }

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 3)
            {
                if (GetProgress(type) > 0f)
                {
                    if (Projectile.localAI[0] == 0)
                    {
                        Projectile.localAI[0] = 1;
                        Owner.ScreenShake(2, 8);
                        SoundEngine.PlaySound(item.MKItem().UseSound, Owner.Center);
                    }
                }
            }
            else if (type == 4)
            {
                SwingEllipse = new(0.8f + (0.5f * GetProgress(type)), 1);
                GetProgress(type).InChatText();
            }
        }
    }
}