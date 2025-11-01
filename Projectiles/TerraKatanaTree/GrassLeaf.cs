using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassLeaf : ModProjectile, ITrailProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        public const int PrepareTime = 30;
        public float PrepareCompletion => MathHelper.Clamp(Timer / PrepareTime, 0f, 1f);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, new Color(0, 70, 0), MoreKatanaTextures.EnergyTrailTexture.Value, 32, 8);
        }

        public override void AI()
        {
            if (PrepareCompletion < 1)
            {
                // 減速する
                Projectile.velocity *= 0.92f;
            }
            else if (PrepareCompletion == 1)
            {
                // 最初のフレームで加速する
                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;

                    SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
                    MoreKatanaUtil.DrawRing(Projectile.Center, DustID.GrassBlades, 24, 6f);

                    // マウスの方向に加速させる
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 direct = Projectile.DirectionTo(Main.MouseWorld);
                        float speed = 25f;
                        Projectile.velocity += direct * speed;
                    }

                    Projectile.netUpdate = true;
                }
            }

            // 発射体の向きと回転
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0);

            // 不透明度
            Projectile.Opacity = MathHelper.Clamp(PrepareCompletion * 2, 0f, 1f);

            // アニメーションフレーム
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Timer++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * 7);

            for (int i = 0; i < 5; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, Projectile.oldVelocity.X * 0.2f, Projectile.oldVelocity.Y * 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Color.White * Projectile.Opacity;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // トレイル
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float fade = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                glowColor *= fade;
                float scale = Projectile.scale;
                scale *= fade;
                Vector2 pos = Projectile.oldPos[i];
                Main.EntitySpriteDraw(texture, pos + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), rectangle, glowColor, Projectile.rotation, origin, scale, spriteEffects, 0);
            }

            // アウトライン
            MoreKatanaUtil.DrawBackglow(texture, position, rectangle, glowColor with { A = 0 }, Projectile.rotation, PrepareCompletion, new Vector2(Projectile.scale), spriteEffects);

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}