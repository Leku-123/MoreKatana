using Terraria.Audio;

namespace MoreKatana
{
    public static class MoreKatanaSounds
    {
        public const string SoundPath = "MoreKatana/Assets/Sounds/";

        public static readonly SoundStyle MuteSound = new SoundStyle(SoundPath + "NIL") { Volume = 0.0f };
        public static readonly SoundStyle LiquidsWaterLava = new SoundStyle(SoundPath + "Liquids_water_lava_", 3) { Volume = 2.0f };
    }
}