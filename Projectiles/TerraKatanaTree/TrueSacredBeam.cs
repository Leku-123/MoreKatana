using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.PrimTrails;
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
    public class TrueSacredBeam : ModProjectile
    {
        private float HostIndex
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float BeamLength
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private const float MinDamageMultiplier = 0.2f;
        private const float MaxBeamLength = 1200f;

        private SacredBeamPrimTrail trail;

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
            Projectile.alpha = 255;
            Projectile.noEnchantmentVisuals = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // 何らかの原因でこの発射体がTrueSacredBeamじゃなくなった場合、この発射体を削除する
            if (Projectile.type != ModContent.ProjectileType<TrueSacredBeam>())
            {
                Projectile.Kill();
                return;
            }

            Projectile hostProj = Main.projectile[(int)HostIndex];
            if (!hostProj.active || hostProj.type != ModContent.ProjectileType<TrueSacredNaginataHoldout>())
            {
                Projectile.Kill();
                return;
            }

            TrueSacredNaginataHoldout trueSacredNaginataHoldout = (TrueSacredNaginataHoldout)hostProj.ModProjectile;
            if (trueSacredNaginataHoldout.FireCompletion >= 1f)
            {
                Projectile.Kill();
                return;
            }

            // ホールド発射体から攻撃の完了率の変数を取得する
            float fireRatio = trueSacredNaginataHoldout.FireCompletion;

            // ビームの開始位置をホールド発射の先の方に調節し、
            Vector2 hostVelocity = Vector2.Normalize(hostProj.velocity);
            Vector2 firePos = hostProj.Center + (hostVelocity * 100f);
            Projectile.Center = firePos;

            // ベロシティをホールド発射から取得
            // 向きを調節する
            Projectile.velocity = hostVelocity;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ビームのダメージを攻撃が終わるにつれ減らしていく
            Projectile.damage = (int)(hostProj.damage * MathHelper.Lerp(1f, MinDamageMultiplier, fireRatio));

            // 攻撃が終わるにつれスケールが小さくなる
            Projectile.scale = 1f - (fireRatio / 2);

            // ビームの長さをLerpを使い最大まで伸ばす
            const float lerp = 0.75f;
            BeamLength = MathHelper.Lerp(BeamLength, MaxBeamLength, lerp);

            // ビームのx, yの寸法
            Vector2 beamDims = new Vector2(Projectile.velocity.Length() * BeamLength, Projectile.width * Projectile.scale);

            // 水をかき乱すシェーダー
            if (Main.netMode != NetmodeID.Server)
                ProduceWaterRipples(beamDims);

            // ビームが光るようにする
            // v3_1 is an unnamed decompiled variable which is the color of the light cast by DelegateMethods.CastLight.
            DelegateMethods.v3_1 = Color.Gold.ToVector3() * 0.75f * fireRatio;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * BeamLength, beamDims.Y, new Utils.TileActionAttempt(DelegateMethods.CastLight));

            // ビームの描画
            DrawBeam();
        }

        private void DrawBeam()
        {
            if (!Projectile.MKProj().Bool[0])
            {
                Projectile.MKProj().Bool[0] = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    trail = new SacredBeamPrimTrail(Projectile, Color.Gold);
                    MoreKatana.primitives.CreateTrail(trail);
                }
            }
            else
            {
                if (trail == null)
                    return;

                Vector2 point = Projectile.Center;

                if (Main.netMode != NetmodeID.Server)
                {
                    trail.Points.Clear();
                    trail.Points.Add(point);
                    trail.Points.Add(point);

                    const int max = 10;
                    for (int i = 0; i < max; i++)
                    {
                        point += Projectile.velocity * MaxBeamLength / max;
                        trail.Points.Add(point);
                    }

                    trail.Scale = Projectile.scale;
                }
            }
        }

        private void ProduceWaterRipples(Vector2 beamDims)
        {
            WaterShaderData shaderData = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();

            // A universal time-based sinusoid which updates extremely rapidly. GlobalTime is 0 to 3600, measured in seconds.
            float waveSine = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
            Vector2 ripplePos = Projectile.position + new Vector2(beamDims.X * 0.5f, 0f).RotatedBy(Projectile.rotation);

            // WaveData is encoded as a Color. Not really sure why.
            Color waveData = new Color(0.5f, 0.1f * Math.Sign(waveSine) + 0.5f, 0f, 1f) * Math.Abs(waveSine);
            shaderData.QueueRipple(ripplePos, waveData, beamDims, RippleShape.Square, Projectile.rotation);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + Projectile.velocity * BeamLength;
            float hitboxCollisionWidth = 80f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, hitboxCollisionWidth * Projectile.scale, ref _);
        }

        public override void CutTiles()
        {
            // tilecut_0 is an unnamed decompiled variable which tells CutTiles how the tiles are being cut (in this case, via a Projectile).
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + Projectile.velocity * BeamLength;

            // PlotTileLine is a function which performs the specified action to all tiles along a drawn line, with a specified width.
            // In this case, it is cutting all tiles which can be destroyed by Projectiles, for example grass or pots.
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // トゥルーエクスカリバーのパーティクル
            ParticleOrchestraSettings particleOrchestraSettings = default;
            particleOrchestraSettings.PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox);
            ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.TrueExcalibur, particleOrchestraSettings, Projectile.owner);
        }
    }
}