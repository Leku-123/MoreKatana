using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using MoreKatana.Systems.CrossMod;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class VolcanoKatana : KatanaItem
    {
        private bool Bomber = true;

        private readonly int[] VolcanoBuffImmune = [BuffID.Frostburn, BuffID.Chilled, BuffID.Frozen, BuffID.Frostburn2];

        public static Color FireColor(byte alpha = 255) => Color.OrangeRed with { A = alpha };

        public override void SetStaticDefaults()
        {
            Item.AddElement(RedemptionCompat.Fire, true);
            Item.SetSlashBonus();
        }

        public override void SetDefaultsItem()
        {
            Item.width = 56;
            Item.height = 70;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.MKItem().UseSound = SoundID.Item1;

            Item.damage = 40;
            Item.knockBack = 6.5f;
            Item.MKItem().AltDamage = 40;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            foreach (int i in VolcanoBuffImmune)
            {
                player.buffImmune[i] = true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
            => !player.IsUsingAlt();

        public override void ActiveSkill(Player player)
        {
            // プレイヤーの向きをマウスの方向に向けて、その方向にホールド発射体をスポーンさせる
            player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, -Vector2.UnitY, ModContent.ProjectileType<VolcanoKatanaHoldout>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                int newDust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[newDust].noGravity = true;
                Main.dust[newDust].velocity.X *= 2f;
                Main.dust[newDust].velocity.Y *= 2f;
            }
        }

        public override bool CanUseItem(Player player) => Bomber = base.CanUseItem(player);

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (player.ZoneSnow)
                modifiers.SourceDamage *= 1.5f;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
                target.AddBuff(BuffID.OnFire, 60 * 3);

            if (Bomber && Main.myPlayer == player.whoAmI && (target == null || target.HittableForOnHitRewards()))
            {
                Vector2 center = target.Center;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), center.X, center.Y, 0f, -1f * player.gravDir, ProjectileID.Volcano, Item.damage, Item.knockBack, player.whoAmI);
                Bomber = false;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}