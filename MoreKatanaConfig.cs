using MoreKatana.UI;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MoreKatana
{
    public class MoreKatanaConfig : ModConfig
    {
        public static MoreKatanaConfig Instance;

        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("VanillaRework")]

        [DefaultValue(true)]
        public bool KatanaRework { get; set; }

        [DefaultValue(true)]
        public bool MuramasaRework { get; set; }

        [Header("WorldGen")]

        [DefaultValue(true)]
        public bool ForgottenAltar { get; set; }

        [Header("Particle")]

        //[BackgroundColor(154, 204, 254, 192)]
        //[SliderColor(224, 165, 56, 128)]
        [Range(100f, 1000f)]
        [DefaultValue(500f)]
        [DrawTicks]
        [Increment(100f)]
        public float MaxParticles { get; set; }

        //[BackgroundColor(154, 204, 254, 192)]
        [DefaultValue(true)]
        public bool ForegroundParticles { get; set; }

        [Header("UI")]

        //[BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(true)]
        public bool UIPosLock { get; set; }

        //[BackgroundColor(192, 54, 64, 192)]
        //[SliderColor(224, 165, 56, 128)]
        [Range(0f, 100f)]
        [DefaultValue(KatanaSlot.DefaultPosX)]
        public float CustomAccSlotPosX { get; set; }

        //[BackgroundColor(192, 54, 64, 192)]
        //[SliderColor(224, 165, 56, 128)]
        [Range(0f, 100f)]
        [DefaultValue(KatanaSlot.DefaultPosY)]
        public float CustomAccSlotPosY { get; set; }

        [Range(0f, 100f)]
        [DefaultValue(ActiveSkillCooldownUI.DefaultPosX)]
        public float CustomActiveSkillCDPosX { get; set; }

        [Range(0f, 100f)]
        [DefaultValue(ActiveSkillCooldownUI.DefaultPosY)]
        public float CustomActiveSkillCDPosY { get; set; }

        [Header("Miscellaneous")]

        //[SliderColor(224, 165, 56, 128)]
        [Range(0f, 1f)]
        [Increment(0.01f)]
        [DefaultValue(1f)]
        public float ScreenShakePower { get; set; }
    }
}