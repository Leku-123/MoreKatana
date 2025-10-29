using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Particles;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraWave : ModProjectile, ITrailProjectile
    {
        private const int MaximumHits = 3;
        private const int HitboxDims = 80;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.scale = 1.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 150;
            Projectile.noEnchantmentVisuals = true;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, TerraKatana.TerraColor[0], MoreKatanaTextures.EnergyTrailTexture.Value, 150, 15);
            tManager.CreateTrail(Projectile, TerraKatana.TerraColor[1] * 0.2f, MoreKatanaTextures.StraightlineTrailTexture.Value, 150, 18);
            tManager.CreateTrail(Projectile, TerraKatana.TerraColor[0] * 0.2f, MoreKatanaTextures.DoublelinesTrailTexture.Value, 70, 8);
        }

        public bool DoTrailDeletion() => Projectile.timeLeft <= 25;

        public override void AI()
        {
            // 発射体の残り時間に応じてフェードアウトをする
            if (Projectile.timeLeft <= 25)
            {
                if (Projectile.alpha < 255)
                {
                    Projectile.timeLeft = 2; // 完全に透明になるまで発射体の残り時間を延長する
                    Projectile.alpha += 12; // アルファ値を加算
                    Projectile.scale *= 1.03f; //スケールを大きくしていく
                    Projectile.velocity *= 0.9f; // ベロシティを遅くしていく
                }
            }
            // フェードアウト時以外はパーティクルをスポーンさせる
            else
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 pos = Vector2.Lerp(Projectile.Center, Projectile.Center, Main.rand.NextFloat()) + Main.rand.NextVector2Circular(HitboxDims, HitboxDims);
                    Vector2 vel = Vector2.Normalize(Projectile.velocity) * 2f;
                    Color color = Color.Lerp(TerraKatana.TerraColor[1], TerraKatana.TerraColor[0], Main.rand.NextFloat());
                    Vector2 scale = new Vector2(0.25f, Main.rand.NextFloat(0.5f, 1.5f)) * 2;
                    Particle line = new ImpactLine(pos, vel, color, scale, 60) { TimeActive = 30 };
                    ParticleHandler.SpawnParticle(line);
                }
            }

            // 発射体の回転をベロシティの向きにする
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ヒット時のカウントに応じて発射体の残り時間を設定し、フェードアウトをするようにする
            if (Projectile.ai[0] >= MaximumHits)
            {
                if (Projectile.timeLeft > 25)
                    Projectile.timeLeft = 25;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false; // タイル接触をこれ以上させないようにする
            Projectile.velocity = oldVelocity; // ベロシティを接触時のものと合わせる
            Projectile.netUpdate = true;

            // 発射体の残り時間を更新してフェードアウトするようにする
            if (Projectile.timeLeft > 25)
                Projectile.timeLeft = 25;

            return false; // タイル接触で発射体を消さない
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // ターゲットが発射体のヒットボックスに触れている場合はおけ
            if (projHitbox.Intersects(targetHitbox))
                return true;

            // それ以外の場合は、CheckAABBvLineCollisionを実行する
            // 発射体のテクスチャが縦長なため、発射体のベロシティと垂直方向の縦長のラインとしてチェックをする
            float _ = float.NaN;
            Vector2 offset = HitboxDims / 2 * Projectile.scale * Vector2.Normalize(Projectile.velocity.TurnLeft());
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref _);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // テラブレードのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TerraBlade, particleOrchestraSettings, Projectile.owner);

            Projectile.ai[0]++; // ヒット時のカウントを増やしていく
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 position = Projectile.Center - (Vector2.Normalize(Projectile.velocity) * 16f) - Main.screenPosition;
            Main.EntitySpriteDraw(texture, position, null, TerraKatana.TerraColor[0] with { A = 0 } * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale * new Vector2(1.4f, 1), SpriteEffects.None, 0);

            for (int i = -1; i <= 1; i++)
            {
                Vector2 offset = Vector2.Normalize(Projectile.velocity).RotatedBy(MathHelper.ToRadians(60) * i) * 30f * Projectile.scale;
                MoreKatanaUtil.DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset,
                    Color.White * Projectile.Opacity, TerraKatana.TerraColor[0] * Projectile.Opacity,
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale * 1.5f), new Vector2(1f, 1f));
                MoreKatanaUtil.DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset,
                    Color.White * Projectile.Opacity, TerraKatana.TerraColor[1] * Projectile.Opacity,
                    0.5f, 0f, 0.1f, 0.9f, 1f, MathHelper.PiOver4, new Vector2(Projectile.scale * 0.8f), new Vector2(1f, 1f));
            }

            return false;
        }
    }
}