using Microsoft.Xna.Framework;
using MoreKatana.Buffs;
using MoreKatana.Items.Katana;
using MoreKatana.Projectiles;
using MoreKatana.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Items
{
    public class MoreKatanaGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool Katana;              // 刀
        public int ActiveSkillDelay;     // アクティブスキルのCDの時間
        private int SwingComboCount = 1; // 振りのコンボ数
        private int SwingType = 0;       // 振りタイプ(発射体)の種類
        private int AIType;

        /// <summary>
        /// 刀の基本的なステータス
        /// </summary>
        /// <param name="item"></param>
        /// <param name="delay"> アクティブスキルのCD </param>
        /// <param name="equipment"> 装備可能かどうか </param>
        public void SetKatanaDefaults(Item item, int delay, bool equipment = false, int type = ProjectileID.None, int combo = 1)
        {
            item.DamageType = DamageClass.Melee;
            item.useStyle = ItemUseStyleID.Swing;

            item.autoReuse = true;
            item.useTurn = false;
            item.noUseGraphic = true;
            item.noMelee = true;

            item.accessory = equipment;

            // 発射体が指定されていない場合、ダミーの発射体を発射する
            // Shoot()を適用させたいため
            item.shoot = item.shoot == ProjectileID.None ? ModContent.ProjectileType<Empty>() : item.shoot;
            item.shootSpeed = 1f;

            Katana = true;
            ActiveSkillDelay = delay;
            SwingType = type == ProjectileID.None ? ModContent.ProjectileType<GeneralKatanaSwing>() : type;
            SwingComboCount = combo;
        }

        /// <summary>
        /// クールダウンを有効化します
        /// </summary>
        /// <param name="player"></param>
        public void ActivateCooldown(Player player) => player.AddBuff(ModContent.BuffType<KatanaArtsCD>(), ActiveSkillDelay);

        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Katana)
            {
                SetKatanaDefaults(item, 300, true, combo: 2);
                item.StatsModifiedBy.Add(Mod);
            }
            if (item.type == ItemID.Muramasa)
            {
                SetKatanaDefaults(item, 300, true, combo: 2);
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
                if (player.IsUsingAlt())
                {
                    if (item.type is ItemID.Katana or ItemID.Muramasa)
                    {
                        // バニラのアクティブスキル
                        VanillaActiveSkill(item, player);
                        ActivateCooldown(player);
                    }
                    else
                    {
                        // Modのアクティブスキル
                        (item.ModItem as KatanaItem).ActiveSkill(player);
                    }
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

                    (item.ModItem as KatanaItem).SetDefaultsItem();
                }
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Katana)
            {
                if (!player.IsUsingAlt())
                {
                    Projectile.NewProjectile(source, position, velocity, SwingType, damage, knockback, player.whoAmI, AIType);
                    AIType = (AIType + 1) % SwingComboCount;
                    return false;
                }
            }

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
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

            }
            if (item.type == ItemID.Muramasa)
            {
                player.MKPlayer().EquipMuramasa = true;
            }
        }
    }
}