using Microsoft.Xna.Framework;
using MoreKatana.Items.Weapons.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
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

            Item.UseSound = SoundID.Item1;

            Item.damage = 14;
            Item.knockBack = 5;
            Item.MKItem().AltDamage = 14;

            Item.value = Item.sellPrice(0, 0, 22, 50);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 60);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            int gem = ModContent.ProjectileType<GemShards_Amethyst>();
            if (player.ownedProjectileCounts[gem] < TotalNumberOfGems)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, gem, Item.damage / 2, 0f, player.whoAmI);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.MaxMana;
            Item.MKItem().ActivateCooldown(player);
            MoreKatanaUtil.DrawRing(player.Center, DustID.GemAmethyst, 24, 10f);
        }

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