using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class UIKatanaryEntryGrid : UIElement
    {
        public event Action OnGridContentsChanged;

        public UIKatanaryEntryGrid(List<KatanaryEntryItem> workingSet, MouseEvent clickOnEntryEvent)
        {
            Width = new StyleDimension(0f, 1f);
            Height = new StyleDimension(0f, 1f);
            _workingSetEntries = workingSet;
            _clickOnEntryEvent = clickOnEntryEvent;
            base.SetPadding(0f);
            UpdateEntries();
            FillBestiarySpaceWithEntries();
        }

        public void UpdateEntries()
        {
            _lastEntry = _workingSetEntries.Count;
        }

        public void FillBestiarySpaceWithEntries()
        {
            base.RemoveAllChildren();
            UpdateEntries();
            int maxEntriesWidth;
            int maxEntriesHeight;
            int maxEntriesToHave;
            GetEntriesToShow(out maxEntriesWidth, out maxEntriesHeight, out maxEntriesToHave);
            FixBestiaryRange(0, maxEntriesToHave);
            int atEntryIndex = _atEntryIndex;
            int num = Math.Min(_lastEntry, atEntryIndex + maxEntriesToHave);
            List<KatanaryEntryItem> list = new();
            for (int i = atEntryIndex; i < num; i++)
            {
                list.Add(_workingSetEntries[i]);
            }
            int num2 = 0;
            float num3 = 0.5f / (float)maxEntriesWidth;
            float num4 = 0.5f / (float)maxEntriesHeight;
            for (int j = 0; j < maxEntriesHeight; j++)
            {
                int k = 0;
                while (k < maxEntriesWidth && num2 < list.Count)
                {
                    UIElement uIElement = new UIKatanaryEntryButton(list[num2], false);
                    num2++;
                    uIElement.OnLeftClick += _clickOnEntryEvent;
                    uIElement.VAlign = (uIElement.HAlign = 0.5f);
                    uIElement.Left.Set(0f, (float)k / (float)maxEntriesWidth - 0.5f + num3);
                    uIElement.Top.Set(0f, (float)j / (float)maxEntriesHeight - 0.5f + num4);
                    uIElement.SetSnapPoint("Entries", num2, new Vector2?(new Vector2(0.2f, 0.7f)), null);
                    base.Append(uIElement);
                    k++;
                }
            }
        }


        public override void Recalculate()
        {
            base.Recalculate();
            FillBestiarySpaceWithEntries();
        }


        public void GetEntriesToShow(out int maxEntriesWidth, out int maxEntriesHeight, out int maxEntriesToHave)
        {
            Rectangle rectangle = base.GetDimensions().ToRectangle();
            maxEntriesWidth = rectangle.Width / 72;
            maxEntriesHeight = rectangle.Height / 72;
            int num = 0;
            maxEntriesToHave = maxEntriesWidth * maxEntriesHeight - num;
        }

        public string GetRangeText()
        {
            int num3;
            int num4;
            int maxEntriesToHave;
            GetEntriesToShow(out num3, out num4, out maxEntriesToHave);
            int atEntryIndex = _atEntryIndex;
            int num = Math.Min(_lastEntry, atEntryIndex + maxEntriesToHave);
            int num2 = Math.Min(atEntryIndex + 1, num);
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 3);
            defaultInterpolatedStringHandler.AppendFormatted<int>(num2);
            defaultInterpolatedStringHandler.AppendLiteral("-");
            defaultInterpolatedStringHandler.AppendFormatted<int>(num);
            defaultInterpolatedStringHandler.AppendLiteral(" (");
            defaultInterpolatedStringHandler.AppendFormatted<int>(_lastEntry);
            defaultInterpolatedStringHandler.AppendLiteral(")");
            return defaultInterpolatedStringHandler.ToStringAndClear();
        }

        public void MakeButtonGoByOffset(UIElement element, int howManyPages)
        {
            element.OnLeftClick += delegate
            {
                OffsetLibraryByPages(howManyPages);
            };
        }

        public void OffsetLibraryByPages(int howManyPages)
        {
            int num;
            int num2;
            int maxEntriesToHave;
            GetEntriesToShow(out num, out num2, out maxEntriesToHave);
            OffsetLibrary(howManyPages * maxEntriesToHave);
        }


        public void OffsetLibrary(int offset)
        {
            int num;
            int num2;
            int maxEntriesToHave;
            GetEntriesToShow(out num, out num2, out maxEntriesToHave);
            FixBestiaryRange(offset, maxEntriesToHave);
            FillBestiarySpaceWithEntries();
        }

        private void FixBestiaryRange(int offset, int maxEntriesToHave)
        {
            _atEntryIndex = Utils.Clamp<int>(_atEntryIndex + offset, 0, Math.Max(0, _lastEntry - maxEntriesToHave));
            if (OnGridContentsChanged != null)
            {
                OnGridContentsChanged();
            }
        }

        private List<KatanaryEntryItem> _workingSetEntries;


        private MouseEvent _clickOnEntryEvent;

        private int _atEntryIndex;

        private int _lastEntry;
    }
}
