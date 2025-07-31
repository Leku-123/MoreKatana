using Microsoft.Xna.Framework;
using MoreKatana.Projectiles.TerraKatanaTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Katana.TerraKatanaTree
{
    public class SacredNaginata : KatanaItem
    {
        public override KatanaID ID => KatanaID.Hallowed;

        public override void SetDefaultsItem()
        {
            Item.width = 70;
            Item.height = 80;

            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.UseSound = SoundID.Item169;

            Item.damage = 45;
            Item.knockBack = 4.5f;

            Item.value = Item.sellPrice(0, 4, 60);
            Item.rare = ItemRarityID.Pink;

            Item.MKItem().SetKatanaDefaults(Item, 0, false, ModContent.ProjectileType<SacredNaginataSwing>(), 3);
        }

        public override void PassiveSkill(Player player, bool equipment)
        {

        }

        public override void ActiveSkill(Player player)
        {

        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.HallowedWeapons);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}