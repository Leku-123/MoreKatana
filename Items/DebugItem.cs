using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public abstract class DebugItem : ModItem
    {
        public override LocalizedText Tooltip => LocalizedText.Empty;
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        public override void SetStaticDefaults() => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 0;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.MenuTick;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(Mod, "tooltip", MoreKatanaUtil.GetTextValue("Items.DebugItem"));
            line.OverrideColor = Color.Red;
            tooltips.Add(line);
        }
    }
}