using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MoreKatana
{
    public class MoreKatanaPlayer : ModPlayer
    {
        // -------- Screen --------
        public Entity ScreenLockEntity = null;
        public Vector2 ScreenLockPos;
        public int ScreenShakeTimer;
        public int ScreenShakeStrength;

        // -------- Dash --------
        public bool DashState;
        public bool GeneralDash;
        public bool SuddenStop;
        public int DashDistance;
        public float DashTimer;
        public float DashTimerMax;
        public Vector2 DashDirection, DashStartPos, DashEndPos;

        public float Flipping;

        public bool EquipMuramasa;

        public override void ResetEffects()
        {
            ScreenLockPos = Player.position;
            if (ScreenShakeTimer > 0)
                ScreenShakeTimer--;
            DashState = false;
            if (!GeneralDash)
                DashTimer = 0f;
            EquipMuramasa = false;
        }

        public override void UpdateDead()
        {
            ResetEffects();
            GeneralDash = false;
            Flipping = 0f;
        }

        public override void ModifyScreenPosition()
        {
            // スクリーンの位置を変更する
            if (ScreenLockEntity != null)
            {
                if (ScreenLockEntity.active && Player.active)
                {
                    Main.screenPosition.X = ScreenLockPos.X - (Main.screenWidth / 2);
                    Main.screenPosition.Y = ScreenLockPos.Y - (Main.screenHeight / 2);
                }
            }

            // スクリーンを揺らす
            // TO-DO 設定で強度を調整可にする
            if (ScreenShakeTimer > 0)
            {
                Main.screenPosition.Y += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength);
                Main.screenPosition.X += Main.rand.Next(-ScreenShakeStrength, ScreenShakeStrength);
            }
        }

        public override void PreUpdate()
        {
            // 汎用のダッシュのステータス
            if (DashState)
            {
                Player.immune = true;
                Player.immuneTime = 60;
                Player.immuneNoBlink = true;
                Player.noFallDmg = true;
                Player.controlJump = false;
                Player.maxFallSpeed = 2000f;
                Player.RemoveAllGrapplingHooks();
                if (Player.mount.Active)
                    Player.mount.Dismount(Player);
            }

            /*if (GoldKatana)
            {
                long coin = Utils.CoinsCount(out bool over, Player.inventory);
                int bonus = 0;
                if (over || coin >= 10000)
                    bonus = 10;
                else if (coin > 999)
                    bonus = (int)(coin /= 1000);
                Player.statDefense += 3 + bonus;
            }*/
        }

        public override void PostUpdateRunSpeeds()
        {
            // 汎用の簡単なダッシュ
            if (GeneralDash && DashTimerMax != 0)
            {
                if (DashTimer == 0)
                {
                    DashStartPos = Player.MountedCenter;
                    DashDirection.Normalize();
                    DashDirection *= DashDistance;
                    DashEndPos = DashStartPos + DashDirection;
                }

                float currentProgress = DashTimer / DashTimerMax;
                float nextProgress = (DashTimer + 1) / DashTimerMax;

                if (currentProgress < 1f)
                {
                    var currentPoint = Vector2.Lerp(DashStartPos, DashEndPos, currentProgress);
                    var nextPoint = Vector2.Lerp(DashStartPos, DashEndPos, nextProgress);
                    Player.velocity = nextPoint - currentPoint;
                }
                else
                {
                    GeneralDash = false;
                    DashTimer = 0f;
                    DashTimerMax = 0f;

                    if (SuddenStop)
                        Player.velocity = Vector2.Zero;
                }

                DashTimer++;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            /*if (Katana)
            {
                if (target.friendly)
                    return;

                Item heldItem = Player.HeldItem;
                float armorPen = Player.GetArmorPenetration<GenericDamageClass>();
                int dam = Player.GetWeaponDamage(heldItem);
                Player.GetArmorPenetration<GenericDamageClass>() = int.MaxValue;
                target.SimpleStrikeNPC(dam / 10, modifiers.HitDirection);
                Player.GetArmorPenetration<GenericDamageClass>() = armorPen;
            }*/
        }

        public static DrawData ManipulateDrawInfo(DrawData input, Player player)
        {
            float rotation = player.MKPlayer().Flipping;
            if (rotation != 0)
            {
                float sin = (float)Math.Sin(rotation + 1.57f * player.direction);
                int off = Math.Abs((int)((input.useDestinationRectangle ? input.destinationRectangle.Width : input.sourceRect?.Width ?? input.texture.Width) * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (input.effect == SpriteEffects.FlipHorizontally)
                    effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                var newRect = new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height);
                var newData = new DrawData(input.texture, newRect, input.sourceRect, input.color, input.rotation, input.origin, effect, 0)
                {
                    shader = input.shader
                };

                return newData;
            }
            else
            {
                return input;
            }
        }
    }
}
