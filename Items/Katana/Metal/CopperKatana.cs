using Terraria;
using Terraria.ID;

namespace MoreKatana.Items.Katana.Metal
{
    public class CopperKatana : KatanaItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 54;

            Item.useTime = 18;
            Item.useAnimation = 18;

            Item.damage = 10;
            Item.knockBack = 6;

            Item.value = Item.sellPrice(silver: 1);

            Item.MKItem().SetKatanaDefaults(Item, 300, true);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {
            player.statDefense += 1;
        }

        public override void ActiveSkill(Player player)
        {

        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 11));
        }

        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 11));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CopperBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}