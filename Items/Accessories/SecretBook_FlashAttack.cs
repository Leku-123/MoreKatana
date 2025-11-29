using MoreKatana.Items.Weapons.TerraKatanaTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items.Accessories
{
    public class SecretBook_FlashAttack : ArtifactItem
    {
        public override void SetDefaultsItem()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.sellPrice(silver: 90);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.MKPlayer().FlashAttackDoubleTap = true;
        }

        public static int[] DisabledItem = [
            ModContent.ItemType<SacredNaginata>(),
            ModContent.ItemType<TrueSacredNaginata>()
            ];

        public static bool ValidItem(Item item)
        {
            foreach (int i in DisabledItem)
            {
                if (item.type == i)
                    return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book)
                .AddIngredient(ItemID.SoulofLight, 6)
                .AddIngredient(ItemID.SoulofNight, 6)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}