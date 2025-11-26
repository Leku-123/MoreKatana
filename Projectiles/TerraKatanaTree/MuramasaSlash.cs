using Microsoft.Xna.Framework;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Projectiles.PrimTrails;
using System.Collections.Generic;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class MuramasaSlash : ModProjectile, ITrailProjectile
    {
        public List<Vector2> points = new List<Vector2>();

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.noEnchantmentVisuals = true;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.ArmorPenetration = 5;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, Color.Blue, MoreKatanaTextures.CutlineTrailTexture.Value, 80, 50, 0);
            tManager.CreateTrail(Projectile, Color.White, MoreKatanaTextures.CutlineTrailTexture.Value, 20, 50, 0);
        }

        public override void AI()
        {
            for (int i = 0; i < 2; ++i)
            {
                int newDust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.DungeonWater, 0f, 0f, 100, default, 0.7f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].noLight = true;
                Main.dust[newDust].velocity = Vector2.Normalize(Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15));
                Main.dust[newDust].velocity *= Main.rand.NextFloat(2f, 4f);
            }

            foreach (Vector2 point in points)
                Lighting.AddLight(point, Color.Blue.ToVector3());

            points.Add(Projectile.position);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 prev = Vector2.Zero;
            foreach (Vector2 point in points)
            {
                if (prev != Vector2.Zero)
                {
                    float collisionPoint = 0f;

                    if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), point, prev, 20, ref collisionPoint))
                        return true;
                }

                prev = point;
            }
            return false;
        }

        public override bool? CanCutTiles() => true;

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 prev = Vector2.Zero;
            foreach (Vector2 point in points)
            {
                if (prev != Vector2.Zero)
                    Utils.PlotTileLine(point, prev, 20, DelegateMethods.CutTiles);

                prev = point;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.position.X > Main.player[Projectile.owner].MountedCenter.X ? 1 : -1;
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}