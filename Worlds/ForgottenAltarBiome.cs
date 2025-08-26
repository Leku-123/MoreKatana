using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Worlds
{
    /// <summary>
    /// ストラクチャーのバイオーム
    /// </summary>
    public class ForgottenAltarBiome : ModBiome
    {
        public static Vector2 StatuePosition;

        public bool ScreenChange;

        private CameraModifier cameraModifier;

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player) => ModContent.GetInstance<MoreKatanaWorld>().samuraiStatueGiftCount >= 1;

        public override void OnEnter(Player player)
        {
            if (!MoreKatanaWorld.seenForgottenAltarBiome)
                MoreKatanaWorld.seenForgottenAltarBiome = true;
        }

        public override void OnInBiome(Player player)
        {
            player.MKPlayer().ForgottenAltarEffect = 30;
            player.MKPlayer().ForgottenAltarMusicOverride = 30;

            Main.hideUI = ScreenChange;

            if (player.Distance(StatuePosition) < 500)
            {
                if (!ScreenChange)
                {
                    ScreenChange = true;
                    cameraModifier = new CameraModifier(StatuePosition, 60, 0.5f, 0.5f, false, "ForgottenAltar");
                    cameraModifier.AddCameraModifier();
                }
            }
            else
            {
                if (ScreenChange)
                {
                    ScreenChange = false;
                    if (cameraModifier != null)
                        cameraModifier.AutoReturn = true;
                }
            }
        }

        public override void OnLeave(Player player)
        {
            Main.hideUI = false;
            if (cameraModifier != null)
                cameraModifier.AutoReturn = true;
        }
    }
}