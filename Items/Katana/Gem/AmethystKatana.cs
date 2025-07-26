using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.Gem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.Gem
{
    public class AmethystKatana : KatanaItem
    {
        public override KatanaIndex KatanaID => KatanaIndex.AmethystKatana;

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
            Item.value = Item.sellPrice(silver: 22, copper: 50);
            Item.MKItem().SetKatanaDefaults(Item, 0);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            int gem = ModContent.ProjectileType<AmethystShards>();
            if (player.ownedProjectileCounts[gem] < TotalNumberOfGems)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, gem, Item.damage / 2, 0f, player.whoAmI);
        }

        public override void ActiveSkill(Player player)
        {
            //Item.UseSound = MoreKatanaSounds.MuteSound;
            //player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
            //Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(player.direction, 0), ModContent.ProjectileType<ChargingWoodenSwing>(), Item.damage * 5, Item.knockBack * 2f, player.whoAmI);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}