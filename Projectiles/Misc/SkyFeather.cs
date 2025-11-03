using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Particles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Misc
{
    public class SkyFeather : ModProjectile
    {
        public int TargetIndex = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // 発射体の回転と方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;

            // 発射体の残り時間に応じてフェードアウトをする
            if (Projectile.timeLeft <= 10)
            {
                if (Projectile.alpha < 255)
                {
                    Projectile.friendly = false;
                    Projectile.timeLeft = 2; // 完全に透明になるまで発射体の残り時間を延長する
                    Projectile.alpha += 25; // アルファ値を加算
                    Projectile.velocity *= 0.75f; // 速度を遅くする
                }
            }
            else // フェードアウト前は通常の挙動を行う
            {
                // ホーミングの処理
                if (Projectile.ai[0] == 0)
                {
                    // ターゲットがいる場合
                    if (TargetIndex >= 0)
                    {
                        if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                        {
                            // ターゲットをホーミングできない場合はターゲットをリセット
                            TargetIndex = -1;
                        }
                        else
                        {
                            // ターゲットの方向にベロシティを調節する
                            Vector2 idealVelocity = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 4.5f);
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.04f);
                        }
                    }
                    // ターゲットがいない場合
                    else if (TargetIndex == -1)
                    {
                        // ターゲットを取得する
                        NPC target = Projectile.Center.ClosestNPCAt(200f, false);
                        if (target != null)
                            TargetIndex = target.whoAmI;
                    }
                }

                // パーティクル
                if (Main.rand.NextBool(5))
                {
                    Vector2 pos = Projectile.oldPos[4] + Projectile.Size / 2 + Main.rand.NextVector2Circular(10, 10);
                    Vector2 vel = Vector2.Normalize(Projectile.velocity) * 0.5f;
                    Color color = Color.Lerp(Color.DodgerBlue, Color.LightCyan, Main.rand.NextFloat());
                    Vector2 scale = new Vector2(0.25f, Main.rand.NextFloat(0.5f, 1f));
                    Particle line = new ImpactLine(pos, vel, color, scale, 60) { TimeActive = 30 };
                    ParticleHandler.SpawnParticle(line);
                }
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            // タイスに接触するヒットボックスを小さく設定する
            width = 8;
            height = 8;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false; // タイル接触をこれ以上させないようにする
            Projectile.velocity = oldVelocity; // ベロシティを接触時のものと合わせる
            Projectile.netUpdate = true;

            // 発射体の残り時間を更新してフェードアウトするようにする
            if (Projectile.timeLeft > 10)
                Projectile.timeLeft = 10;

            return false; // タイル接触で発射体を消さない
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 発射体の残り時間を更新してフェードアウトするようにする
            if (Projectile.timeLeft > 10)
                Projectile.timeLeft = 10;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // トレイル
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                float fade = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Color color2 = color;
                color2 *= fade;
                float scale = Projectile.scale;
                scale *= fade;
                Vector2 pos = Projectile.oldPos[i];
                Main.EntitySpriteDraw(texture, pos + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), rectangle, color2, Projectile.rotation, origin, scale, spriteEffects, 0);
            }

            // 本体の描画
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}