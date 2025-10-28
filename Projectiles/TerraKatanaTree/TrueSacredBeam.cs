using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
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

        private const float MaxBeamScale = 3f;
        private const float MinDamageMultiplier = 0.2f;
        private const float MaxBeamLength = 1200f;

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

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + Projectile.velocity * BeamLength;
            float hitboxCollisionWidth = 22f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, hitboxCollisionWidth * Projectile.scale, ref _);
        }

        public override void CutTiles()
        {
            // ExampleMod:
            // tilecut_0 は、タイルがどのようにカットされるかを CutTiles に伝える無名の逆コンパイルされた変数です（この場合、Projectile 経由でカットされます）。
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + Projectile.velocity * BeamLength;

            // PlotTileLine は、描画された線に沿って指定された幅のすべてのタイルに対して、指定されたアクションを実行する関数です。
            // この場合、例えば草や壺など、投射物によって破壊可能なすべてのタイルをカットします。
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
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
            // ビームの位置をランダムで震わせる
            Vector2 firePos = hostProj.Center + (Vector2.Normalize(hostProj.velocity) * 100f);
            Projectile.Center = firePos;
            Projectile.Center += Main.rand.NextVector2Unit() * 3f;

            // ベロシティをホールド発射から取得
            // 向きを調節する
            Projectile.velocity = Vector2.Normalize(hostProj.velocity);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ビームのダメージを攻撃が終わるにつれ減らしていく
            Projectile.damage = (int)(hostProj.damage * MathHelper.Lerp(1f, MinDamageMultiplier, fireRatio));

            // 攻撃が終わるにつれスケールが小さくなる
            // ゼロにならないように0.01f加算する
            Projectile.scale = (MaxBeamScale * (1f - fireRatio)) + 0.01f;

            // 攻撃が終わるにつれ不透明度を減少させる
            Projectile.Opacity = 1f - fireRatio;

            // ビームの長さをLerpを使い最大まで伸ばす
            const float lerp = 0.75f;
            BeamLength = MathHelper.Lerp(BeamLength, MaxBeamLength, lerp);

            // ビームのx, yの寸法
            Vector2 beamDims = new Vector2(Projectile.velocity.Length() * BeamLength, Projectile.width * Projectile.scale);

            // 水をかき乱すシェーダー
            if (Main.netMode != NetmodeID.Server)
                ProduceWaterRipples(beamDims);

            // ビームが光るようにする
            // ExampleMod:
            // v3_1 は、DelegateMethods.CastLight によって生成される色を示す、名前が付けられていないデコンパイルされた変数です
            DelegateMethods.v3_1 = Color.Gold.ToVector3() * 0.75f * fireRatio;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * BeamLength, beamDims.Y, new Utils.TileActionAttempt(DelegateMethods.CastLight));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // ビームに明確な方向が無い場合描画しない
            if (Projectile.velocity == Vector2.Zero)
                return false;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 centerFloored = Projectile.Center.Floor() + Projectile.velocity * Projectile.scale * 10.5f;
            Vector2 drawScale = new Vector2(Projectile.scale);

            // ExampleMod: f_1 は名前が付けられていないデコンパイルされた変数で、その機能は不明です。1 のままにしておいてください。
            DelegateMethods.f_1 = 1f;
            Vector2 startPosition = centerFloored - Main.screenPosition;
            Vector2 endPosition = startPosition + Projectile.velocity * BeamLength;

            Color backColor = Color.Gold * Projectile.Opacity;
            Color mediumColor = Color.Crimson * Projectile.Opacity;
            Color frontColor = Color.White * Projectile.Opacity;

            DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale, backColor with { A = 0 });
            DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale * 0.8f, mediumColor with { A = 50 });
            DrawBeam(Main.spriteBatch, texture, startPosition, endPosition, drawScale * 0.3f, frontColor with { A = 0 });

            return false;
        }

        private void DrawBeam(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 drawScale, Color beamColor)
        {
            Utils.LaserLineFraming lineFraming = new Utils.LaserLineFraming(DelegateMethods.RainbowLaserDraw);

            // ExampleModより:
            // c_1 は DelegateMethods.RainbowLaserDraw によって描画されるビームのレンダリング色を示す、名前が付けられていないデコンパイルされた変数です
            DelegateMethods.c_1 = beamColor;
            Utils.DrawLaser(spriteBatch, texture, startPosition, endPosition, drawScale, lineFraming);
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