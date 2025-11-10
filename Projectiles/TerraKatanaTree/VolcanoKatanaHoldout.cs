using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Weapons.TerraKatanaTree;
using MoreKatana.Particles;
using MoreKatana.Projectiles.Base;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class VolcanoKatanaHoldout : CustomSword
    {
        public const int MaxRockCount = 5;
        private bool ChargeComplete;

        public override bool? CanDamage() => false;

        public override void Initialize(int type)
        {
            Projectile.MKProj().ActivateCD = true;

            ContinuousSwing = true; // 設定した全てのスイングを連続で行う
            FixedDirection = true; // プレイヤーと発射体の方向を固定する
            CreateSound = false; // デフォルトのサウンドを鳴らさない
            NoSpeedBonus = true; // 速度ボーナスを適用しない

            GetTextureValues();

            // 初期化時に発射体スポーンとその演出を行う
            ChargeEffects(type);
        }

        private void ChargeEffects(int type)
        {
            // スクリーンシェイクの演出
            Owner.ScreenShake(5, 5);

            // サウンドとパーティクルの演出
            if (type == MaxRockCount - 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, Owner.Center);
                ParticleHandler.SpawnParticle(new PulseCircle(Owner.Center, Vector2.UnitY, VolcanoKatana.FireColor(), new Vector2(1.2f), 40, CircOutEasing));
            }
            else
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, Owner.Center);
                ParticleHandler.SpawnParticle(new PulseCircle(Owner.Center, Vector2.UnitY, VolcanoKatana.FireColor(), new Vector2(0.8f), 40, CircOutEasing));
            }

            // 火球の発射体を発射
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Top, Vector2.Zero, ModContent.ProjectileType<VolcanicEruptionRock>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, type);
        }

        // 突きのスイングデータ
        public override SwingData GetSwingData(int type) => new SwingData(OwnerItem.useAnimation, 0f, 0.5f, delay: 10f);

        // 突きのアニメーション
        public CurveSegment Thrust => new CurveSegment(CircInEasing, 0f, 0.15f, 0.85f); // 突き
        public CurveSegment Hold => new CurveSegment(SineBumpEasing, 0.2f, Thrust.EndingHeight, 0.2f); // 少しのホールド
        public CurveSegment Retract => new CurveSegment(PolyOutEasing, 0.5f, Hold.EndingHeight, -1f, 2); // 手元に引き戻す
        public CurveSegment Correct => new CurveSegment(PolyInEasing, 0.85f, Retract.EndingHeight, 0.17f); // 位置の調節
        internal float ThrustAnimation => PiecewiseAnimation(Progress, Thrust, Hold, Retract, Correct);
        public override float GetProgress(int type) => ThrustAnimation;

        public override void AdditionalAI(int type, bool onDelay)
        {
            // プレイヤーのアイテム使用時間、
            // 発射体の残り時間を延長する
            Owner.SetDummyItemTime(2);
            Projectile.timeLeft = 2;

            // 右クリックを離したら発射体を消す
            if (Projectile.owner == Main.myPlayer && !Main.mouseRight)
                Projectile.Kill();

            // SwingEllipseのX値を調節することで疑似的に突きの挙動にする
            SwingEllipse = new(0.7f + (0.3f * GetProgress(type)), 1);

            // 突きの頂点に向かってスケールを大きくする
            Projectile.scale = 1f + (0.2f * GetProgress(type));

            // 最後のスイング処理はディレイを引き延ばす
            if (onDelay && type == MaxRockCount - 1)
            {
                if (DelayTimer > 1f)
                {
                    // マウスを右クリックしている場合はディレイを延長する
                    // DelayTimerを更新し続けることでディレイを進ませない
                    if (Projectile.owner == Main.myPlayer && Main.mouseRight)
                    {
                        DelayTimer = 2f;
                        Projectile.netUpdate = true;
                    }
                }

                Owner.ScreenShake(10, 2);

                // 発射体の位置をランダムで揺らす
                Projectile.Center += Main.rand.NextVector2Unit();

                // 火球のチャージの完了フラグ
                ChargeComplete = true;
            }
        }

        public override void DrawTrail(int type) { } // カスタムスイングのトレイルは行わない

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Item[OwnerItem.type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color glowColor = Color.White * Projectile.Opacity;
            Color trailColor = VolcanoKatana.FireColor() * Projectile.Opacity;

            // バックグローの描画
            if (Progress != 1f)
                DrawBackglow(texture, position, null, VolcanoKatana.FireColor(20) * Projectile.Opacity, Projectile.rotation, 4f * (1 - Progress), new Vector2(Projectile.scale), SwingEffectsHV());
            if (ChargeComplete)
                DrawBackglow(texture, position, null, VolcanoKatana.FireColor(20) * Projectile.Opacity, Projectile.rotation, 4f * ((float)Math.Sin(Main.GameUpdateCount / 30f) + 0.3f), new Vector2(Projectile.scale), SwingEffectsHV());

            // 武器本体の描画
            DrawBasicSword(texture, Projectile.Center);

            // 剣先にスパークルを描画する
            Vector2 offset = Utils.DirectionTo(Owner.MountedCenter, Projectile.Center) * 40f * Projectile.scale;
            DrawPrettyStarSparkle(1f, SpriteEffects.None, position + offset, glowColor * (1 - Progress), trailColor * (1 - Progress),
                    0.5f, 0f, 0.1f, 0.9f, 1f, 0f, new Vector2(Projectile.scale * 1.5f, Projectile.scale * 3.5f), new Vector2(1f, 1f));

            return false;
        }
    }
}