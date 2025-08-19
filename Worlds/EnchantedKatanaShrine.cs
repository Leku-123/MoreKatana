using StructureHelper.API;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace MoreKatana.Worlds
{
    /// <summary>
    /// ストラクチャーの生成パス
    /// </summary>
    public class EnchantedKatanaShrine : GenPass
    {
        public EnchantedKatanaShrine(string name, float loadWeight) : base(name, loadWeight)
        {

        }

        public const string StructurePath = "Worlds/EnchantedKatanaShrine";

        // これはGenerator.GetStructureDimensions()使ってもいいかもしれない
        public const int StructureWidth = 114;
        public const int StructureHeight = 79;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = MoreKatanaWorld.EnchantedKatanaShrineMessage.Value;

            Point16 point = new Point16(0, 0);
            List<Point16> vs = new List<Point16>();

            var placableTiles = new List<ushort>()
            {
                TileID.Dirt,
                TileID.Grass,
                TileID.Sand,
                TileID.SnowBlock,
                TileID.Ebonsand,
                TileID.Crimsand
            };

            // ダンジョンの方向
            int dungeonDirection = Main.dungeonX - (Main.maxTilesX / 2) > 0 ? 1 : -1;

            // ワールドのサイズごと倍率の値を整理する
            float worldSizeValue = Main.maxTilesX <= 4200 ? 1.2f : Main.maxTilesX <= 6400 ? 1.5f : 1.8f;

            // ストラクチャを設置するX位置
            // ダンジョンのX位置から外側へ、ワールドサイズを考慮して移動した位置
            int placementX = Main.dungeonX + (int)(StructureWidth * worldSizeValue * dungeonDirection);
            int placementPositionX = WorldGen.genRand.Next(placementX, placementX + 8);

            // ストラクチャを設置するY位置
            // 本当は Main.spawnTileY を使いたくないので、今後 Main.worldSurface や GenVars の何かに変更したい
            // とにかく空島を回避する何かが必要だ
            int placementPositionY = (int)(Main.spawnTileY - (Main.maxTilesY / 10f));

            // 有効なタイルが見つかるまで位置を調節する
            // 少なくとも地上レイヤー以上になるようにする
            bool foundValidGround = false;
            int attempts = 0;
            while (!foundValidGround && attempts++ < 100000)
            {
                while (!WorldGen.SolidTile(placementPositionX, placementPositionY) && placementPositionY <= Main.worldSurface)
                {
                    placementPositionY++;
                }

                if (Main.tile[placementPositionX, placementPositionY].HasTile)
                {
                    foreach (int i in placableTiles)
                    {
                        if (Main.tile[placementPositionX, placementPositionY].TileType == i)
                        {
                            foundValidGround = true;
                        }
                    }
                }
            }

            // ポイントの位置にストラクチャを設置
            vs.Add(new Point16(placementPositionX - (StructureWidth / 2), placementPositionY - (int)(StructureHeight / 1.5f)));
            point = Main.rand.Next(vs);
            Generator.GenerateStructure(StructurePath, point, MoreKatana.Instance);

            int pointX = (int)point.ToVector2().X;
            int pointY = (int)point.ToVector2().Y;

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

            // 余分なタイルを消す
            // ストラクチャで上書きしているが改めてチェックする
            // リビングウッドを考慮してY方向にはタイルチェックを多く行う
            for (int x = pointX - StructureWidth / 2; x < pointX + StructureWidth / 2; x++)
            {
                for (int y = pointY - StructureHeight / 2; y < pointY + StructureHeight; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        foreach (int i in killTiles)
                        {
                            if (Main.tile[x, y].TileType == i)
                            {
                                WorldGen.KillTile(x, y);
                            }
                        }
                    }
                    foreach (int j in killWalls)
                    {
                        if (Main.tile[x, y].WallType == j)
                        {
                            WorldGen.KillWall(x, y);
                        }
                    }
                }
            }
        }
    }
}