using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using MoreKatana.Projectiles.PrimTrails;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassKatanaDance : CustomSword
    {
        public const int TotalSwings = 5; // 剣の振る数の合計
        public const float SwingUseTime = 20f; // 剣の振る時間

        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = (int)SwingUseTime;
            Projectile.MKProj().ActivateCD = true;
            Projectile.MKProj().Bool[0] = false;

            SwingEllipse = new(1.8f, 0.5f);
            ContinuousSwing = true; // 設定した全てのスイングを連続で行う
            FixedDirection = true; // 実際には発射体の方向は固定せずランダムですが、プレイヤーの向きを固定化するためにもこれを設定しておいた方が確実です
            NoSpeedBonus = true; // 速度ボーナスを適用しない
            CreateSound = false; // デフォルトのサウンドを鳴らさない

            GetTextureValues();

            // 剣の振る向きをランダムにする
            Projectile.velocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);

            // 葉の発射体をスポーン
            SpawnLeaf(out _);
        }

        // TotalSwingまでは同じスイングを繰り返す。以降は空にして発射体を消す
        public override SwingData GetSwingData(int type) => type < TotalSwings ? new SwingData(SwingUseTime, 1.25f, 0f, Main.rand.NextBool()) : new SwingData();

        public override void AdditionalAI(int type, bool onDelay)
        {
            Owner.direction = 1; // トレイルが崩れるのを防ぐためにプレイヤーの方向を固定する
            Owner.armorEffectDrawShadow = true; // プレイヤーの残像の効果
            Owner.SetDummyItemTime(2);
            Owner.FlipEffect(Progress * 6f); // フリップエフェクト
            Owner.MKPlayer().slowFallEffect = 2;

            if (GetProgress(type) > 0.5f)
            {
                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;

                    // 葉の発射体をスポーン
                    SpawnLeaf(out Vector2 vel);

                    // トレイルのみの斬撃を発射
                    // トレイルのみにする理由は演出面の問題
                    if (type < TotalSwings - 1)
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int subSwing = ModContent.ProjectileType<GrassKatanaDance2>();
                            Projectile.NewProjectile(Owner.GetSource_ItemUse(OwnerItem), Owner.MountedCenter, vel, subSwing, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                        }
                    }
                }
            }
        }

        private void SpawnLeaf(out Vector2 vel)
        {
            Owner.ScreenShake(2, 3);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = +0.5f }, Owner.Center);

            // 発射の向きをランダムにする
            vel = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);

            // 葉を発射
            if (Projectile.owner == Main.myPlayer)
            {
                float speed = 16f;
                int leaf = ModContent.ProjectileType<GrassLeaf>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, vel * speed, leaf, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);

        public override void DrawTrail(int type)
        {
            if (Timer > 1f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail, type: 0);
            }
        }
    }

    public class GrassKatanaDance2 : CustomSword
    {
        public override void Initialize(int type)
        {
            Projectile.localNPCHitCooldown = (int)GrassKatanaDance.SwingUseTime;
            SwingEllipse = new(1.8f, 0.5f);
            FixedDirection = true;
            NoSpeedBonus = true;
            CreateSound = false;
            GetTextureValues();
        }

        public override SwingData GetSwingData(int type) => new SwingData(GrassKatanaDance.SwingUseTime, 1.25f, 0f, Main.rand.NextBool());

        public override bool PreDraw(ref Color lightColor) => false;

        public override void DrawTrail(int type)
        {
            if (Timer > 1f)
            {
                if (!PrimsCreated)
                {
                    PrimsCreated = true;
                    SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(SwordTrail);
                }

                UpdateTrail(SwordTrail, type: 0);
            }
        }
    }
}