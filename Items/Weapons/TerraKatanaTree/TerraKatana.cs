using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class TerraKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.None;

        public override void SetDefaultsItem()
        {
            Item.width = 62;
            Item.height = 68;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 15;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 22;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 60, true, ModContent.ProjectileType<TerraKatanaSwing>(), 5);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
        }

        public override void ActiveSkill(Player player)
        {
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Terra);
        }
    }
}