using MoreKatana.Projectiles.Base;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
/*
namespace MoreKatana.Projectiles.Misc
{
    public class EnchantedKatanaSwingActiveSkill : CustomSword
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
            GetEllipse(1.5f, 1.5f);
            SwingStats(40, 3.25f, 0.25f);
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
            MoreKatanaUtil.ProjectileSplitInAllDirections(Projectile.GetSource_FromThis(), Owner.MountedCenter, SwordItem.shootSpeed, 8, ModContent.ProjectileType<EnchantedKatanaBeam>(), BeamDamage, SwordItem.knockBack * 2f, Projectile.owner, ai2: 1f);
        }

        public override void OnKill(int timeLeft) => Owner.FlipEffect(0);
    }
}
*/