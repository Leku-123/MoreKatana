using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TrueSacredSecondaryMagicCircle : ModProjectile
    {
        private float HostIndex
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float MagicCircleDirection
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private ref float Timer => ref Projectile.ai[2];

        public float PrepareCompletion => MathHelper.Clamp(Timer / TrueSacredNaginataHoldout.PrepareTime, 0f, 1f);
        public float FireCompletion => MathHelper.Clamp((Timer - TrueSacredNaginataHoldout.PrepareTime) / TrueSacredNaginataHoldout.FireTime, 0f, 1f);

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 36000;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile hostProj = Main.projectile[(int)HostIndex];

            if (!hostProj.active || hostProj.type != ModContent.ProjectileType<TrueSacredNaginataHoldout>())
            {
                Projectile.Kill();
                return;
            }

            TrueSacredNaginataHoldout trueSacredNaginataHoldout = (TrueSacredNaginataHoldout)hostProj.ModProjectile;

            // 発射体の位置
            // ホールド発射体の剣先から、MagicCircleDirectionの向きに移動した位置をオフセットとし、
            // 発射体の位置とオフセットの線形補完を行う
            Vector2 offset = hostProj.Center + (Vector2.Normalize(hostProj.velocity) * 100f);
            float offsetDir = Vector2.Normalize(hostProj.velocity).ToRotation() + MathHelper.PiOver2;
            offset += offsetDir.ToRotationVector2() * (200f * MagicCircleDirection);
            const float posLerp = 0.065f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, offset, posLerp);

            // 発射体の速度をマウス方向への線形補完した速度の単位ベクトルに変換
            const float lerp = 0.08f;
            Vector2 normalizeVel = Vector2.Normalize(Projectile.velocity); //発射体の速度の単位ベクトル
            Projectile.velocity = Vector2.Lerp(normalizeVel, Vector2.Normalize(player.MKPlayer().MouseWorld - Projectile.Center), lerp);
            Projectile.velocity.Normalize();

            // ベロシティの方向に回転
            Projectile.rotation = Vector2.Normalize(Projectile.velocity).ToRotation();

            // 不透明度をホールド発射体に合わせる
            Projectile.Opacity = hostProj.Opacity;

            if (PrepareCompletion < 0.8f)
            {
                int newDust = Dust.NewDust(Projectile.Center - new Vector2(4), 32, 32, DustID.HallowedWeapons, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].scale *= 2;
                Main.dust[newDust].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2.5f, 4.5f);
            }
            else if (PrepareCompletion == 1f && FireCompletion < 1f)
            {
                if (Timer % 15 == 0)
                {
                    // 他の魔法陣とサウンドが重ならないようにサウンド処理する発射体を制限する
                    if (MagicCircleDirection == 1)
                        SoundEngine.PlaySound(SoundID.Item4, player.Center);

                    // パーティクルと発射体のスポーン位置にランダム性を持たせる
                    Vector2 vector = Main.rand.NextVector2Unit() * 50;

                    // パーティクル
                    ParticleOrchestraSettings particleOrchestraSettings = default;
                    particleOrchestraSettings.PositionInWorld = Projectile.Center + vector;
                    ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);

                    // 刃の発射体を発射
                    if (Projectile.owner == Main.myPlayer)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + vector, Projectile.velocity * 25, ModContent.ProjectileType<SacredEdge>(), Projectile.damage, 0, Projectile.owner);
                }
            }

            Timer++;
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            Texture2D circle = MoreKatanaTextures.MagicCircleTexture.Value;
            Texture2D ring = MoreKatanaTextures.MagicRingTexture.Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color circleColor = Color.Crimson * Projectile.Opacity;

            Main.EntitySpriteDraw(bloom, position, null, circleColor with { A = 0 } * 0.5f, Projectile.rotation, bloom.Size() / 2f, new Vector2(0.25f * PrepareCompletion, 0.5f * PrepareCompletion), 0, 0);

            MoreKatanaUtil.DrawCompression(circle, circleColor, Projectile.rotation, Projectile.Opacity, new Vector2(2, 1), 1, Timer, BlendState.Additive);
            Main.spriteBatch.Draw(circle, position, null, circleColor, 0f, circle.Size() / 2, 0.5f * PrepareCompletion, SpriteEffects.None, 0);

            MoreKatanaUtil.DrawCompression(ring, circleColor, Projectile.rotation, Projectile.Opacity, new Vector2(2, 1), 1, Timer / 10f, BlendState.Additive);
            Main.spriteBatch.Draw(ring, position, null, circleColor, 0f, ring.Size() / 2, 0.5f * PrepareCompletion, SpriteEffects.None, 0);

            Main.spriteBatch.SetEndBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}