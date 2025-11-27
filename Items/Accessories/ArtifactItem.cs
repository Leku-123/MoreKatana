using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.UI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace MoreKatana.Items.Accessories
{
    public abstract class ArtifactItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public sealed override void SetDefaults()
        {
            Item.accessory = true;
            SetDefaultsItem();
        }

        public virtual void SetDefaultsItem()
        {

        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            if ((equippedItem.ModItem is ArtifactItem) && (incomingItem.ModItem is ArtifactItem))
                return false;
            return true;
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            return modded && slot == AccessorySystem.KatanaSlots;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int index = tooltips.FindIndex(x => x.Name == "Tooltip0");
            if (index < 0)
                return;

            TooltipLine line = new TooltipLine(Mod, "Artifact", MoreKatanaUtil.GetTextValue("Items.ArtifactItem"));
            tooltips.Insert(index + 1, line);
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "Artifact")
            {
                Vector2 lineposition = new Vector2(line.OriginalX, line.OriginalY);
                Utils.DrawBorderString(Main.spriteBatch, line.Text, lineposition, Color.LightGoldenrodYellow);
                Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                for (int i = 0; i < 4; i++)
                {
                    float amount = 2f;
                    if (MoreKatanaUtil.IsJapanese(line.Text))
                        amount = 1.4f;

                    Vector2 drawpos = lineposition + new Vector2(0, amount * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                    Utils.DrawBorderString(Main.spriteBatch, line.Text, drawpos, Color.Goldenrod);
                }
                Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                return false;
            }
            return base.PreDrawTooltipLine(line, ref yOffset);
        }
    }
}