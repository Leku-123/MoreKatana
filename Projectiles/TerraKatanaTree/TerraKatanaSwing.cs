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
                    SwingEllipse = new(1.1f, 0.5f);
                    Owner.ScreenShake(2, 3);
                    break;
                case 3:
                    SwingEllipse = new(1.9f, 0.9f);
                    CreateSound = false;
                    ImpactCharge = 5;
                    break;
                case 4:
                    SwingEllipse = Vector2.One;
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

                UpdateTrail(SwordTrail);
            }
        }

        public SwingData Down => new SwingData(SwordItem.useAnimation * 1.2f, 0.8f);
        public SwingData Up => new SwingData(SwordItem.useAnimation * 0.6f, 0.6f, backspin: true);
        public SwingData Down2 => new SwingData(SwordItem.useAnimation * 0.6f, 0.65f, 0.2f);
        public SwingData Down3 => new SwingData(SwordItem.useAnimation * 2.5f, 0.65f, 0.2f);
        public SwingData Thrust => new SwingData(SwordItem.useAnimation * 1.5f, 0f, 0.5f);

        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up, Down2, Down3, Thrust);

        public CurveSegment Prepare => new CurveSegment(SineOutEasing, 0f, 0f, -0.05f);
        public CurveSegment Swing => new CurveSegment(SineOutEasing, 0.4f, Prepare.EndingHeight, 1.03f);
        public CurveSegment Recovery => new CurveSegment(LinearEasing, 0.65f, Swing.EndingHeight, 0.02f);
        public float LargeSwingAnimation => PiecewiseAnimation(Progress, Prepare, Swing, Recovery);

        public CurveSegment thrust = new CurveSegment(PolyInEasing, 0f, 0.15f, 0.85f, 3);
        public CurveSegment hold = new CurveSegment(SineBumpEasing, 0.3f, 1f, 0.2f);
        public CurveSegment retract = new CurveSegment(PolyOutEasing, 0.7f, 1f, -1f, 2);
        public CurveSegment correct = new CurveSegment(PolyInEasing, 0.85f, 0f, 0.17f);
        internal float ThrustAnimation => PiecewiseAnimation(Progress, thrust, hold, retract, correct);

        public override float GetProgress(int type)
        {
            if (type == 3)
            {
                return LargeSwingAnimation;
            }
            else if (type == 4)
            {
                return ThrustAnimation;
            }
            else
                return GeneralSwingAnimation(Progress);
        }

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            GetProgress(type).InChatText();
            if (type == 3)
            {
                if (GetProgress(type) > 0f)
                {
                    if (Projectile.localAI[0] == 0)
                    {
                        Projectile.localAI[0] = 1;
                        Owner.ScreenShake(3, 15);
                        SoundEngine.PlaySound(item.MKItem().UseSound, Owner.Center);
                    }
                }
            }
            else if (type == 4)
            {
                SwingEllipse = new(0.8f + (0.5f * GetProgress(type)), 1);
                Projectile.scale = 1f + (0.5f * GetProgress(type));
            }
        }
    }
}