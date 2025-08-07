using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TrueSacredNaginataSwing : CustomSword
    {
        public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

        private Vector2 DirectionToProj => Utils.DirectionTo(Owner.MountedCenter, Projectile.Center);

        private CustomSwordPrimTrail trail, trail2;

        /// <summary>
        /// 刀身が長いためトレイルの横幅を小さくして、オフセットを先端に調節する
        /// 二種類のトレイルを重ねてスポーンさせる
        /// </summary>
        /// <param name="dir"></param>
        public override void DrawTrail(int dir)
        {
            if (Timer != 0f)
            {
                if (!primsCreated)
                {
                    primsCreated = true;
                    trail = new CustomSwordPrimTrail(Projectile, Color.Gold * 0.3f, 20, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(trail);
                    trail2 = new CustomSwordPrimTrail(Projectile, Color.Crimson * 0.3f, 20, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(trail2);
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    trail.TextureType = 0;
                    trail.Direction = Owner.direction * -dir;
                    trail.PrimCenter = Owner.MountedCenter;
                    trail2.TextureType = 0;
                    trail2.Direction = Owner.direction * -dir;
                    trail2.PrimCenter = Owner.MountedCenter;

                    Vector2 offset = DirectionToProj * 50f;
                    Vector2 offset2 = DirectionToProj * 60f;

                    trail.Points.Add(Projectile.Center + offset - Owner.MountedCenter);
                    trail2.Points.Add(Projectile.Center + offset2 - Owner.MountedCenter);

                    if (progress >= 0.98f)
                    {
                        trail?.OnDestroy();
                        trail2?.OnDestroy();
                    }
                }
            }
        }

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = (type != 2 ? Owner.itemAnimationMax : Owner.itemAnimationMax / 3) * Projectile.MaxUpdates;
            SwordSize(126);
            TrailColor = Color.Gold * 0.3f;
        }

        public override bool AttackPattern(Item item, int type)
        {
            GetEllipse(1f, 0.45f);
            float num = Owner.itemAnimationMax / 3f;
            SwingStats(num * 2f, 0.6f, 0.2f, type % 2 != 0);
            DelayTimer = num;

            if (type == 2)
            {
                GetEllipse(0.5f, 0.5f);
                SwingStats(40, 3.25f, 0.25f);
                DelayTimer = 15;
            }

            return base.AttackPattern(item, type);
        }

        public override float GetProgress(int type) => type != 2 ? EaseFunction.EaseCubicOut.Ease(progress) : EaseFunction.Linear.Ease(progress);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            Owner.SetDummyItemTime(2);
            ChainPhysics();

            if (type == 2)
            {
                if (!delay)
                {
                    Owner.FlipEffect(progress * 9f);

                    if (Projectile.soundDelay <= 0)
                    {
                        Projectile.soundDelay = 15 * Projectile.MaxUpdates;

                        if (progress > 0.1f)
                            SoundEngine.PlaySound(SoundID.Item169, Owner.Center);
                    }
                }
                else
                {
                    float disappearProgress = 1 - (DelayTimer / (15 * Projectile.MaxUpdates));
                    Projectile.alpha = (int)(255 * EaseFunction.EaseCubicOut.Ease(disappearProgress));
                    Owner.reuseDelay = 5;
                }
            }
            else
            {
                Owner.FlipEffect(0);
            }
        }

        private Vector2[] chainPoints;
        private Vector2[] chainVels;

        public void ChainPhysics()
        {
            int length = 12;
            if (chainVels != null)
            {
                for (int i = 0; i < chainVels.Length; i++)
                {
                    chainVels[i] = (Vector2.UnitY.ToRotation() - (i * 0.01f)).ToRotationVector2() * 2f;
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);

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
            Color glowColor = Color.White * (1f - (Projectile.alpha / 255f));
            Color trailColor = TrailColor * (1f - (Projectile.alpha / 255f));

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = Backspin ? SpriteEffects.FlipVertically : SpriteEffects.None;

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                backglowOffset *= 1 - progress;
                glowColor.A = 0;
                Main.EntitySpriteDraw(texture, position + backglowOffset, rectangle, glowColor, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);
            }

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            // 剣先にスパークルを描画する
            Vector2 offset = DirectionToProj * 80f;
            MoreKatanaUtil.DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - progress), trailColor * (1 - progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 2.5f), new Vector2(1f, 1f));

            DrawChain(color);
            return false;
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