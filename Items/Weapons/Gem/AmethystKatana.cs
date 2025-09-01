using Microsoft.Xna.Framework;
using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Weapons.Gem
{
    public class AmethystKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Amethyst;

        public const int TotalNumberOfGems = 3;

        public override void SetDefaultsItem()
        {
            Item.width = 46;
            Item.height = 48;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.UseSound = MoreKatanaSounds.SwordSlash;

            Item.damage = 14;
            Item.knockBack = 5;
            Item.MKItem().AltDamage = 14;

            Item.value = Item.sellPrice(0, 0, 22, 50);
            Item.rare = ItemRarityID.White;

            Item.shoot = ModContent.ProjectileType<GemShards_Amethyst>();

            Item.MKItem().SetKatanaDefaults(Item, 60 * 5);
        }

        public override bool AltFunctionUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] >= TotalNumberOfGems;

        public override void PassiveSkill(Player player, bool equipment)
        {
            if (player.ownedProjectileCounts[Item.shoot] < TotalNumberOfGems && player.itemAnimation == 0)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, Item.shoot, Item.damage / 2, 0f, player.whoAmI);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CopperKatana>())
                .AddIngredient(ItemID.Amethyst, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TinKatana>())
                .AddIngredient(ItemID.Amethyst, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}