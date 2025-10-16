using Terraria.Audio;

namespace MoreKatana
{
    public static class MoreKatanaSounds
    {
        public const string SoundPath = "MoreKatana/Assets/Sounds/";

        public static readonly SoundStyle MuteSound = new SoundStyle(SoundPath + "NIL") { Volume = 0.0f };
        public static readonly SoundStyle SlashEffect = new SoundStyle(SoundPath + "SlashEffect");
        public static readonly SoundStyle SwordSlash = new SoundStyle(SoundPath + "SwordSlash_", 3) { Volume = 0.7f };
        public static readonly SoundStyle SlashHit = new SoundStyle(SoundPath + "SlashHit") { Volume = 0.8f };
        public static readonly SoundStyle LiquidsWaterLava = new SoundStyle(SoundPath + "Liquids_water_lava_", 3) { Volume = 2.0f };
        public static readonly SoundStyle DialogueTick = new SoundStyle(SoundPath + "DialogueTick") { Volume = 0.1f };
    }
}