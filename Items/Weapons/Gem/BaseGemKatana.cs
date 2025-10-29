using Microsoft.Xna.Framework;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Gem
{
    public abstract class BaseGemKatana : KatanaItem
    {
        public readonly int TotalGems;

        public BaseGemKatana(int totalGems)
        {
            TotalGems = totalGems;
        }

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Arcane, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 52;
            Item.height = 60;

            Item.MKItem().AltDamage = Item.damage * 2;
            Item.MKItem().UseSound = SoundID.Item1;
            Item.MKItem().SetKatanaDefaults(Item, 60 * 5);
        }

        public override bool AltFunctionUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] >= TotalGems;

        public override void PassiveSkill(Player player, bool equipment)
        {
            if (player.ownedProjectileCounts[Item.shoot] < TotalGems && player.itemAnimation == 0)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, Item.damage / 2, 0f, player.whoAmI);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;
    }
}