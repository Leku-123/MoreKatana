using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraEdge : ModProjectile, ITrailProjectile
    {
        public enum AttackType
        {
            Firing, // 通常挙動
            Homing // ホーミング挙動
        }

        public AttackType CurrentType
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        private const int Lifetime = 120;
        public int TargetIndex = -1;

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.5f;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.noEnchantmentVisuals = true;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, TerraKatana.TerraColor[0], MoreKatanaTextures.EnergyTrailTexture.Value, 42, 15);
            tManager.CreateTrail(Projectile, TerraKatana.TerraColor[1] * 0.2f, MoreKatanaTextures.StraightlineTrailTexture.Value, 42, 18);
        }

        public override void AI()
        {
            Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;
            Projectile.scale = Utils.GetLerpValue(0f, 0.1f, Projectile.timeLeft / (float)Lifetime, true);
            Projectile.alpha = 0;

            if (CurrentType == AttackType.Firing)
            {
                // ダスト
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f) + Projectile.velocity, TerraKatana.DustType, Projectile.velocity * -1.2f, 0);
                    dust.scale = 0.3f;
                    dust.fadeIn = Main.rand.NextFloat() * 1.2f;
                    dust.noGravity = true;
                }
            }
            else if (CurrentType == AttackType.Homing)
            {
                Projectile.extraUpdates = 1;

                if (++Projectile.ai[1] > 15 * Projectile.MaxUpdates)
                {
                    if (TargetIndex >= 0)
                    {
                        if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                        {
                            TargetIndex = -1;
                        }
                        else
                        {
                            Vector2 idealVelocity = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 6.5f);
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.08f);
                        }
                    }
                    else if (TargetIndex == -1)
                    {
                        NPC potentialTarget = Projectile.Center.ClosestNPCAt(600f);
                        if (potentialTarget != null)
                            TargetIndex = potentialTarget.whoAmI;
                        else
                            Projectile.velocity *= 0.99f;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // テラブレードのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TerraBlade, particleOrchestraSettings, Projectile.owner);

            if (CurrentType == AttackType.Homing)
            {
                for (int i = 0; i < 12; ++i)
                {
                    int newDust = Dust.NewDust(target.position, target.width, target.height, TerraKatana.DustType, 0f, 0f, 100, default, 1f);
                    Main.dust[newDust].noGravity = true;
                    Main.dust[newDust].noLight = true;
                    Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity).RotatedBy(15);
                    Main.dust[newDust].velocity *= 10;
                }

                Projectile.ExpandHitboxBy(150);
                Projectile.Damage();
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Color.White with { A = 0 } * Projectile.Opacity;
            float bladeScale = Utils.GetLerpValue(3f, 13f, Projectile.velocity.Length(), true);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale * bladeScale, spriteEffects, 0);

            Vector2 offset = Vector2.Normalize(Projectile.velocity) * 65f * Projectile.scale * bladeScale;
            MoreKatanaUtil.DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, color, TerraKatana.TerraColor[0],
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale, Projectile.scale * 1.5f), new Vector2(1f, 1f));
            return false;
        }
    }
}