using MoreKatana.Items.Weapons.Misc;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public class MoreKatanaGlobalItemLootTable : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.GoldenCrate)//金のクレート
            {
                //1/30でエンチャント刀が1本出てくる（幸運の影響を受けない）
                itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(30, ModContent.ItemType<EnchantedKatana>()));
            }
            if (item.type == ItemID.GoldenCrateHard)//チタニウムのクレート
            {
                //1/15でエンチャント刀が1本出てくる（幸運の影響を受けない）
                itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(15, ModContent.ItemType<EnchantedKatana>()));
            }
        }
    }
}
