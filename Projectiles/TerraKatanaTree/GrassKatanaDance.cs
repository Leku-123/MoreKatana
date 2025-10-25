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
        public const int TotalSwing = 5; // 振り数の合計
        public const float SwingUseTime = 20f; // 剣の振る時間

        public override void Initialize(Item item, int type)
        {
            Projectile.localNPCHitCooldown = (int)SwingUseTime;
            Projectile.MKProj().ActivateCD = true;

            SwingEllipse = new(1.8f, 0.5f);
            ContinuousSwing = true;
            FixedDirection = true;
            NoSpeedBonus = true;
            CreateSound = false;

            GetTextureValues();

            // 剣の振る向きをランダムにする
            Projectile.velocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);

            // 葉の発射体をスポーン
            SpawnLeaf(out _);
        }

        public override SwingData GetSwingData(int type)
        {
            // TotalSwingまでは同じスイングを繰り返す。以降は消す
            if (type < TotalSwing)
                return new SwingData(SwingUseTime, 1.25f, 0f, Main.rand.NextBool());
            else
                return new SwingData();
        }

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.direction = 1; // トレイルが崩れるのを防ぐために方向を固定する
            Owner.armorEffectDrawShadow = true; // プレイヤーの残像の効果
            Owner.SetDummyItemTime(2);
            Owner.AddBuff(BuffID.Featherfall, 10);
            Owner.FlipEffect(Progress * 6f); // フリップエフェクト

            if (GetProgress(type) > 0.5f)
            {
                if (!Projectile.MKProj().Bool[0])
                {
                    Projectile.MKProj().Bool[0] = true;

                    // 葉の発射体をスポーン
                    SpawnLeaf(out Vector2 vel);

                    // トレイルのみの斬撃を発射
                    // トレイルのみにする理由は演出面の問題
                    if (type < TotalSwing - 1)
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int subSwing = ModContent.ProjectileType<GrassKatanaDance2>();
                            Projectile.NewProjectile(Owner.GetSource_ItemUse(item), Owner.MountedCenter, vel, subSwing, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                        }
                    }
                }
            }
        }

        private void SpawnLeaf(out Vector2 vel)
        {
            Owner.ScreenShake(2, 3);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = +0.5f }, Owner.position);

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
        public override void Initialize(Item item, int type)
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