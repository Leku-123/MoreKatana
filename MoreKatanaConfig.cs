using MoreKatana.UI;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MoreKatana
{
    public class MoreKatanaConfig : ModConfig
    {
        public static MoreKatanaConfig Instance;

        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("UI")]

        [BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(true)]
        public bool AccSlotPosLock { get; set; }

        [BackgroundColor(192, 54, 64, 192)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 100f)]
        [DefaultValue(KatanaSlot.DefaultPosX)]
        public float CustomAccSlotPosX { get; set; }

        [BackgroundColor(192, 54, 64, 192)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 100f)]
        [DefaultValue(KatanaSlot.DefaultPosY)]
        public float CustomAccSlotPosY { get; set; }

        [Header("Miscellaneous")]

        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 1f)]
        [Increment(0.01f)]
        [DefaultValue(1f)]
        public float ScreenShakePower { get; set; }
    }
}