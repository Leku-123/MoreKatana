using Microsoft.Xna.Framework;
using MoreKatana.Items;
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
        public static LocalizedText KatanasText { get; private set; }

        public override void SetupContent()
        {
            // マウスをホバーしたときのテキスト
            KatanasText = Mod.GetLocalization($"{nameof(KatanaSlot)}.Katana");
        }

        public override void Load()
        {
            AccessorySystem.KatanaSlots = Type;
        }

        // アクセサリースロットを頭の染料スロットの横に設置する
        // 仮なのでもっと分かりやすい場所に変えても良い
        // ModConfig作って位置を調節できるようにしても良いかもしれないですね
        public override Vector2? CustomLocation
        {
            get
            {
                int height = 175;
                if (Main.mapStyle == 1)
                {
                    height += Main.miniMapHeight + 16;
                }
                return new Vector2?(new Vector2(Main.screenWidth - 246, height));
            }
        }

        public override string FunctionalTexture => "MoreKatana/UI/KatanaSlot";
        public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back7";

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

        public override void OnMouseHover(AccessorySlotType context)
        {
            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = KatanasText.Value;
                    break;
            }
        }
    }
}