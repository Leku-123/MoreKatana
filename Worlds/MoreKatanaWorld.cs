using StructureHelper.API;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MoreKatana.Worlds
{
    public class MoreKatanaWorld : ModSystem
    {
        public static LocalizedText EnchantedKatanaShrineMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            EnchantedKatanaShrineMessage = Mod.GetLocalization($"WorldGen.{nameof(EnchantedKatanaShrineMessage)}");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // メモ
            // https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps

            int SurfaceOreandStoneIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Surface Ore and Stone"));
            if (SurfaceOreandStoneIndex != -1)
            {
                tasks.Insert(SurfaceOreandStoneIndex + 1, new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
            }
            //tasks.Add(new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
        }
    }

    public class EnchantedKatanaShrine : GenPass
    {
        public EnchantedKatanaShrine(string name, float loadWeight) : base(name, loadWeight)
        {

        }

        public const int StructureWidth = 116;
        public const int StructureHeight = 81;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = MoreKatanaWorld.EnchantedKatanaShrineMessage.Value;
            Point16 point = new Point16(0, 0);
            List<Point16> vs = new List<Point16>();

            bool rightSide = Main.dungeonX - Main.spawnTileX > 0;
            int width = Main.dungeonX + (rightSide ? StructureWidth : -StructureWidth * 2);
            int height = (int)Main.worldSurface;
            for (int x = width; x < width + 16; x++)
            {
                for (int y = height - 300; y < height; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        if (Main.tile[x, y].TileType != TileID.Cloud && Main.tile[x, y].TileType != TileID.RainCloud)
                        {
                            vs.Add(new Point16(x, y - StructureHeight + 25));
                            break;
                        }
                    }
                }
            }

            point = Main.rand.Next(vs);
            Generator.GenerateStructure("Worlds/EnchantedKatanaShrine", point, MoreKatana.Instance);

            var killTiles = new List<ushort>()
            {
                TileID.LivingWood,
                TileID.LeafBlock
            };
            var killWalls = new List<ushort>()
            {
                WallID.DirtUnsafe,
                WallID.LivingWood,
                WallID.LivingWoodUnsafe,
                WallID.LivingLeaf
            };

            int pointX = (int)point.ToVector2().X;
            int pointY = (int)point.ToVector2().Y;
            for (int x = pointX; x < pointX + StructureWidth; x++)
            {
                for (int y = pointY - StructureHeight; y < pointY + StructureHeight - 16; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        foreach (int i in killTiles)
                        {
                            if (Main.tile[x, y].TileType == i)
                                WorldGen.KillTile(x, y);
                        }
                    }
                    foreach (int j in killWalls)
                    {
                        if (Main.tile[x, y].WallType == j)
                            WorldGen.KillWall(x, y);
                    }
                }
            }
        }
    }
}