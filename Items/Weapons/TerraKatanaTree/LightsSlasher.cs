using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.Systems.CrossMod;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class LightsSlasher : KatanaItem
    {
        public override KatanaID ID => KatanaID.Lights;

        public const float MaxTeleportDistance = 300f;

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Shadow, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 58;
            Item.height = 68;

            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 25;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 0;

            Item.value = Item.sellPrice(silver: 66);
            Item.rare = ItemRarityID.Blue;

            Item.MKItem().SetKatanaDefaults(Item, 180);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.moveSpeed += 0.35f;
            player.runAcceleration += 0.1f;
            player.maxRunSpeed += 3f;

            for (int i = 0; i < 50; i++)
            {
                Vector2 offset = new Vector2();
                double angle = Main.rand.NextDouble() * 2d * Math.PI;
                offset.X += (float)(Math.Sin(angle) * MaxTeleportDistance);
                offset.Y += (float)(Math.Cos(angle) * MaxTeleportDistance);
                int newDust = Dust.NewDust(player.Center + offset - new Vector2(4, 4), 0, 0, DustID.ManaRegeneration, 0, 0, 100, Color.DarkBlue, 1f);
                Main.dust[newDust].scale = 0.5f;
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity = player.velocity;
            }
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = null;
            Item.MKItem().ActivateCooldown(player);

            player.AddBuff(BuffID.ParryDamageBuff, 60);

            Vector2 vector = player.SafeDirectionTo(player.MKPlayer().MouseWorld);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, vector * MaxTeleportDistance, ModContent.ProjectileType<LightsTeleport>(), Item.MKItem().AltDamage, 0, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.WitherLightning);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 15)
                .AddIngredient(ItemID.DemoniteBar, 16)
                .AddIngredient(ItemID.Shadewood, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}