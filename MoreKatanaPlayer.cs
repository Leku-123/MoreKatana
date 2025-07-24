using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace MoreKatana
{
    public class MoreKatanaPlayer : ModPlayer
    {
        // -------- Screen --------
        public Entity ScreenLockEntity = null;
        public Vector2 ScreenLockPos;
        public int ScreenShakeTimer;
        public int ScreenShakeStrength;





        public bool EquipMuramasa = false;

        public override void ResetEffects()
        {
            ScreenLockPos = Player.position;
            if (ScreenShakeTimer > 0)
                ScreenShakeTimer--;





            EquipMuramasa = false;
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
            /*if (EquipGoldKatana)
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Katana(target, ref modifiers);
        }

        /*public void Katana(in NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.friendly) return;

            Item heldItem = Player.HeldItem;

            float armorPen = Player.GetArmorPenetration<GenericDamageClass>();

            int dam = Player.GetWeaponDamage(heldItem);

            Player.GetArmorPenetration<GenericDamageClass>() = int.MaxValue;

            target.SimpleStrikeNPC(dam / 10, modifiers.HitDirection);

            Player.GetArmorPenetration<GenericDamageClass>() = armorPen;
        }*/
    }
}
