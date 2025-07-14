using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.System
{
    public class MKPlayer : ModPlayer
    {
        // バニラ刀 //
        /// <summary>
        /// カタナ
        /// </summary>
        public bool EquipKatana = false;
        /// <summary>
        /// ムラマサ
        /// </summary>
        public bool EquipMuramasa = false;


        // モアカタナ //

        // 木材刀
        /// <summary>
        /// 木材の刀
        /// </summary>
        public bool EquipWoodenKatana = false;

        // プレハード金属刀

        /// <summary>
        /// 銅の刀
        /// </summary>
        public bool EquipCopperKatana = false;
        /// <summary>
        /// 金の刀
        /// </summary>
        public bool EquipGoldKatana = false;
        /// <summary>
        /// 鉛の刀
        /// </summary>
        public bool EquipLeadKatana = false;
        /// <summary>
        /// プラチナの刀
        /// </summary>
        public bool EquipPlatinumKatana = false;
        /// <summary>
        /// 銀の刀
        /// </summary>
        public bool EquipSilverKatana = false;
        /// <summary>
        /// 錫の刀
        /// </summary>
        public bool EquipTinKatana = false;
        /// <summary>
        /// タングステンの刀
        /// </summary>
        public bool EquipTungstenKatana = false;


        public override void ResetEffects()
        {
            EquipKatana = false;
            EquipMuramasa = false;
            EquipWoodenKatana = false;
        }

        public override void PreUpdate()
        {
            if (EquipKatana)
            {
                Player.statDefense += 2;
                Player.endurance += 0.05f;
            }

            if (EquipCopperKatana)
                Player.statDefense += 1;

            if (EquipTinKatana)
                Player.statDefense += 2;

            if (EquipTungstenKatana)
            {
                Player.statDefense += 5;
                Player.fireWalk = true;
                Player.buffImmune[BuffID.OnFire] = true;
            }

            if (EquipPlatinumKatana)
                Player.statDefense += 7;

            if (EquipLeadKatana)
            {
                Player.statDefense += 4;
                Player.moveSpeed *= 0.9f;
            }

            if (EquipSilverKatana)
                Player.statDefense += 2;

            if (EquipGoldKatana)
            {
                long coin = Utils.CoinsCount(out bool over, Player.inventory);
                int bonus = 0;
                if (over || coin >= 10000)
                    bonus = 10;
                else if (coin > 999)
                {
                    bonus = (int)(coin /= 1000);
                }
                Player.statDefense += (3 + bonus);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (EquipCopperKatana)
                CopperKatana(target);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (EquipKatana)
                Katana(target, ref modifiers);

            if (EquipWoodenKatana)
                WoodenKatana(target, ref modifiers);

            if (EquipTinKatana && !target.friendly)
                target.damage -= 1;

            if (EquipSilverKatana)
            {
                if (
                       target.type == NPCID.Vampire
                    || target.type == NPCID.VampireBat
                    || target.type == NPCID.Werewolf
                   )
                {
                    modifiers.SetInstantKill();
                }
            }
        }

        public void Katana(in NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.friendly) return;

            Item heldItem = Player.HeldItem;

            float armorPen = Player.GetArmorPenetration(heldItem.DamageType);

            int dam = Player.GetWeaponDamage(heldItem);

            Player.GetArmorPenetration(heldItem.DamageType) = int.MaxValue;

            target.SimpleStrikeNPC(dam / 10, modifiers.HitDirection);

            Player.GetArmorPenetration(heldItem.DamageType) = armorPen;
        }

        public void WoodenKatana(in NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.life < 1)
                return;
            modifiers.SetMaxDamage(target.life - 1);
        }

        public void CopperKatana(NPC target)
        {
            target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 11));
        }
    }
}
