using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class NightKatana : KatanaItem
    {

        private int hitNPCWhoAmI = -1;
        private int hitCount = 0;

        public override void SetDefaultsItem()
        {
            Item.width = 60;
            Item.height = 70;

            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 40;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 50;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Blue;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            int useSpeedBoost = 25 - hitCount / 3;
            Item.defense = 1 * hitCount;
            Item.useTime = useSpeedBoost;
            Item.useAnimation = useSpeedBoost;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.whoAmI != hitNPCWhoAmI)
            {
                hitNPCWhoAmI = target.whoAmI;
                hitCount = 1;
                return;
            }

            hitCount++;
        }
    }
}
