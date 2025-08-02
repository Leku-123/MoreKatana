using Microsoft.Xna.Framework;
using MoreKatana.Buffs;
using MoreKatana.Projectiles.TerraKatanaTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.TerraKatanaTree
{
    public class GrassKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Grass;

        public override void SetDefaultsItem()
        {
            Item.width = 62;
            Item.height = 68;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.UseSound = SoundID.Item1;

            Item.damage = 15;
            Item.knockBack = 4.5f;
            Item.MKItem().AltDamage = 22;

            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ItemRarityID.Orange;

            Item.MKItem().SetKatanaDefaults(Item, 60, true, ModContent.ProjectileType<GrassKatanaSwing>(), 3);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.AddBuff(ModContent.BuffType<PhotosynthesisBuff>(), 2);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = null;
            Vector2 randomVel = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, randomVel, ModContent.ProjectileType<GrassKatanaDance>(), Item.MKItem().AltDamage, Item.knockBack, player.whoAmI);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GrassBlades);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(4))
                target.AddBuff(BuffID.Poisoned, 60 * 7);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RichMahogany, 8)
                .AddIngredient(ItemID.Stinger, 12)
                .AddIngredient(ItemID.JungleSpores, 12)
                .AddIngredient(ItemID.Vine, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}