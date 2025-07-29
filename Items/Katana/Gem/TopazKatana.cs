using Microsoft.Xna.Framework;
using MoreKatana.Items.Katana.Metal;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.Gem
{
    public class TopazKatana : KatanaItem
    {
        public override KatanaID ID => KatanaID.Topaz;

        public const int TotalNumberOfGems = 3;

        public override void SetDefaultsItem()
        {
            Item.width = 46;
            Item.height = 48;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.UseSound = SoundID.Item1;

            Item.damage = 15;
            Item.knockBack = 5;

            Item.value = Item.sellPrice(0, 0, 45, 0);
            Item.rare = ItemRarityID.White;

            Item.MKItem().SetKatanaDefaults(Item, 0);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            int gem = ModContent.ProjectileType<GemShards_Topaz>();
            if (player.ownedProjectileCounts[gem] < TotalNumberOfGems)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, gem, Item.damage / 2, 0f, player.whoAmI);
        }

        public override void ActiveSkill(Player player)
        {
            Item.UseSound = SoundID.MaxMana;
            Item.MKItem().ActivateCooldown(player);
            MoreKatanaUtil.DrawRing(player.Center, DustID.GemTopaz, 24, 10f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CopperKatana>())
                .AddIngredient(ItemID.Topaz, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TinKatana>())
                .AddIngredient(ItemID.Topaz, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}