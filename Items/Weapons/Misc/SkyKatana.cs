using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Weapons.Misc
{
    public class SkyKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Sky;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 58;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4f;
            //Item.MKItem().AltDamage = 20;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;

            //Item.shoot = ModContent.ProjectileType<EnchantedKatanaBeam>();
            Item.shootSpeed = 9.5f;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

    }
}
