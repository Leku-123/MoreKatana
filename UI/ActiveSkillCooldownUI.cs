using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MoreKatana.Assets.ExtraTextures;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace MoreKatana.UI
{
    public class ActiveSkillCooldownUI : ModSystem
    {
        internal const float DefaultPosX = 40f;
        internal const float DefaultPosY = 5f;
        private const float MouseDragEpsilon = 0.05f;

        private static Vector2? dragOffset = null;

        private const int particleTextureFrames = 5;
        public static int particleFrame;
        public static int particleFrameTimer;

        private static Texture2D preparedTexture, sheatheTexture, particleTexture;

        private static MoreKatanaConfig Config => MoreKatanaConfig.Instance;

        public override void OnModLoad()
        {
            preparedTexture = ModContent.Request<Texture2D>("MoreKatana/UI/ActiveSkillCooldownUI_Prepared", AssetRequestMode.ImmediateLoad).Value;
            sheatheTexture = ModContent.Request<Texture2D>("MoreKatana/UI/ActiveSkillCooldownUI_Sheathe", AssetRequestMode.ImmediateLoad).Value;
            particleTexture = ModContent.Request<Texture2D>("MoreKatana/UI/ActiveSkillCooldownUI_Particle", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            preparedTexture = sheatheTexture = particleTexture = null;
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            Vector2 screenRatioPosition = new Vector2(Config.CustomActiveSkillCDPosX, Config.CustomActiveSkillCDPosY);
            if (screenRatioPosition.X < 0f || screenRatioPosition.X > 100f)
                screenRatioPosition.X = DefaultPosX;
            if (screenRatioPosition.Y < 0f || screenRatioPosition.Y > 100f)
                screenRatioPosition.Y = DefaultPosY;

            float uiScale = Main.UIScale;
            Vector2 screenPos = screenRatioPosition;
            screenPos.X = (int)(screenPos.X * 0.01f * Main.screenWidth);
            screenPos.Y = (int)(screenPos.Y * 0.01f * Main.screenHeight);

            MoreKatanaPlayer mkPlayer = player.MKPlayer();

            float quotient;
            if (mkPlayer.ActiveSkillCD != 0)
            {
                quotient = (float)mkPlayer.ActiveSkillCD / mkPlayer.ActiveSkillCDMax;
                particleFrame = 0;
                particleFrameTimer = 0;
            }
            else
            {
                quotient = 0;

                if (++particleFrameTimer % 3 == 0)
                    particleFrame++;
            }

            if (mkPlayer.ActiveSkillCD != 0 && mkPlayer.ActiveSkillCD < 10)
                screenPos += Main.rand.NextVector2Unit() * 2;

            DrawCooldownUI(spriteBatch, screenPos, quotient);

            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);
            Rectangle waterBarArea = Utils.CenteredRectangle(screenPos, preparedTexture.Size() * uiScale);

            MouseState ms = Mouse.GetState();
            Vector2 mousePos = Main.MouseScreen;

            // マウスのドラッグ操作
            if (waterBarArea.Intersects(mouseHitbox))
            {
                if (!Config.UIPosLock)
                    Main.LocalPlayer.mouseInterface = true;

                // マウスがUIの上にある場合、クールダウン時間を表示する
                if (quotient > 0f)
                {
                    int cooldown = mkPlayer.ActiveSkillCD / 60 + 1;
                    string textToDisplay = MoreKatanaUtil.GetTextValue("Tooltips.ActiveSkillCD") + ": " + (mkPlayer.ActiveSkillCD / 60 + 1) + " " + MoreKatanaUtil.GetTextValue("Tooltips.Second");
                    Main.instance.MouseText(textToDisplay, 0, 0, -1, -1, -1, -1);
                }
                else
                {

                }

                Vector2 newScreenRatioPosition = screenRatioPosition;

                // マウスを押している間UIをオフセットとともにドラッグする。
                if (!Config.UIPosLock && ms.LeftButton == ButtonState.Pressed)
                {
                    if (!dragOffset.HasValue)
                        dragOffset = mousePos - screenPos;

                    Vector2 newCorner = mousePos - dragOffset.GetValueOrDefault(Vector2.Zero);

                    // 新しいnewCornerの位置を画面比率の位置に変換する
                    newScreenRatioPosition.X = (100f * newCorner.X) / Main.screenWidth;
                    newScreenRatioPosition.Y = (100f * newCorner.Y) / Main.screenHeight;
                }

                // 位置の変化を計算してUIを動かす
                Vector2 delta = newScreenRatioPosition - screenRatioPosition;
                if (Math.Abs(delta.X) >= MouseDragEpsilon || Math.Abs(delta.Y) >= MouseDragEpsilon)
                {
                    Config.CustomActiveSkillCDPosX = newScreenRatioPosition.X;
                    Config.CustomActiveSkillCDPosY = newScreenRatioPosition.Y;
                }

                // マウスを離したら設定を保存しdragOffsetをnull
                if (ms.LeftButton == ButtonState.Released)
                {
                    dragOffset = null;
                    MoreKatana.SaveConfig(Config);
                }
            }
            else
            {
                if (!Config.UIPosLock)
                {
                    bool changed = false;
                    if (Config.CustomActiveSkillCDPosX != screenRatioPosition.X)
                    {
                        Config.CustomActiveSkillCDPosX = screenRatioPosition.X;
                        changed = true;
                    }
                    if (Config.CustomActiveSkillCDPosY != screenRatioPosition.Y)
                    {
                        Config.CustomActiveSkillCDPosY = screenRatioPosition.Y;
                        changed = true;
                    }

                    if (changed)
                        MoreKatana.SaveConfig(Config);
                }
            }
        }

        private static void DrawCooldownUI(SpriteBatch spriteBatch, Vector2 screenPos, float ratio)
        {
            float uiScale = Main.UIScale;

            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
            spriteBatch.Draw(bloom, screenPos, null, Color.DimGray with { A = 0 } * 0.7f, 0f, bloom.Size() / 2f, new Vector2(1.5f * uiScale, 0.7f * uiScale), SpriteEffects.None, 0);

            if (ratio == 0)
            {
                spriteBatch.Draw(preparedTexture, screenPos, null, Color.White, 0f, preparedTexture.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

                Texture2D starTex = MoreKatanaTextures.StarSparkleTexture.Value;
                Color color = Color.White * 0.3f;
                float rot = Main.GlobalTimeWrappedHourly;
                spriteBatch.Draw(starTex, screenPos + new Vector2(30 * uiScale, 0), null, color with { A = 0 }, rot, starTex.Size() / 2f, uiScale * 0.15f, 0, 0);
                spriteBatch.Draw(starTex, screenPos + new Vector2(30 * uiScale, 0), null, color with { A = 0 }, -rot, starTex.Size() / 2f, uiScale * 0.2f, 0, 0);

                Rectangle particleRect = particleTexture.Frame(1, particleTextureFrames, 0, particleFrame);
                spriteBatch.Draw(particleTexture, screenPos + new Vector2(25 * uiScale, 3 * uiScale), particleRect, Color.White, 0f, particleRect.Size() * 0.5f, uiScale, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(sheatheTexture, screenPos, null, Color.Gray, 0f, sheatheTexture.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

                Rectangle rectangle = new Rectangle(0, 0, (int)(sheatheTexture.Width * (1 - ratio)), sheatheTexture.Height);

                Rectangle highlightRect = rectangle;
                highlightRect.Width += 4;
                spriteBatch.Draw(sheatheTexture, screenPos, highlightRect, Color.White with { A = 0 }, 0f, sheatheTexture.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

                spriteBatch.Draw(sheatheTexture, screenPos, rectangle, Color.White, 0f, sheatheTexture.Size() * 0.5f, uiScale, SpriteEffects.None, 0);
            }

            DynamicSpriteFontExtensionMethods.DrawString(
                spriteBatch,
                FontAssets.MouseText.Value,
                (int)((1 - ratio) * 100) + "%",
                screenPos + new Vector2(0, 25),
                Color.OrangeRed, 0f,
                ChatManager.GetStringSize(FontAssets.MouseText.Value, (int)((1 - ratio) * 100) + "%", Vector2.One) / 2,
                uiScale, SpriteEffects.None, 0f);
        }
    }
}