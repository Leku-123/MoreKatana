using MoreKatana.Worlds;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.SceneEffect
{
    public class ForgottenAltarMusicScene : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                if (Main.dayTime)
                    return MusicLoader.GetMusicSlot(Mod, "Assets/Music/ForgottenAltarDay");
                else
                    return MusicLoader.GetMusicSlot(Mod, "Assets/Music/ForgottenAltarNight");
            }
        }

        public override SceneEffectPriority Priority
        {
            get
            {
                if (Main.LocalPlayer.InModBiome<ForgottenAltarBiome>())
                    return ModContent.GetInstance<ForgottenAltarBiome>().Priority;
                else
                    return SceneEffectPriority.BossHigh;
            }
        }

        public override bool IsSceneEffectActive(Player player) => player.MKPlayer().ForgottenAltarMusicOverride > 0;
    }
}