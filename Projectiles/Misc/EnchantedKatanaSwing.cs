using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MoreKatana.Projectiles.Misc
{
    /*
    public class EnchantedKatanaSwing : CustomSword
    {
        public const float SwingUseTime = 21f;

        public int BeamDamage => SwordItem.MKItem().AltDamage;

        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool AttackPattern(Item item, int type)
        {
            GetEllipse(1.8f, 0.5f);
            SwingStats(SwingUseTime, 1.25f, 0f);z
            return base.AttackPattern(item, type);
        }

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Owner.armorEffectDrawShadow = true; // プレイヤーの残像の効果
            Owner.FlipEffect(progress * 6f); // フリップエフェクト
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, new Vector2(-Owner.direction * SwordItem.shootSpeed, 0f), ModContent.ProjectileType<EnchantedKatanaBeam>(), BeamDamage, SwordItem.knockBack * 2f, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, new Vector2(Owner.direction * SwordItem.shootSpeed, 0f), ModContent.ProjectileType<EnchantedKatanaBeam>(), BeamDamage, SwordItem.knockBack * 2f, Projectile.owner);
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);
    }
    */

    public class EnchantedKatanaSwing : CustomSword
    {
        public int flipCount;
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = Owner.itemAnimationMax * Projectile.MaxUpdates;
            GetTextureValues(this, item);
        }

        public override bool AttackPattern(Item item, int type)
        {
            if (type == 0)
            {
                GetEllipse(1.8f, 0.5f);
                SwingStats(Owner.itemAnimationMax, 1, 0f);
                flipCount = 1;
            }
            else if (type == 1)
            {
                GetEllipse(1f, 1f);
                SwingStats(Owner.itemAnimationMax * 2, 3.25f, 0.25f);
                flipCount = 2;
            }

            return base.AttackPattern(item, type);
        }

        public override void AdditionalAI(Item item, int type, bool onDelay)
        {
            Owner.SetDummyItemTime(2);
            Owner.FlipEffect(progress * 6 * flipCount); // フリップエフェクト

            if (type == 1)
            {
                if (Projectile.soundDelay <= 0)
                {
                    Projectile.soundDelay = 15 * Projectile.MaxUpdates;
                    SoundEngine.PlaySound(SoundID.Item1, Owner.Center);
                }
            }
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);
    }
}
