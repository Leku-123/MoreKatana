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

        private CustomSwordPrimTrail trail;

        /// <summary>
        /// 刀身が長いためトレイルの横幅を小さくして、オフセットを先端に調節する
        /// </summary>
        /// <param name="dir"></param>
        public override void DrawTrail(int dir, int type)
        {
            if (Timer != 0f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    trail = new CustomSwordPrimTrail(Projectile, TrailColor, 20, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(trail);
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    trail.TextureType = 0;
                    trail.Direction = Owner.direction * -dir;
                    trail.PrimCenter = Owner.MountedCenter;

                    Vector2 offset = DirectionToProj * 60f;

                    trail.Points.Add(Projectile.Center + offset - Owner.MountedCenter);

                    if (Progress >= 0.98f)
                        trail?.OnDestroy();
                }
            }
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = (type != 2 ? Owner.itemAnimationMax : Owner.itemAnimationMax / 3) * Projectile.MaxUpdates;
            SwordSize(124);
            TrailColor = Color.Gold * 0.3f;
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(1f, 0.45f);
            float num = Owner.itemAnimationMax / 3f;
            SwingStats(num * 2f, 0.6f, 0.2f, type % 2 != 0);
            DelayTimer = num;

            if (type == 2)
            {
                SwingEllipse = new(0.5f);
                SwingStats(40, 3.25f, 0.25f);
                DelayTimer = 15;
            }

            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f);
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f);
        public override float GetProgress(int type) => type != 2 ? PiecewiseAnimation(Progress, execute, unwind) : LinearEasing(Progress, 1);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);

            if (type == 2)
            {
                if (!delay)
                {
                    Owner.FlipEffect(Progress * 9f);

                    if (Projectile.soundDelay <= 0)
                    {
                        Projectile.soundDelay = 15 * Projectile.MaxUpdates;

                        if (Progress > 0.1f)
                            SoundEngine.PlaySound(SoundID.Item169, Owner.Center);
                    }
                }
                else
                {
                    float disappearProgress = 1 - (DelayTimer / (15 * Projectile.MaxUpdates));
                    Projectile.alpha = (int)(255 * CircOutEasing(disappearProgress, 1));
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
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.Excalibur, particleOrchestraSettings, Projectile.owner);

            for (int i = 0; i < 2; i++)
            {
                Vector2 vector = Main.rand.NextVector2Unit() * 200;
                int edgeProj = ModContent.ProjectileType<SacredEdge>();
                if (Owner.ownedProjectileCounts[edgeProj] < 6)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + vector, -vector / 10, edgeProj, Projectile.damage / 3, 0, Projectile.owner);
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

            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), spriteEffects | spriteEffects2);

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            // 剣先にスパークルを描画する
            Vector2 offset = DirectionToProj * 80f;
            MoreKatanaUtil.DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

            return false;
        }
    }
}