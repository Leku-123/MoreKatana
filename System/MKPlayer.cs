using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.System
{
    public class MKPlayer : ModPlayer
    {
        public bool EquipKatana = false;
        public bool EquipMuramasa = false;
        public bool EquipWoodenKatana = false;

        public override void ResetEffects()
        {
            EquipKatana = false;
            EquipMuramasa = false;
            EquipWoodenKatana = false;
        }

        public override void PreUpdate()
        {
            if (EquipKatana)
                KatanaUpdate();
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (EquipWoodenKatana)
                WoodenKatana(in target, ref modifiers);
            if (EquipKatana)
                Katana(in target, ref modifiers);

        }

        public void WoodenKatana(in NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.life < 1)
                return;
            modifiers.SetMaxDamage(target.life - 1);
        }

        public void KatanaUpdate()
        {
            Player.statDefense += 2;
            Player.endurance += 0.05f;
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
    }
}
