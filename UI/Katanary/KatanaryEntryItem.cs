using MoreKatana.Items.Katana;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana.UI.Katanary
{
    public class KatanaryEntryItem
    {
        public List<IBestiaryInfoElement> Info { get; private set; }

        public KatanaryEntryItem()
        {
            Info = new List<IBestiaryInfoElement>();
        }

        public static KatanaryEntryItem Katana(int itemType)
        {
            KatanaItem katana = ModContent.GetModItem(itemType) as KatanaItem;
            List<IBestiaryInfoElement> list =
            [
                new KatanaNetIdKatanaryInfoElement(katana.Item.netID),
                new NamePlateInfoElement(Lang.GetItemName(itemType).Key, itemType),
                new NPCPortraitInfoElement(katana.StarRarity),
                new KatanaStatsReportInfoElement(itemType),
                //new NPCKillCounterInfoElement(itemType)
            ];
            if (katana.Item.rare != ItemRarityID.White)
            {
                list.Add(new RareSpawnBestiaryInfoElement(katana.Item.rare));
            }
            IBestiaryUICollectionInfoProvider uIInfoProvider;
            uIInfoProvider = new KatanaUICollectionInfoProvider();
            string key = Lang.GetItemName(katana.Item.netID).Key;
            //key = key.Replace("ItemName.", "");
            string text = "Katanary_FlavorText.item_" + key;
            if (Language.Exists(text))
            {
                list.Add(new FlavorTextBestiaryInfoElement(text));
            }
            return new KatanaryEntryItem
            {
                Icon = new UnlockableNPCEntryIcon(itemType, 0f, 0f, 0f, 0f, null),
                Info = list,
                UIInfoProvider = uIInfoProvider
            };
        }

        public static KatanaryEntryItem Biome(string nameLanguageKey, string texturePath, Func<bool> unlockCondition)
        {
            return new KatanaryEntryItem
            {
                Icon = new CustomEntryIcon(nameLanguageKey, texturePath, unlockCondition),
                Info = new List<IBestiaryInfoElement>()
            };
        }

        public void AddTags(params IBestiaryInfoElement[] elements)
        {
            Info.AddRange(elements);
        }

        public IEntryIcon Icon;

        public IBestiaryUICollectionInfoProvider UIInfoProvider;
    }
}

