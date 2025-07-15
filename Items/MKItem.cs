using MoreKatana.Buffs;
using MoreKatana.Items.Katana;
using MoreKatana.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public class MKItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool Katana;          // 刀
        public int ActiveSkillDelay; // アクティブスキルのCDの時間

        /// <summary>
        /// 刀の基本的なステータス
        /// </summary>
        /// <param name="item"></param>
        /// <param name="delay"> アクティブスキルのCD </param>
        /// <param name="equipment"> 装備可能かどうか </param>
        public void SetKatanaDefaults(Item item, int delay, bool equipment)
        {
            item.DamageType = DamageClass.Melee;
            item.useStyle = ItemUseStyleID.Swing;
            item.UseSound = SoundID.Item1;
            item.accessory = equipment;
            item.autoReuse = true;
            Katana = true;
            ActiveSkillDelay = delay;
        }

        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Katana)
            {
                SetKatanaDefaults(item, 300, true);
                item.StatsModifiedBy.Add(Mod);
            }
            if (item.type == ItemID.Muramasa)
            {
                SetKatanaDefaults(item, 300, true);
                item.StatsModifiedBy.Add(Mod);
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (Katana)
            {
                // デバフを使って右クリを制御する
                return !player.HasBuff(ModContent.BuffType<KatanaArtsCD>());
            }

            return base.AltFunctionUse(item, player);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (Katana)
            {
                if (player.altFunctionUse == 2)
                {
                    if (item.type is ItemID.Katana or ItemID.Muramasa)
                    {
                        // バニラのアクティブスキル
                        VanillaActiveSkill(item, player);
                    }
                    else
                    {
                        // Modのアクティブスキル
                        (item.ModItem as KatanaItem).ActiveSkill(player);
                    }

                    // CD
                    player.AddBuff(ModContent.BuffType<KatanaArtsCD>(), ActiveSkillDelay);
                }
            }
            return base.CanUseItem(item, player);
        }

        public override void HoldItem(Item item, Player player)
        {
            if (Katana)
            {
                if (item.type is ItemID.Katana or ItemID.Muramasa)
                {
                    // バニラのパッシブスキル
                    VanillaPassiveSkill(item, player, false);
                }
                else
                {
                    // Modのパッシブスキル
                    (item.ModItem as KatanaItem).PassiveSkill(player, false);
                }
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (Katana)
            {
                if (item.type is ItemID.Katana or ItemID.Muramasa)
                {
                    // バニラのパッシブスキル
                    VanillaPassiveSkill(item, player, true);
                }
                else
                {
                    // Modのパッシブスキル
                    (item.ModItem as KatanaItem).PassiveSkill(player, true);
                }
            }
        }

        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            // ムラマサのヒトダマ発生処理
        }

        public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
        {
            // ムラマサのヒトダマ発生処理
        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            if ((equippedItem.type is ItemID.Katana or ItemID.Muramasa || equippedItem.ModItem is KatanaItem)
                && (incomingItem.type is ItemID.Katana or ItemID.Muramasa || incomingItem.ModItem is KatanaItem))
            {
                // 刀は同時に装備させないようにする
                return false;
            }

            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }

        public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
        {
            if (Katana)
            {
                // ModのスロットかつKatanaSlotsの時装備できる
                return modded && slot == AccessorySystem.KatanaSlots;
            }

            return base.CanEquipAccessory(item, player, slot, modded);
        }

        public void VanillaActiveSkill(Item item, Player player)
        {
            if (item.type == ItemID.Katana)
            {

            }
            if (item.type == ItemID.Muramasa)
            {

            }
        }

        public void VanillaPassiveSkill(Item item, Player player, bool equipment)
        {
            if (item.type == ItemID.Katana)
            {
                player.GetModPlayer<MKPlayer>().EquipKatana = true;
            }
            if (item.type == ItemID.Muramasa)
            {
                player.GetModPlayer<MKPlayer>().EquipMuramasa = true;
            }
        }
    }
}