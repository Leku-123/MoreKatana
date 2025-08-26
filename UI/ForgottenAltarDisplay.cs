using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Worlds;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.UI
{
    public class ForgottenAltarDisplay
    {
        private static int DisplayTimer;
        private const int DisplayTimeCount = 3 * 60;
        private static int DisplayScrollNumber;
        private static string DisplayText = "";
        private static float MaxTextLength;

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            DisplayText = MoreKatanaUtil.GetTextValue($"Biomes.{nameof(ForgottenAltarBiome)}.DisplayName");

            if (DisplayTimer == 0)
                MaxTextLength = DisplayText.Length;

            if (MoreKatanaWorld.seenForgottenAltarBiome)
                DisplayTimer++;
            else
                DisplayTimer = 0;

            if (!player.InModBiome<ForgottenAltarBiome>())
                return;

            void TickSound() => SoundEngine.PlaySound(MoreKatanaSounds.DialogueTick, player.Center);

            if (DisplayTimer <= DisplayTimeCount)
            {
                player.velocity *= 0.85f;

                if (DisplayScrollNumber < DisplayText.Length)
                {
                    if (DisplayTimer % 3 == 0)
                    {
                        TickSound();
                        DisplayScrollNumber++;
                    }
                }
            }
            else
            {
                if (DisplayScrollNumber > 0)
                {
                    if (DisplayTimer % 3 == 0)
                    {
                        TickSound();
                        DisplayScrollNumber--;
                    }
                }
            }

            DisplayText = DisplayText.Substring(0, DisplayScrollNumber);

            var font = FontAssets.MouseText.Value;
            Vector2 textArea = font.MeasureString(DisplayText);
            Vector2 textPosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 4);

            Vector2 offset = new Vector2(20, 0);
            Vector2 leftside = textPosition - Vector2.UnitX * textArea - offset;
            Vector2 rightside = textPosition + Vector2.UnitX * textArea + offset;
            float opacity = DisplayText.Length / MaxTextLength;

            string textureKey = "MoreKatana/UI/ForgottenAltarDisplay";
            Texture2D frame = ModContent.Request<Texture2D>(textureKey + "_Frame", AssetRequestMode.ImmediateLoad).Value;
            Vector2 scale = new Vector2(Vector2.Distance(leftside, rightside) / frame.Width, 1f);
            spriteBatch.Draw(frame, textPosition + new Vector2(0, 10), null, Color.White * opacity, 0f, new Vector2(frame.Width / 2, frame.Height / 2), scale, SpriteEffects.None, 0f);

            Texture2D left = ModContent.Request<Texture2D>(textureKey + "_Left", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw(left, leftside, null, Color.White * opacity, 0f, new Vector2(left.Width / 2, left.Height / 2), 1f, SpriteEffects.None, 0f);

            Texture2D right = ModContent.Request<Texture2D>(textureKey + "_Right", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw(right, rightside, null, Color.White * opacity, 0f, new Vector2(right.Width / 2, right.Height / 2), 1f, SpriteEffects.None, 0f);

            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            spriteBatch.Draw(bloom, textPosition, null, Color.Gray with { A = 0 } * opacity * 0.5f, 0f, bloom.Size() / 2f, scale, SpriteEffects.None, 0);

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, DisplayText, textPosition - Vector2.UnitX * textArea, Color.White, 0f, textArea * new Vector2(0f, 0.5f), new Vector2(2f));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawpos = textPosition + new Vector2(0, 2 * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, DisplayText, drawpos - Vector2.UnitX * textArea, Color.White, 0f, textArea * new Vector2(0f, 0.5f), new Vector2(2f));
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}