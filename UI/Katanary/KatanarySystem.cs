using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace MoreKatana.UI.Katanary
{
    [Autoload(Side = ModSide.Client)]
    public class KatanarySystem : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            int pivotTopLeftX = 0, pivotTopLeftY = 0;

            Main.inventoryScale = 0.85f;
            int num = (int)((float)(450 + pivotTopLeftX) - 56f * Main.inventoryScale * 2f);
            int num2 = 258 + pivotTopLeftY;
            int width = 30;
            int num3 = 30;
            int num4 = 244;
            num = 498;
            num2 = num4 + num3 + 4;
            if ((Main.player[Main.myPlayer].chest != -1 || Main.npcShop > 0) && !Main.recBigList)
            {
                num2 += 168;
                Main.inventoryScale = 0.755f;
                num += 5;
                num4 += 24;
            }
            if (Main.editChest)
            {
                num2 += 24;
            }
            Rectangle rectangle = new Rectangle(num, num2, (int)((float)TextureAssets.InventoryBack.Width() * Main.inventoryScale), (int)((float)TextureAssets.InventoryBack.Height() * Main.inventoryScale));
            rectangle = new Rectangle(num, num2, width, num3);
            bool flag = false;
            if (rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)
            {
                Main.player[Main.myPlayer].mouseInterface = true;
                flag = true;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.player[Main.myPlayer].SetTalkNPC(-1, false);
                    Main.npcChatCornerItem = 0;
                    Main.npcChatText = "";
                    Main.mouseLeftRelease = false;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    IngameFancyUI.OpenUIState(Main.BestiaryUI);
                    Main.BestiaryUI.OnOpenPage();
                }
            }
            Texture2D value = TextureAssets.BestiaryMenuButton.Value;
            Vector2 position = rectangle.Center.ToVector2();
            Rectangle rectangle2 = value.Frame(2, 1, (!flag) ? 1 : 0, 0, 0, 0);
            rectangle2.Width -= 2;
            rectangle2.Height -= 2;
            Vector2 origin = rectangle2.Size() / 2f;
            Color white = Color.White;
            Main.spriteBatch.Draw(value, position, new Rectangle?(rectangle2), white, 0f, origin, 1f, SpriteEffects.None, 0f);
            UILinkPointNavigator.SetPosition(310, position);
            if (!Main.mouseText && flag)
            {
                Main.instance.MouseText(Language.GetTextValue("GameUI.Bestiary"), 0, 0, -1, -1, -1, -1, 0);
            }
        }

    }
}
