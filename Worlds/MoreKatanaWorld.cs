using MoreKatana.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace MoreKatana.Worlds
{
    public class MoreKatanaWorld : ModSystem
    {
        public static LocalizedText ForgottenAltarMessage { get; private set; }
        public static bool seenForgottenAltarBiome = false;
        public int samuraiStatueGiftCount;

        public override void ClearWorld()
        {
            seenForgottenAltarBiome = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (seenForgottenAltarBiome)
                tag["seenForgottenAltar"] = true;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            seenForgottenAltarBiome = tag.ContainsKey("seenForgottenAltar");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.WriteFlags(seenForgottenAltarBiome);
        }

        public override void NetReceive(BinaryReader reader)
        {
            reader.ReadFlags(out seenForgottenAltarBiome);
        }

        public override void SetStaticDefaults()
        {
            ForgottenAltarMessage = Mod.GetLocalization($"WorldGen.{nameof(ForgottenAltarMessage)}");
        }

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            samuraiStatueGiftCount = tileCounts[ModContent.TileType<SamuraiStatueGift>()];
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // メモ https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps

            int DirtRockWallRunnerIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dirt Rock Wall Runner"));
            if (DirtRockWallRunnerIndex != -1)
            {
                if (MoreKatanaConfig.Instance.ForgottenAltar)
                    tasks.Insert(DirtRockWallRunnerIndex + 1, new ForgottenAltar("Forgotten Altar", 237.4298f));
            }
        }
    }
}