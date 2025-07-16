using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class KatanaryBar : UIElement
    {
        public KatanaryBar(KatanaryDataBase db)
        {
            _db = db;
            _bestiaryBarItems = new List<KatanaryBarItem>();
            RecalculateBars();
        }

        public void RecalculateBars()
        {
            _bestiaryBarItems.Clear();
            int total = _db.Entries.Count;
            int totalCollected = _db.Entries.Count((KatanaryEntryItem e) => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0);
            List<KatanaryBarItem> bestiaryBarItems = _bestiaryBarItems;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Total: ");
            defaultInterpolatedStringHandler.AppendFormatted<float>((float)totalCollected / (float)total * 100f, "N2");
            defaultInterpolatedStringHandler.AppendLiteral("% Collected");
            bestiaryBarItems.Add(new KatanaryBarItem(defaultInterpolatedStringHandler.ToStringAndClear(), total, totalCollected, Main.OurFavoriteColor));
            List<KatanaryEntryItem> items = _db.GetKatanaryEntriesByMod(null);
            int collected = items.Count((KatanaryEntryItem oe) => oe.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0);
            List<KatanaryBarItem> bestiaryBarItems2 = _bestiaryBarItems;
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Terraria: ");
            defaultInterpolatedStringHandler.AppendFormatted<float>((float)collected / (float)items.Count * 100f, "N2");
            defaultInterpolatedStringHandler.AppendLiteral("% Collected");
            bestiaryBarItems2.Add(new KatanaryBar.KatanaryBarItem(defaultInterpolatedStringHandler.ToStringAndClear(), items.Count, collected, _colors[0]));
            for (int i = 1; i < ModLoader.Mods.Length; i++)
            {
                items = _db.GetKatanaryEntriesByMod(ModLoader.Mods[i]);
                if (items != null)
                {
                    collected = items.Count((KatanaryEntryItem oe) => oe.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0);
                    List<KatanaryBarItem> bestiaryBarItems3 = _bestiaryBarItems;
                    defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
                    defaultInterpolatedStringHandler.AppendFormatted(ModLoader.Mods[i].DisplayName);
                    defaultInterpolatedStringHandler.AppendLiteral(": ");
                    defaultInterpolatedStringHandler.AppendFormatted<float>((float)collected / (float)items.Count * 100f, "N2");
                    defaultInterpolatedStringHandler.AppendLiteral("% Collected");
                    bestiaryBarItems3.Add(new KatanaryBarItem(defaultInterpolatedStringHandler.ToStringAndClear(), items.Count, collected, _colors[i % _colors.Length]));
                }
            }
        }

        // Token: 0x060028FF RID: 10495 RVA: 0x0051081C File Offset: 0x0050EA1C
        protected override void DrawSelf(SpriteBatch sb)
        {
            int xOffset = 0;
            Rectangle rectangle = base.GetDimensions().ToRectangle();
            rectangle.Height -= 3;
            bool drawHover = false;
            KatanaryBarItem hoverData = null;
            for (int i = 1; i < _bestiaryBarItems.Count; i++)
            {
                KatanaryBarItem barData = _bestiaryBarItems[i];
                int offset = (int)((float)rectangle.Width * ((float)barData.EntryCount / (float)_db.Entries.Count));
                if (i == _bestiaryBarItems.Count - 1)
                {
                    offset = rectangle.Width - xOffset;
                }
                int width = (int)((float)offset * ((float)barData.CompletedCount / (float)barData.EntryCount));
                Rectangle drawArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, width, rectangle.Height);
                Rectangle outlineArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, offset, rectangle.Height);
                xOffset += offset;
                sb.Draw(TextureAssets.MagicPixel.Value, outlineArea, barData.DrawColor * 0.3f);
                sb.Draw(TextureAssets.MagicPixel.Value, drawArea, barData.DrawColor);
                if (!drawHover && outlineArea.Contains(new Point(Main.mouseX, Main.mouseY)))
                {
                    drawHover = true;
                    hoverData = barData;
                }
            }
            KatanaryBarItem bottomData = _bestiaryBarItems[0];
            int bottomWidth = (int)((float)rectangle.Width * ((float)bottomData.CompletedCount / (float)bottomData.EntryCount));
            Rectangle bottomDrawArea = new Rectangle(rectangle.X, rectangle.Bottom, bottomWidth, 3);
            Rectangle bottomOutlineArea = new Rectangle(rectangle.X, rectangle.Bottom, rectangle.Width, 3);
            sb.Draw(TextureAssets.MagicPixel.Value, bottomOutlineArea, bottomData.DrawColor * 0.3f);
            sb.Draw(TextureAssets.MagicPixel.Value, bottomDrawArea, bottomData.DrawColor);
            if (!drawHover && bottomOutlineArea.Contains(new Point(Main.mouseX, Main.mouseY)))
            {
                drawHover = true;
                hoverData = bottomData;
            }
            if (drawHover && hoverData != null)
            {
                Main.instance.MouseText(hoverData.Tooltop, 0, 0, -1, -1, -1, -1, 0);
            }
        }

        // Token: 0x040019FE RID: 6654
        private KatanaryDataBase _db;

        // Token: 0x040019FF RID: 6655
        private List<KatanaryBarItem> _bestiaryBarItems;

        // Token: 0x04001A00 RID: 6656
        private readonly Color[] _colors = new Color[]
        {
            new Color(232, 76, 61),
            new Color(155, 88, 181),
            new Color(27, 188, 155),
            new Color(243, 156, 17),
            new Color(45, 204, 112),
            new Color(241, 196, 15)
        };

        // Token: 0x020009EF RID: 2543
        private class KatanaryBarItem
        {
            // Token: 0x06005718 RID: 22296 RVA: 0x0069C99C File Offset: 0x0069AB9C
            public KatanaryBarItem(string tooltop, int entryCount, int completedCount, Color drawColor)
            {
                Tooltop = tooltop;
                EntryCount = entryCount;
                CompletedCount = completedCount;
                DrawColor = drawColor;
            }

            // Token: 0x04006BCD RID: 27597
            internal readonly string Tooltop;

            // Token: 0x04006BCE RID: 27598
            internal readonly int EntryCount;

            // Token: 0x04006BCF RID: 27599
            internal readonly int CompletedCount;

            // Token: 0x04006BD0 RID: 27600
            internal readonly Color DrawColor;
        }
    }
}
