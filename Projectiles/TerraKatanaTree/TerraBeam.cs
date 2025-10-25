using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Buffs;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class TerraBeam : ModProjectile
    {
        private const int Lifetime = 28;
        private const int BeamDustID = DustID.Terra;

        private const float MaxBeamScale = 1.2f;

        private const float MaxBeamLength = 2400f;
        private const float BeamTileCollisionWidth = 1f;
        private const int NumSamplePoints = 3;
        private const float BeamLengthChangeFactor = 0.75f;

        private const float OuterBeamOpacityMultiplier = 0.82f;
        private const float InnerBeamOpacityMultiplier = 0.2f;
        private const float MaxBeamBrightness = 0.75f;

        private const float MainDustBeamEndOffset = 14.5f;
        private const float SidewaysDustBeamEndOffset = 4f;
        private const float BeamRenderTileOffset = 10.5f;
        private const float BeamLengthReductionFactor = 14.5f;

        private Vector2 beamVector = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            if (Projectile.type != ModContent.ProjectileType<TerraBeam>())
            {
                Projectile.Kill();
                return;
            }

            // 1フレームでベロシティと回転を設定
            // 実際のベロシティは0にする
            if (Projectile.velocity != Vector2.Zero)
            {
                beamVector = Vector2.Normalize(Projectile.velocity);
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            // 発射体の消滅までの進行に合わせてスケールを調節する
            float power = (float)Projectile.timeLeft / Lifetime;
            Projectile.scale = MaxBeamScale * power;

            // LaserScanを実行してビームの長さを計算する
            float[] laserScanResults = new float[NumSamplePoints];
            float scanWidth = Projectile.scale < 1f ? 1f : Projectile.scale;
            Collision.LaserScan(Projectile.Center, beamVector, BeamTileCollisionWidth * scanWidth, MaxBeamLength, laserScanResults);
            float avg = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
                avg += laserScanResults[i];
            avg /= NumSamplePoints;
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], avg, BeamLengthChangeFactor);

            // ビームのx, yの寸法
            Vector2 beamDims = new Vector2(beamVector.Length() * Projectile.ai[0], Projectile.width * Projectile.scale);

            Color beamColor = GetBeamColor();
            ProduceBeamDust(beamColor);

            // 水をかき乱すシェーダー
            if (Main.netMode != NetmodeID.Server)
            {
                ProduceWaterRipples(beamDims);
            }

            // ビームが光るようにする
            // ExampleMod:
            // v3_1 は、DelegateMethods.CastLight によって生成される色を示す、名前が付けられていないデコンパイルされた変数です
            DelegateMethods.v3_1 = beamColor.ToVector3() * power * MaxBeamBrightness;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + beamVector * Projectile.ai[0], beamDims.Y, DelegateMethods.CastLight);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // ターゲットが発射体のヒットボックスに触れている場合はおけ
            if (projHitbox.Intersects(targetHitbox))
                return true;

            // それ以外の場合は、AABB線衝突チェックを実行してビーム全体をチェックする
            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + beamVector * Projectile.ai[0];
            float hitboxCollisionWidth = 15f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, hitboxCollisionWidth * Projectile.scale, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 命中した際の方向を正しくする
            modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // テラブレードのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TerraBlade, particleOrchestraSettings, Projectile.owner);
        }

        private Color GetBeamColor()
        {
            Color c = new Color(96, 248, 96);
            return c;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (beamVector == Vector2.Zero || Projectile.velocity != Vector2.Zero)
                return false;

            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            float beamLength = Projectile.ai[0];
            Vector2 centerFloored = Projectile.Center.Floor() + beamVector * Projectile.scale * BeamRenderTileOffset;
            Vector2 scaleVec = new Vector2(Projectile.scale);

            beamLength -= BeamLengthReductionFactor * Projectile.scale * Projectile.scale;

            DelegateMethods.f_1 = 1f; // f_1 is an unnamed decompiled variable whose function is unknown. Leave it at 1.
            Vector2 beamStartPos = centerFloored - Main.screenPosition;
            Vector2 beamEndPos = beamStartPos + beamVector * beamLength;
            Utils.LaserLineFraming llf = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

            // Draw the outermost beam
            // c_1 is an unnamed decompiled variable which is the render color of the beam drawn by DelegateMethods.RainbowLaserDraw
            Color beamColor = GetBeamColor();
            DelegateMethods.c_1 = beamColor * OuterBeamOpacityMultiplier * Projectile.Opacity;
            Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);

            for (int i = 0; i < 5; ++i)
            {
                beamColor = Color.Lerp(beamColor, Color.White, 0.4f);
                scaleVec *= 0.85f;
                DelegateMethods.c_1 = beamColor * InnerBeamOpacityMultiplier * Projectile.Opacity;
                Utils.DrawLaser(Main.spriteBatch, tex, beamStartPos, beamEndPos, scaleVec, llf);
            }

            Texture2D bloomTex = MoreKatanaTextures.BloomTexture.Value;
            Texture2D starTex = MoreKatanaTextures.StarSparkleTexture.Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = new Color(96, 248, 96) with { A = 0 };
            Main.EntitySpriteDraw(bloomTex, position, null, color, 0f, bloomTex.Size() / 2f, Projectile.scale * 0.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(starTex, position, null, color, 0f, starTex.Size() / 2f, new Vector2(Projectile.scale * 0.7f, Projectile.scale * 1.5f), SpriteEffects.None, 0);

            return false;
        }

        private void ProduceBeamDust(Color beamColor)
        {
            Vector2 laserEndPos = Projectile.Center + beamVector * (Projectile.ai[0] - MainDustBeamEndOffset * Projectile.scale);
            for (int i = 0; i < 2; ++i)
            {
                float dustAngle = Projectile.rotation + (Main.rand.NextBool() ? 1f : -1f) * MathHelper.PiOver2;
                float dustStartDist = Main.rand.NextFloat(1f, 1.8f);
                Vector2 dustVel = dustAngle.ToRotationVector2() * dustStartDist;
                int d = Dust.NewDust(laserEndPos, 0, 0, BeamDustID, dustVel.X, dustVel.Y, 0, beamColor);
                Main.dust[d].color = beamColor;
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.7f;

                if (Projectile.scale > 1f)
                {
                    Main.dust[d].velocity *= Projectile.scale;
                    Main.dust[d].scale *= Projectile.scale;
                }

                if (Projectile.scale != MaxBeamScale)
                {
                    Dust smallDust = Dust.CloneDust(d);
                    smallDust.scale /= 2f;
                }
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 dustOffset = beamVector.RotatedBy(MathHelper.PiOver2) * (Main.rand.NextFloat() - 0.5f) * Projectile.width;
                Vector2 dustPos = laserEndPos + dustOffset - Vector2.One * SidewaysDustBeamEndOffset;
                int d = Dust.NewDust(dustPos, 8, 8, BeamDustID, 0f, 0f, 100, beamColor, 1.2f);
                Main.dust[d].velocity *= 0.5f;
                Main.dust[d].velocity.Y = -Math.Abs(Main.dust[d].velocity.Y);
            }
        }

        public override void CutTiles()
        {
            // ExampleMod:
            // tilecut_0 は、タイルがどのようにカットされるかを CutTiles に伝える無名の逆コンパイルされた変数です（この場合、Projectile 経由でカットされます）。
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + beamVector * Projectile.ai[0];

            // PlotTileLine は、描画された線に沿って指定された幅のすべてのタイルに対して、指定されたアクションを実行する関数です。
            // この場合、例えば草や壺など、投射物によって破壊可能なすべてのタイルをカットします。
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
        }

        private void ProduceWaterRipples(Vector2 beamDims)
        {
            WaterShaderData shaderData = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();

            // 普遍的な時間ベースの正弦波で、非常に高速に更新されます。GlobalTimeは0から3600の範囲で、秒単位で測定されます。
            float waveSine = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
            Vector2 ripplePos = Projectile.position + new Vector2(beamDims.X * 0.5f, 0f).RotatedBy(Projectile.rotation);

            // WaveDataはカラーとしてエンコードされています。なぜそうなのかはよく分かりません。
            Color waveData = new Color(0.5f, 0.1f * Math.Sign(waveSine) + 0.5f, 0f, 1f) * Math.Abs(waveSine);
            shaderData.QueueRipple(ripplePos, waveData, beamDims, RippleShape.Square, Projectile.rotation);
        }
    }
}