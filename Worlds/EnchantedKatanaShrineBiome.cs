using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Worlds
{
    /// <summary>
    /// ストラクチャーのバイオーム
    /// </summary>
    public class EnchantedKatanaShrineBiome : ModBiome
    {
        public static Vector2 StatuePosition;

        public bool ScreenChange;

        private CameraModifier cameraModifier;

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

        public override void OnInBiome(Player player)
        {
            player.MKPlayer().EnchantedKatanaShrineEffect = 30;
            Main.hideUI = ScreenChange;

            if (player.Distance(StatuePosition) < 500)
            {
                if (!ScreenChange)
                {
                    ScreenChange = true;
                    cameraModifier = new CameraModifier(StatuePosition, 60, 0.5f, 0.5f, false, "EnchantedKatanaShrine");
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