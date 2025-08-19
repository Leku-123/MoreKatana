using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Worlds;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.UI
{
    public class EnchantedShrineCursor
    {
        public static void DrawEnchantedShrineCursor(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (drawPlayer.dead || drawPlayer.ghost || !drawPlayer.active)
                return;

            if (drawInfo.shadow != 0f)
                return;

            if (!drawPlayer.InModBiome<EnchantedKatanaShrineBiome>())
                return;

            if (!Main.hideUI)
                return;

            Texture2D texture = ModContent.Request<Texture2D>("MoreKatana/UI/EnchantedShrineCursor", AssetRequestMode.ImmediateLoad).Value;
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f * ((float)Math.Sin(Main.GameUpdateCount / 15f) + 0.3f);
                Main.spriteBatch.Draw(texture, mousePos + backglowOffset, null, Color.White with { A = 0 }, 0f, texture.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, mousePos, null, Color.White, 0f, texture.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

            var font = FontAssets.MouseText.Value;
            string text = MoreKatanaUtil.GetTextValue("Tooltips.GetIt");
            Vector2 textPos = mousePos + new Vector2(0, 5);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, Color.White, 0f, new Vector2(0.5f, 0.5f), Vector2.One);
        }
    }
}