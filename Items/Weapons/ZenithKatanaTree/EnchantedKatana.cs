using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.ZenithKatanaTree;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.ZenithKatanaTree
{
    public class EnchantedKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Enchanted;

        public override void SetDefaultsItem()
        {
            Item.width = 48;
            Item.height = 54;

            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4.25f;
            Item.MKItem().AltDamage = 20;

            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;

            Item.shootSpeed = 9.5f;

            Item.MKItem().SetKatanaDefaults(Item, 60, type: ModContent.ProjectileType<EnchantedKatanaSwing>());
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.MaxMana;
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);

            int[] dustType = [DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink];
            MoreKatanaUtil.DrawRing(player.Center, dustType, 30, 6f, dustSize: 2f);
            MoreKatanaUtil.DrawRing(player.Center, dustType, 27, 8f, dustSize: 2);
            MoreKatanaUtil.DrawRing(player.Center, dustType, 26, 10f, dustSize: 3f);

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new(player.direction, 0f), ModContent.ProjectileType<EnchantedKatanaSwingActiveSkill>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            velocity = new(player.direction, 0);
        }
    }
}
