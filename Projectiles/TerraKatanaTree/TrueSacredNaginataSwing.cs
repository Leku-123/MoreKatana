using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TrueSacredNaginataSwing : CustomSword
    {
        public override string Texture => this.GetTexture(Name);

        private int projCount;

        private Vector2 DirectionToProj => Utils.DirectionTo(Owner.MountedCenter, Projectile.Center);

        private CustomSwordPrimTrail SubTrail;

        /// <summary>
        /// 刀身が長いためトレイルの横幅を小さくして、オフセットを先端に調節する
        /// 二種類のトレイルを重ねてスポーンさせる
        /// </summary>
        /// <param name="dir"></param>
        public override void DrawTrail(int type)
        {
            if (GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, Color.Gold * 0.3f, 20, (int)(SwingTime * 1.5f));
                    SubTrail = new CustomSwordPrimTrail(Projectile, Color.Crimson * 0.3f, 20, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                    MoreKatana.primitives.CreateTrail(SubTrail);
                }

                Vector2 p = Projectile.Center - Owner.MountedCenter;
                UpdateTrail(SwordTrail, GetProgress(type) >= 0.98f, point: p + (DirectionToProj * 50f));
                UpdateTrail(SubTrail, GetProgress(type) >= 0.98f, point: p + (DirectionToProj * 60f));
            }
        }

        public override void Initialize(Item item, int type)
        {
            if (type == 2)
            {
                Projectile.localNPCHitCooldown = item.useAnimation / 3 * Projectile.MaxUpdates;
                NoSpeedBonus = true;
                SwingEllipse = new(0.5f);
                ImpactCharge = 4;
                CreateSound = false;
            }
            else
            {
                Projectile.localNPCHitCooldown = -1;
                NoSpeedBonus = false;
                SwingEllipse = new(1f, 0.45f);
            }

            SwordSize(126);
            TrailColor = Color.Gold * 0.3f;
        }

        public SwingData Down => new SwingData(SwordItem.useAnimation / 2f, 0.6f, 0.2f); // 1振り目
        public SwingData Up => new SwingData(SwordItem.useAnimation, 0.6f, 0.2f, true); // 2振り目
        public SwingData Spin => new SwingData(SwordItem.useAnimation * 1.5f, 2.6f, 0.25f, delay: SwordItem.useAnimation / 2f); // 3振り目
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up, Spin);

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public float NormalAnimation => PiecewiseAnimation(Progress, execute, unwind); // 1,2振り目のアニメーション

        public CurveSegment prepare = new CurveSegment(SineOutEasing, 0f, 0f, -0.05f);
        public CurveSegment spinning = new CurveSegment(LinearEasing, 0.2f, -0.05f, 1.05f);
        public float SpinAnimation => PiecewiseAnimation(Progress, prepare, spinning); // 3振り目のアニメーション
        public float SpinAnimationDelay => MathHelper.SmoothStep(1f, 1.01f, DelayProgress); // 3振り目のディレイ

        public override float GetProgress(int type)
        {
            if (type == 2)
            {
                if (Progress != 1f)
                    return SpinAnimation;
                else
                    return SpinAnimationDelay;
            }
            else
                return NormalAnimation;
        }

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);
            ChainPhysics();

            if (type == 2)
            {
                if (!delay)
                {
                    Projectile.friendly = GetProgress(type) >= 0f;
                    Owner.FlipEffect(GetProgress(type) * 9f);

                    if (Timer % (20 * Projectile.MaxUpdates) == 0)
                        SoundEngine.PlaySound(SoundID.Item169, Owner.Center);

                    if (Timer % (10 * Projectile.MaxUpdates) == 0)
                        SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f }, Owner.Center);

                    if (Timer % (4 * Projectile.MaxUpdates) == 0)
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            float rad = -30 * projCount * Owner.direction;
                            float offset = 100f;
                            Vector2 vector = -Vector2.UnitY.RotatedBy(rad) * offset;

                            ParticleOrchestraSettings particleOrchestraSettings = default;
                            particleOrchestraSettings.PositionInWorld = Owner.MountedCenter + vector;
                            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);

                            Vector2 vel = Vector2.Normalize(Main.MouseWorld - Owner.MountedCenter) * 25f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + vector, vel, ModContent.ProjectileType<SacredEdge>(), Projectile.damage / 2, 0, Projectile.owner);
                            projCount++;
                        }
                    }
                }
                else
                {
                    Projectile.friendly = false;
                    Projectile.Opacity = 1 - CircOutEasing(DelayProgress, 1);
                    Owner.reuseDelay = 5;
                }
            }
            else
            {
                Owner.FlipEffect(0);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);

            SoundEngine.PlaySound(MoreKatanaSounds.SlashHit, Owner.Center);

            for (int i = 0; i < 2; i++)
            {
                Vector2 vector = Main.rand.NextVector2Unit() * 200;
                int edgeProj = ModContent.ProjectileType<SacredEdge>();
                if (Owner.ownedProjectileCounts[edgeProj] < 6)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + vector, -vector / 10, edgeProj, Projectile.damage / 3, 0, Projectile.owner);
                    }
                }
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = TrailColor * Projectile.Opacity;

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = SwingDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            if (Progress != 1f)
                DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), spriteEffects | spriteEffects2);

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            // 剣先にスパークルを描画する
            Vector2 offset = DirectionToProj * 80f;
            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

            DrawChain(color);
            return false;
        }

        private Vector2[] chainVels;
        private Vector2[] chainPoints;

        public void ChainPhysics()
        {
            int length = 12;
            if (chainVels != null)
            {
                for (int i = 0; i < chainVels.Length; i++)
                {
                    chainVels[i] = (MathHelper.PiOver2 - (i * 0.01f)).ToRotationVector2() * 2f;
                }
            }
            else
                chainVels = new Vector2[length];

            if (chainPoints != null)
            {
                Vector2 offset = DirectionToProj * 18f;
                chainPoints[0] = Projectile.Center + offset;

                for (int i = 1; i < chainPoints.Length; i++)
                {
                    chainPoints[i] += chainVels[i];
                    if (chainPoints[i].Distance(chainPoints[i - 1]) > 10)
                        chainPoints[i] = Vector2.Lerp(chainPoints[i], chainPoints[i - 1] + new Vector2(5, 0).RotatedBy(chainPoints[i - 1].AngleTo(chainPoints[i])), 0.4f);
                }
            }
            else
            {
                chainPoints = new Vector2[length];
                for (int i = 0; i < chainPoints.Length; i++)
                    chainPoints[i] = Projectile.Center;
            }
        }

        private void DrawChain(Color lightColor)
        {
            if (chainPoints != null)
            {
                for (int i = 0; i < chainPoints.Length - 1; i++)
                {
                    Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

                    int style = 0;
                    if (i == chainPoints.Length - 3)
                        style = 1;
                    if (i > chainPoints.Length - 3)
                        style = 2;
                    Rectangle frame = chainTex.Frame(1, 3, 0, style);
                    float rotation = chainPoints[i].AngleTo(chainPoints[i + 1]);
                    Vector2 stretch = new Vector2(0.3f + Utils.GetLerpValue(0, chainPoints.Length - 2, i, true) * 0.2f, chainPoints[i].Distance(chainPoints[i + 1]) / (frame.Height - 5));
                    Main.EntitySpriteDraw(chainTex, chainPoints[i] - Main.screenPosition, frame, lightColor.MultiplyRGBA(Color.Lerp(Color.DimGray, Color.White, (float)i / chainPoints.Length)), rotation - MathHelper.PiOver2, frame.Size() * new Vector2(0.5f, 0f), stretch, 0, 0);
                }
            }
        }
    }
}