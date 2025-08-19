using MoreKatana.Tiles;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MoreKatana.Worlds
{
    public class MoreKatanaWorld : ModSystem
    {
        public static LocalizedText EnchantedKatanaShrineMessage { get; private set; }
        public int samuraiStatueGiftCount;

        public override void SetStaticDefaults()
        {
            EnchantedKatanaShrineMessage = Mod.GetLocalization($"WorldGen.{nameof(EnchantedKatanaShrineMessage)}");
        }

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            samuraiStatueGiftCount = tileCounts[ModContent.TileType<SamuraiStatueGift>()];
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // メモ https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps

            int SurfaceOreandStoneIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Surface Ore and Stone"));
            if (SurfaceOreandStoneIndex != -1)
            {
                if (MoreKatanaConfig.Instance.EnchantedKatanaShrine)
                    tasks.Insert(SurfaceOreandStoneIndex + 1, new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
            }
            //tasks.Add(new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
        }
    }
}