using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.SceneEffect
{
    public class EnchantedShrineMusicScene : ModSceneEffect
    {
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

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player) => player.MKPlayer().EnchantedKatanaShrineMusicOverride > 0;
    }
}