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
    public class ForgottenAltar : GenPass
    {
        public ForgottenAltar(string name, float loadWeight) : base(name, loadWeight)
        {

        }

        public const string StructurePath = "Worlds/ForgottenAltar";

        // これはGenerator.GetStructureDimensions()使ってもいいかもしれない
        public const int StructureWidth = 114;
        public const int StructureHeight = 79;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            // 生成時のメッセージ
            progress.Message = MoreKatanaWorld.ForgottenAltarMessage.Value;

            Point16 point = new Point16(0, 0);
            List<Point16> list = new List<Point16>();

            var placableTiles = new List<ushort>()
            {
                TileID.Dirt,
                TileID.Grass,
                TileID.Sand,
                TileID.SnowBlock,
                TileID.Ebonsand,
                TileID.CorruptGrass,
                TileID.Crimsand,
                TileID.CrimsonGrass
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
            // 空島を回避するために地上レイヤーの少し上からチェックを始める
            int placementPositionY = (int)Main.worldSurface - (Main.maxTilesY / 6);

            // 有効なタイルが見つかるまで位置を調節する
            // 地上レイヤーより上になるようにする
            bool foundValidGround = false;
            int attempts = 0;
            while (!foundValidGround && attempts++ < 100000)
            {
                // 地上レイヤーの位置までY位置を調節する
                if (placementPositionY <= Main.worldSurface)
                {
                    placementPositionY++;
                }

                // 有効なタイルを見つけた場合ループを終了する
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

            // 有効なタイルの位置にストラクチャーを設置
            // ポイントをストラクチャーのサイズに合わせる
            list.Add(new Point16(placementPositionX - (StructureWidth / 2), placementPositionY - (int)(StructureHeight / 1.5f)));
            point = Main.rand.Next(list);
            Generator.GenerateStructure(StructurePath, point, MoreKatana.Instance);

            /* 生成タスクをDirt Rock Wall Runnerにしたのでいらないかも
             * その関係上石ブロックを露出させにくくなったから、ストラクチャーをちょっと手直しする必要がある(かも)
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
            for (int x = pointX; x < pointX + StructureWidth; x++)
            {
                for (int y = pointY - StructureHeight * 2; y < pointY + StructureHeight / 2; y++)
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
            }*/
        }
    }
}