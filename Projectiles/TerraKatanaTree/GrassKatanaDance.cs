using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.TerraKatanaTree
{
    public class GrassKatanaDance : CustomSword
    {
        public const float SwingUseTime = 20f;

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = (int)SwingUseTime;
            Projectile.MKProjectile().ActivateCD = true;

            ContinuousSwing = true;
            FixedDirection = true;
            NoSpeedBonus = true;

            GetTextureValues();

            // スイングの向きをランダムにする
            Projectile.velocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);

            // 葉の発射体をスポーン
            SpawnLeaf(out _);
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(1.8f, 0.5f);
            SwingStats(SwingUseTime, 1.25f, 0f, Main.rand.NextBool());
            return type != 4; // 5回振る
        }

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Owner.AddBuff(BuffID.Featherfall, 10);
            Owner.armorEffectDrawShadow = true; // プレイヤーの残像の効果
            Owner.FlipEffect(Progress * 6f); // フリップエフェクト

            if (Progress == 0.5f)
            {
                // 葉の発射体をスポーン
                SpawnLeaf(out Vector2 vel);

                // トレイルのみの斬撃を発射
                // トレイルのみにする理由は演出面の問題
                if (Projectile.owner == Main.myPlayer && type != 4)
                {
                    int subSwing = ModContent.ProjectileType<GrassKatanaDance2>();
                    Projectile.NewProjectile(Owner.GetSource_ItemUse(item), Owner.MountedCenter, vel, subSwing, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                }
            }
        }

        private void SpawnLeaf(out Vector2 vel)
        {
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = +0.5f }, Owner.Center);
            Owner.ScreenShake(2, 3);

            // 発射の向きをランダムにする
            vel = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
            if (Projectile.owner == Main.myPlayer)
            {
                int leaf = ModContent.ProjectileType<GrassLeaf>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, vel * 16f, leaf, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);
    }

    public class GrassKatanaDance2 : CustomSword
    {
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = (int)GrassKatanaDance.SwingUseTime;
            GetTextureValues();
        }

        public override bool SwingPattern(Item item, int type)
        {
            SwingEllipse = new(1.8f, 0.5f);
            SwingStats(GrassKatanaDance.SwingUseTime, 1.25f, 0f, Main.rand.NextBool());
            return base.SwingPattern(item, type);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}