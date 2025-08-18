using Microsoft.Xna.Framework;
using MoreKatana.Tiles;
using StructureHelper.API;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
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
            // メモ
            // https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps

            int SurfaceOreandStoneIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Surface Ore and Stone"));
            if (SurfaceOreandStoneIndex != -1)
            {
                if (MoreKatanaConfig.Instance.EnchantedKatanaShrine)
                    tasks.Insert(SurfaceOreandStoneIndex + 1, new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
            }
            //tasks.Add(new EnchantedKatanaShrine("Enchanted Katana Shrine", 237.4298f));
        }
    }

    /// <summary>
    /// ストラクチャーの生成パス
    /// </summary>
    public class EnchantedKatanaShrine : GenPass
    {
        public EnchantedKatanaShrine(string name, float loadWeight) : base(name, loadWeight)
        {

        }

        public const string StructurePath = "Worlds/EnchantedKatanaShrine";

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = MoreKatanaWorld.EnchantedKatanaShrineMessage.Value;

            Point16 point = new Point16(0, 0);
            List<Point16> vs = new List<Point16>();

            Point16 dimensions = Generator.GetStructureDimensions(StructurePath, MoreKatana.Instance);
            int structureWidth = dimensions.X;
            int structureHeight = dimensions.Y;

            bool rightSide = Main.dungeonX - Main.spawnTileX > 0;
            int width = Main.dungeonX + (rightSide ? structureWidth : -structureWidth * 2);
            int height = (int)Main.worldSurface;
            for (int x = width - 8; x < width + 8; x++)
            {
                for (int y = height - 250; y < height; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        if (Main.tile[x, y].TileType != TileID.Cloud && Main.tile[x, y].TileType != TileID.RainCloud)
                        {
                            vs.Add(new Point16(x, y - structureHeight + 25));
                            break;
                        }
                    }
                }
            }

            point = Main.rand.Next(vs);
            Generator.GenerateStructure(StructurePath, point, MoreKatana.Instance);

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
            for (int x = pointX; x < pointX + structureWidth; x++)
            {
                for (int y = pointY - structureHeight; y < pointY + structureHeight - 16; y++)
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

    /// <summary>
    /// ストラクチャーのバイオーム
    /// </summary>
    public class EnchantedKatanaShrineBiome : ModBiome, ICameraModifier
    {
        #region CameraModifier
        private const int FramesToLast = 60;
        private int FramesElapsed;

        public string UniqueIdentity { get; private set; }
        public bool Finished { get; private set; }

        public void Update(ref CameraInfo cameraInfo)
        {
            Player player = Main.LocalPlayer;

            UniqueIdentity = "EnchantedKatanaShrine";

            float progress = Utils.GetLerpValue(0, FramesToLast, FramesElapsed);

            float lerpAmount = progress switch
            {
                < 0.5f => Utils.Remap(progress, 0, 0.5f, 0, 1),
                > 0.5f => Utils.Remap(progress, 0.5f, 1f, 1, 0),
                _ => 1,
            };

            cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, StatuePos - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), lerpAmount);

            if (!Main.gameInactive && !Main.gamePaused && (lerpAmount != 1f || !ScreenChange))
                FramesElapsed++;

            if (FramesElapsed >= FramesToLast || !player.InModBiome<EnchantedKatanaShrineBiome>())
                Finished = true;

            if (player.dead || player.ghost || !player.active)
                Finished = true;

            Main.hideUI = !Finished;
        }
        #endregion

        public static Vector2 StatuePos;

        public static bool ScreenChange;

        public override int Music
        {
            get
            {
                if (Main.dayTime)
                    return MusicLoader.GetMusicSlot(Mod, "Assets/Music/EnchantedShrineDay");
                else
                    return MusicLoader.GetMusicSlot(Mod, "Assets/Music/EnchantedShrineNight");
            }
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player) => ModContent.GetInstance<MoreKatanaWorld>().samuraiStatueGiftCount >= 1;

        public override void OnEnter(Player player)
        {

        }

        public override void OnInBiome(Player player)
        {
            player.MKPlayer().EnchantedKatanaShrineEffect = 30;

            if (player.Distance(StatuePos) < 500)
            {
                if (!ScreenChange)
                {
                    ScreenChange = true;
                    Main.instance.CameraModifiers.Add(new EnchantedKatanaShrineBiome());
                }
            }
            else
            {
                ScreenChange = false;
            }
        }

        public override void OnLeave(Player player)
        {
            Main.hideUI = false;
            ScreenChange = false;
        }
    }
}