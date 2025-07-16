using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.UI.Katanary
{
    public class KatanaryDataBase
    {
        public List<KatanaryEntryItem> Entries
        {
            get
            {
                return _entries;
            }
        }

        public List<IBestiaryEntryFilter> Filters
        {
            get
            {
                return _filters;
            }
        }

        public List<IBestiarySortStep> SortSteps
        {
            get
            {
                return _sortSteps;
            }
        }

        public KatanaryEntryItem Register(KatanaryEntryItem entry)
        {
            _entries.Add(entry);
            for (int i = 0; i < entry.Info.Count; i++)
            {
                KatanaNetIdKatanaryInfoElement nPCNetIdBestiaryInfoElement = entry.Info[i] as KatanaNetIdKatanaryInfoElement;
                if (nPCNetIdBestiaryInfoElement != null)
                {
                    _byKatanaId[nPCNetIdBestiaryInfoElement.NetId] = entry;
                }
            }
            ModItem modItem = ContentSamples.ItemsByType[((NPCNetIdBestiaryInfoElement)entry.Info[0]).NetId].ModItem;
            Mod mod = ((modItem != null) ? modItem.Mod : null);
            if (mod == null)
            {
                _vanillaEntries.Add(entry);
            }
            else if (_byMod.ContainsKey(mod))
            {
                _byMod[mod].Add(entry);
            }
            else
            {
                _byMod.Add(mod, new List<KatanaryEntryItem> { entry });
            }
            return entry;
        }

        public IBestiaryEntryFilter Register(IBestiaryEntryFilter filter)
        {
            _filters.Add(filter);
            return filter;
        }

        public IBestiarySortStep Register(IBestiarySortStep sortStep)
        {
            _sortSteps.Add(sortStep);
            return sortStep;
        }

        public KatanaryEntryItem FindEntryByKatanaID(int katanaNetID)
        {
            KatanaryEntryItem value;
            if (_byKatanaId.TryGetValue(katanaNetID, out value))
            {
                return value;
            }
            _trashEntry.Info.Clear();
            return _trashEntry;
        }


        public void ApplyPass(KatanaryEntriesPass pass)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                pass(_entries[i]);
            }
        }

        public List<KatanaryEntryItem> GetKatanaryEntriesByMod(Mod mod)
        {
            if (mod == null)
            {
                return _vanillaEntries;
            }
            List<KatanaryEntryItem> value;
            _byMod.TryGetValue(mod, out value);
            return value;
        }


        public float GetCompletedPercentByMod(Mod mod)
        {
            if (mod == null)
            {
                return (float)_vanillaEntries.Count((KatanaryEntryItem e) => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)_vanillaEntries.Count;
            }
            List<KatanaryEntryItem> value;
            if (_byMod.TryGetValue(mod, out value))
            {
                return (float)value.Count((KatanaryEntryItem e) => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)value.Count;
            }
            return -1f;
        }


        private List<KatanaryEntryItem> _entries = new();


        private List<IBestiaryEntryFilter> _filters = new List<IBestiaryEntryFilter>();


        private List<IBestiarySortStep> _sortSteps = new List<IBestiarySortStep>();


        private Dictionary<int, KatanaryEntryItem> _byKatanaId = new();


        private KatanaryEntryItem _trashEntry = new();


        private List<KatanaryEntryItem> _vanillaEntries = new();


        private Dictionary<Mod, List<KatanaryEntryItem>> _byMod = new();

        public delegate void KatanaryEntriesPass(KatanaryEntryItem entry);
    }
}

