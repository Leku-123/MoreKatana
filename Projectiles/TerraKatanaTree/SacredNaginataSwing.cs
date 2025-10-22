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
    public class SacredNaginataSwing : CustomSword
    {
        public override string Texture => this.GetTexture(Name);

        private Vector2 DirectionToProj => Utils.DirectionTo(Owner.MountedCenter, Projectile.Center);

        /// <summary> 刀身が長いためトレイルの横幅を小さくして、オフセットを先端に調節する </summary>
        public override void DrawTrail(int type)
        {
            if (GetProgress(type) >= 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, 20, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                Vector2 offset = DirectionToProj * 60f;
                Vector2 p = Projectile.Center - Owner.MountedCenter;
                UpdateTrail(SwordTrail, GetProgress(type) >= 0.98f, point: p + offset);
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

            SwordSize(124);
            TrailColor = Color.Gold * 0.3f;
        }

        #region 振りの設定
        public SwingData Down => new SwingData(SwordItem.useAnimation / 2f, 0.6f, 0.2f); // 1振り目
        public SwingData Up => new SwingData(SwordItem.useAnimation, 0.6f, 0.2f, true); // 2振り目
        public SwingData Spin => new SwingData(SwordItem.useAnimation * 1.5f, 2.6f, 0.25f, delay: SwordItem.useAnimation / 2f); // 3振り目
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up, Spin);
        #endregion

        #region 振りのアニメーションの設定
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
        #endregion

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 2)
            {
                if (!delay)
                {
                    // 振りかぶり時はダメージを与えないようにする
                    Projectile.friendly = GetProgress(type) >= 0f;

                    Owner.FlipEffect(GetProgress(type) * 9f);

                    if (Projectile.soundDelay % (20 * Projectile.MaxUpdates) == 0)
                        SoundEngine.PlaySound(SoundID.Item169, Owner.position);
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

            Owner.ScreenShake(2, 6);

            SoundEngine.PlaySound(MoreKatanaSounds.SlashHit, Owner.Center);

            // エクスカリバーのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.Excalibur, particleOrchestraSettings, Projectile.owner);

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
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale * 2f, Projectile.scale * 5f), new Vector2(1f, 1f));

            return false;
        }
    }
}