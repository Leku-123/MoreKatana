using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace MoreKatana.Assets.ExtraTextures
{
    public static class MoreKatanaTextures
    {
        public const string TexturePath = "MoreKatana/Assets/ExtraTextures/";
        public const string AdditivePath = TexturePath + "AdditiveTextures/";
        public const string TrailPath = TexturePath + "Trails/";

        public static Asset<Texture2D> BloomTexture;
        public static Asset<Texture2D> MagicCircleTexture;
        public static Asset<Texture2D> MagicRingTexture;
        public static Asset<Texture2D> ShieldTexture;
        public static Asset<Texture2D> StarSparkleTexture;
        public static Asset<Texture2D>[] SwordTrailTexture = new Asset<Texture2D>[5];
        public static Asset<Texture2D> EnergyTrailTexture;
        public static Asset<Texture2D> FlameTrailTexture;
        public static Asset<Texture2D> StraightlineTrailTexture;
        public static Asset<Texture2D> DoublelinesTrailTexture;

        public static void LoadTextures()
        {
            if (Main.dedServ)
                return;

            BloomTexture = Request<Texture2D>(AdditivePath + "CircleGradient", AssetRequestMode.ImmediateLoad);
            MagicCircleTexture = Request<Texture2D>(AdditivePath + "MagicCircle", AssetRequestMode.ImmediateLoad);
            MagicRingTexture = Request<Texture2D>(AdditivePath + "MagicRing", AssetRequestMode.ImmediateLoad);
            ShieldTexture = Request<Texture2D>(AdditivePath + "Shield", AssetRequestMode.ImmediateLoad);
            StarSparkleTexture = Request<Texture2D>(AdditivePath + "StarSparkle", AssetRequestMode.ImmediateLoad);

            for (int i = 0; i < SwordTrailTexture.Length; i++)
                SwordTrailTexture[i] = Request<Texture2D>(TrailPath + "SwordSlashTrail_" + i, AssetRequestMode.ImmediateLoad);

            EnergyTrailTexture = Request<Texture2D>(TrailPath + "Trail_0", AssetRequestMode.ImmediateLoad);
            FlameTrailTexture = Request<Texture2D>(TrailPath + "Trail_1", AssetRequestMode.ImmediateLoad);
            StraightlineTrailTexture = Request<Texture2D>(TrailPath + "Trail_2", AssetRequestMode.ImmediateLoad);
            DoublelinesTrailTexture = Request<Texture2D>(TrailPath + "Trail_3", AssetRequestMode.ImmediateLoad);
        }

        public static void UnloadTextures()
        {
            if (Main.dedServ)
                return;

            BloomTexture = null;
            MagicCircleTexture = null;
            MagicRingTexture = null;
            ShieldTexture = null;
            StarSparkleTexture = null;

            for (int i = 0; i < SwordTrailTexture.Length; i++)
                SwordTrailTexture[i] = null;

            EnergyTrailTexture = null;
            FlameTrailTexture = null;
            StraightlineTrailTexture = null;
            DoublelinesTrailTexture = null;
        }
    }
}