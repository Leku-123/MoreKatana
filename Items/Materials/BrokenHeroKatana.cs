using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MoreKatana.Items.Materials
{
    public class BrokenHeroKatana : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 44;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 7, 50);
        }
    }
}