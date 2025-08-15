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
            DrawRingMix(player.Center, 30, 6f, default, 2f, false, DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink);
            DrawRingMix(player.Center, 27, 8f, default, 2.5f, false, DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink);
            DrawRingMix(player.Center, 26, 10f, default, 3f, false, DustID.MagicMirror, DustID.Enchanted_Gold, DustID.Enchanted_Pink);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new(player.direction, 0f), ModContent.ProjectileType<EnchantedKatanaSwingActiveSkill>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            velocity = new(player.direction, 0);
        }

        /// <summary>
        /// リング状にダストを混ぜてスポーンする
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="density"></param>
        /// <param name="speed"></param>
        /// <param name="color"></param>
        /// <param name="dustSize"></param>
        /// <param name="noLight"></param>
        public static void DrawRingMix(Vector2 position, int density, float speed, Color color = default, float dustSize = 1f, bool noLight = false, params int[] overrideTypes)
        {
            for (int i = 0; i < density; i++)
            {
                if (overrideTypes.Length <= 0) return;

                Vector2 velocity = speed * Vector2.UnitY.RotatedBy(MathHelper.TwoPi / density * i);
                int d = Dust.NewDust(position, 0, 0, overrideTypes.ElementAt(Main.rand.Next(0, overrideTypes.Length)), newColor: color);
                Main.dust[d].noLight = noLight;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = velocity;
                Main.dust[d].scale = dustSize;
            }
        }
    }
}
