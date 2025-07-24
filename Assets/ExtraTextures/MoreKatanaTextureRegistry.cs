using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;

namespace MoreKatana.Assets.ExtraTextures
{
    public static class MoreKatanaTextureRegistry
    {
        public const string TexturePath = "WeaponsOverhaul/Assets/ExtraTextures/";
        public const string AdditivePath = TexturePath + "AdditiveTextures/";
        public const string TrailPath = TexturePath + "Trails/";

        #region Additive Textures
        public static Asset<Texture2D> BloomTexture => Request<Texture2D>(AdditivePath + "CircleGradient");
        #endregion

        #region Trail Textures
        public static Asset<Texture2D> SwordTrailTexture(int num) => Request<Texture2D>(TrailPath + "SwordSlashTrail_" + System.Math.Min(num, 4), AssetRequestMode.ImmediateLoad);
        public static Asset<Texture2D> EnergyTrailTexture => Request<Texture2D>(TrailPath + "Trail_0", AssetRequestMode.ImmediateLoad);
        public static Asset<Texture2D> FlameTrailTexture => Request<Texture2D>(TrailPath + "Trail_1", AssetRequestMode.ImmediateLoad);
        public static Asset<Texture2D> StraightlineTrailTexture => Request<Texture2D>(TrailPath + "Trail_2", AssetRequestMode.ImmediateLoad);
        #endregion
    }
}