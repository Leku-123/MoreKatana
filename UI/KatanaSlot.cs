using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace MoreKatana.UI
{
    public class AccessorySystem : ModSystem
    {
        public static int KatanaSlots;
    }

    public class KatanaSlot : ModAccessorySlot
    {
        internal const float DefaultPosX = 50f;
        internal const float DefaultPosY = 50f;

        public bool MouseDrag = false;

        public Vector2? Location = null;

        public MoreKatanaConfig Config => MoreKatanaConfig.Instance;

        public static LocalizedText KatanasText { get; private set; }

        public override void SetupContent()
        {
            AccessorySystem.KatanaSlots = Type;
          
            // マウスをホバーしたときのテキスト
            KatanasText = Mod.GetLocalization($"{nameof(KatanaSlot)}.Katana");
        }

        public override Vector2? CustomLocation
        {
            get
            {
                if (!Config.AccSlotPosLock)
                    return Location;

                // カスタム位置がロックされている場合nullを返す
                return null;
            }
        }

        public override string FunctionalTexture => "MoreKatana/UI/KatanaSlot_Icon";
        public override string FunctionalBackgroundTexture => "MoreKatana/UI/KatanaSlot_Back";

        public override bool DrawFunctionalSlot => Main.EquipPage != 1 && (!UILinkPointNavigator.Shortcuts.NPCS_IconsDisplay || !PlayerInput.UsingGamepad);

        // 衣装スロットと染料スロットは設置しない
        public override bool DrawVanitySlot => false;
        public override bool DrawDyeSlot => false;

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            if (AccessorySystem.KatanaSlots != Type)
                AccessorySystem.KatanaSlots = Type;

            return checkItem.MKItem().Katana; // Katanaならスロットに入れられる
        }

        public override bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered)
        {
            Vector2 screenRatioPosition = new Vector2(Config.CustomAccSlotPosX, Config.CustomAccSlotPosY);
            if (screenRatioPosition.X < 0f || screenRatioPosition.X > 100f)
                screenRatioPosition.X = DefaultPosX;
            if (screenRatioPosition.Y < 0f || screenRatioPosition.Y > 100f)
                screenRatioPosition.Y = DefaultPosY;

            if (MouseDrag)
            {
                screenRatioPosition.X = (int)(Main.MouseScreen.X / 0.01f / Main.screenWidth);
                screenRatioPosition.Y = (int)(Main.MouseScreen.Y / 0.01f / Main.screenHeight);
            }

            Vector2 screenPos = screenRatioPosition;
            screenPos.X = (int)(screenPos.X * 0.01f * Main.screenWidth);
            screenPos.Y = (int)(screenPos.Y * 0.01f * Main.screenHeight);

            Location = screenPos;

            if (!Config.AccSlotPosLock)
            {
                bool changed = false;
                if (Config.CustomAccSlotPosX != screenRatioPosition.X)
                {
                    Config.CustomAccSlotPosX = screenRatioPosition.X;
                    changed = true;
                }
                if (Config.CustomAccSlotPosY != screenRatioPosition.Y)
                {
                    Config.CustomAccSlotPosY = screenRatioPosition.Y;
                    changed = true;
                }

                if (changed)
                    MoreKatana.SaveConfig(Config);
            }

            return true;
        }

        public override void OnMouseHover(AccessorySlotType context)
        {
            if (Main.mouseRight)
                MouseDrag = true;
            else
                MouseDrag = false;

            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = KatanasText.Value;
                    break;
            }
        }
    }
}