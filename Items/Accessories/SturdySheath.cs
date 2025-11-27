using Microsoft.Xna.Framework;
using MoreKatana.Particles;
using Terraria;
using Terraria.Localization;

namespace MoreKatana.Items.Accessories
{
    public class SturdySheath : ArtifactItem
    {
        public readonly int CDReductionValue = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CDReductionValue);

        public override void SetDefaultsItem()
        {
            Item.width = 22;
            Item.height = 22;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.MKPlayer().ActiveSkillCD == player.MKPlayer().ActiveSkillCDMax)
            {
                if (player.MKPlayer().ActiveSkillCDMax == 1)
                    return;

                player.MKPlayer().ActiveSkillCD -= player.MKPlayer().ActiveSkillCD / CDReductionValue;

                Particle glowSpark = new GlowSparkParticle(player.Center, new Vector2(0.1f, 0.1f).RotatedByRandom(100), false, 15, Main.rand.NextFloat(0.05f, 0.09f), Main.rand.NextBool() ? Color.Silver : Color.DimGray, new Vector2(2f, 0.5f), true);
                ParticleHandler.SpawnParticle(glowSpark);
            }
        }
    }
}