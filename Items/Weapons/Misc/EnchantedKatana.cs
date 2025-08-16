using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Misc;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Misc
{
    public class EnchantedKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Enchanted;

        public static int[] EnchantedDustType = [DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink];

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

            Item.shoot = ModContent.ProjectileType<EnchantedKatanaBeam>();
            Item.shootSpeed = 9.5f;

            Item.MKItem().SetKatanaDefaults(Item, 60, type: ModContent.ProjectileType<EnchantedKatanaSwing>());
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.MaxMana;
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);

            for (int i = 0; i < 3; i++)
            {
                float speed = 6f + (2 * i);
                float scale = 1f + (0.5f * i);
                MoreKatanaUtil.DrawRing(player.Center, EnchantedDustType, 24, speed, dustScale: scale);
            }

            var source = player.GetSource_ItemUse(Item);
            Projectile.NewProjectile(source, player.Center, new Vector2(player.direction, 0f), Item.MKItem().SwingType, Item.MKItem().AltDamage, Item.knockBack, player.whoAmI, 1f);
            MoreKatanaUtil.ProjectileSplitInAllDirections(source, player.MountedCenter, Item.shootSpeed, 8, Item.shoot, Item.MKItem().AltDamage, Item.knockBack, player.whoAmI, 1f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            velocity = new Vector2(player.direction, 0) * Item.shootSpeed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!player.IsUsingAlt())
            {
                Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage / 2, 0f, player.whoAmI);
                Projectile.NewProjectile(source, player.MountedCenter, -velocity, type, damage / 2, 0f, player.whoAmI);
            }
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                int newDust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Utils.SelectRandom(Main.rand, EnchantedDustType));
                Main.dust[newDust].noGravity = true;
            }
        }
    }
}
