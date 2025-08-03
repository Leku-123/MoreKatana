using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class VolcanoKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Volcano;

        public override void SetDefaultsItem()
        {
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.width = 54;
            Item.height = 64;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.UseSound = SoundID.Item1;
            Item.damage = 40;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            //WIP 仮で発射
            Item.shoot = ProjectileID.BallofFire;
            Item.shootSpeed = 10f;
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Frostburn2] = true;
            player.buffImmune[BuffID.Frozen] = true;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 2; i++)
            {
                int moyasu = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 2.5f);
                Main.dust[moyasu].noGravity = true;
                Main.dust[moyasu].velocity.X *= 2f;
                Main.dust[moyasu].velocity.Y *= 2f;
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
                target.AddBuff(BuffID.OnFire, 60 * 3);

            if (hit.Damage >= 1)
                if (player.ZoneSnow)
                    hit.Damage *= 2;

            if (player.ItemAnimationJustStarted && Main.myPlayer == Item.whoAmI && (target == null || target.HittableForOnHitRewards()))
            {
                Vector2 center = target.Center;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), center.X, center.Y, 0f, -1f * player.gravDir, ProjectileID.Volcano, Item.damage, Item.knockBack, player.whoAmI, 0f, 2f, 0f);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
}
