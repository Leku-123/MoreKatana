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
    /// <summary>
    /// 石像に近づいてUIが非表示(Main.hideUI)になる可能性があるので
    /// UIの表示のタスクとは別のタスクでレンダリングします
    /// </summary>
    public class ForgottenAltarDisplay
    {
        /// <summary> UI描画のタイマー </summary>
        private static int DisplayTimer;
        /// <summary> UIのフェードアウトが始まるまでの時間 </summary>
        private const int DisplayTimeCount = 3 * 60;
        /// <summary> UIのテキスト </summary>
        private static string DisplayText = "";
        /// <summary> テキストの表示される長さ </summary>
        private static int TextScrollNumber;
        /// <summary> テキストの長さ </summary>
        private static float MaxTextLength;

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            // テキスト
            DisplayText = MoreKatanaUtil.GetTextValue($"Biomes.{nameof(ForgottenAltarBiome)}.DisplayName");

            if (DisplayTimer == 0)
                MaxTextLength = DisplayText.Length;

            if (MoreKatanaWorld.seenForgottenAltarBiome)
                DisplayTimer++;
            else
                DisplayTimer = 0;

            if (!player.InModBiome<ForgottenAltarBiome>())
                return;

            if (DisplayTimer <= DisplayTimeCount)
            {
                if (TextScrollNumber < DisplayText.Length)
                {
                    if (DisplayTimer % 3 == 0)
                    {
                        SoundEngine.PlaySound(MoreKatanaSounds.DialogueTick, player.Center);
                        TextScrollNumber++;
                    }
                }
            }
            else
            {
                if (TextScrollNumber > 0)
                {
                    if (DisplayTimer % 3 == 0)
                    {
                        SoundEngine.PlaySound(MoreKatanaSounds.DialogueTick, player.Center);
                        TextScrollNumber--;
                    }
                }
            }

            if (TextScrollNumber > 0)
            {
                player.velocity.X = 0f;
                player.position = player.oldPosition;

                player.immune = true;
                player.immuneTime = 60;
                player.immuneNoBlink = true;
                player.noFallDmg = true;
            }

            DisplayText = DisplayText.Substring(0, TextScrollNumber);

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
            spriteBatch.Draw(bloom, textPosition, null, Color.Gray with { A = 0 } * opacity * 0.7f, 0f, bloom.Size() / 2f, scale, SpriteEffects.None, 0);

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, DisplayText, textPosition - Vector2.UnitX * textArea, Color.White, 0f, textArea * new Vector2(0f, 0.5f), new Vector2(2f));

            spriteBatch.SetEndBegin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawpos = textPosition + new Vector2(0, 3 * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, DisplayText, drawpos - Vector2.UnitX * textArea, Color.White, 0f, textArea * new Vector2(0f, 0.5f), new Vector2(2f));
            }
            spriteBatch.SetEndBegin(SpriteSortMode.Deferred, null, null, null, null);
        }
    }
}