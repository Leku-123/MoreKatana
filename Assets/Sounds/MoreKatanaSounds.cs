using Terraria.Audio;

namespace MoreKatana
{
    public static class MoreKatanaSounds
    {
        public const string SoundPath = "MoreKatana/Assets/Sounds/";

        public static readonly SoundStyle MuteSound = new(SoundPath + "NIL") { Volume = 0.0f };
    }
}