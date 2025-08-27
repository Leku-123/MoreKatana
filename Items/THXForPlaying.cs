using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public class THXForPlaying : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Blue;
        }

        public override bool? UseItem(Player player)
        {
            // 仮
            Utils.OpenToURL("https://terrariamods.wiki.gg/wiki/TGSMoreKatana");
            return true;
        }
    }
}