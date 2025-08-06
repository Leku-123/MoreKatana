using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Buffs;
using MoreKatana.Items.Weapons;
using MoreKatana.Projectiles;
using MoreKatana.UI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MoreKatana.Items
{
    public class MoreKatanaGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool Katana;             // 刀
        public int AltDamage;           // アクティブスキルのダメージ
        public int ActiveSkillDelay;    // アクティブスキルのCDの時間
        public int SwingComboCount = 1; // 振りのコンボ数
        public int SwingType = 0;       // 振りの種類

        private int AttackType;
        private int ComboExpireTimer = 0;

        /// <summary>
        /// 刀の基本的なステータス
        /// </summary>
        /// <param name="item"></param>
        /// <param name="delay"> アクティブスキルのCD </param>
        /// <param name="equipment"> 装備可能かどうか </param>
        /// <param name="type"> 振りの種類 </param>
        /// <param name="combo"> 振りのコンボ数 </param>
        public void SetKatanaDefaults(Item item, int delay, bool equipment = false, int type = ProjectileID.None, int combo = 1)
        {
            item.DamageType = DamageClass.Melee;
            item.useStyle = ItemUseStyleID.Shoot;

            item.autoReuse = true;
            item.useTurn = false;
            item.noUseGraphic = true;
            item.noMelee = true;

            item.accessory = equipment;

            // 発射体が指定されていない場合、ダミーの発射体を発射する
            // Shoot()を適用させたいため
            item.shoot = item.shoot == ProjectileID.None ? ModContent.ProjectileType<Empty>() : item.shoot;
            item.shootSpeed = item.shootSpeed == 0f ? 1f : item.shootSpeed;

            Katana = true;
            ActiveSkillDelay = delay;

            // 何もしない場合汎用の振りが適用される
            SwingType = type == ProjectileID.None ? ModContent.ProjectileType<GeneralKatanaSwing>() : type;
            SwingComboCount = combo;
        }

        /// <summary>
        /// クールダウンを有効化する
        /// </summary>
        /// <param name="player"></param>
        public void ActivateCooldown(Player player) => player.AddBuff(ModContent.BuffType<KatanaArtsCD>(), ActiveSkillDelay);

        public override void SetDefaults(Item item)
        {
            if (item.type is ItemID.Katana or ItemID.Muramasa)
                item.StatsModifiedBy.Add(Mod);

            SetDefaultsVanillaItem(item);
        }

        public void SetDefaultsVanillaItem(Item item)
        {
            if (item.type == ItemID.Katana)
            {
                item.UseSound = SoundID.Item1;
                item.MKItem().AltDamage = 36;
                SetKatanaDefaults(item, 60, true);
            }
            if (item.type == ItemID.Muramasa)
            {
                item.UseSound = SoundID.Item1;
                item.MKItem().AltDamage = 48;
                SetKatanaDefaults(item, 60, true, combo: 2);
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (Katana)
            {
                // バフを使って右クリを制御する
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
                        // バニラアイテムのアクティブスキル
                        VanillaActiveSkill(item, player);
                    }
                    else
                    {
                        // Modアイテムのアクティブスキル
                        (item.ModItem as KatanaItem).ActiveSkill(player);
                    }
                }
            }
            return base.CanUseItem(item, player);
        }

        public override void UpdateInventory(Item item, Player player)
        {
            if (Katana)
            {
                if (item == player.ActiveItem())
                {
                    // 120fごとにコンボをリセットする
                    if (ComboExpireTimer++ >= 120)
                        AttackType = 0;

                    if (item.type is ItemID.Katana or ItemID.Muramasa)
                    {
                        // バニラアイテムのパッシブスキル
                        VanillaPassiveSkill(item, player, false);

                        // アイテムの設定を更新する
                        SetDefaultsVanillaItem(item);
                    }
                    else
                    {
                        // Modアイテムのパッシブスキル
                        (item.ModItem as KatanaItem).PassiveSkill(player, false);

                        // アイテムの設定を更新する
                        (item.ModItem as KatanaItem).SetDefaultsItem();
                    }
                }
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Katana)
            {
                if (!player.IsUsingAlt())
                {
                    Projectile.NewProjectile(source, position, velocity, SwingType, damage, knockback, player.whoAmI, AttackType);
                    AttackType = (AttackType + 1) % SwingComboCount;
                    ComboExpireTimer = 0;
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

        private void VanillaActiveSkill(Item item, Player player)
        {
            if (item.type == ItemID.Katana)
            {
                item.UseSound = SoundID.Item71;
                ActivateCooldown(player);
                player.CreateDashSlash(player.GetSource_ItemUse(item), AltDamage, item.knockBack, 400, 10f);
            }
            if (item.type == ItemID.Muramasa)
            {

            }
        }

        private void VanillaPassiveSkill(Item item, Player player, bool equipment)
        {
            if (item.type == ItemID.Katana)
            {
                player.statDefense += 2;
            }
            if (item.type == ItemID.Muramasa)
            {

            }
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

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "DefaultText")
            {
                Vector2 lineposition = new Vector2(line.OriginalX, line.OriginalY);
                Utils.DrawBorderString(Main.spriteBatch, line.Text, lineposition, Color.LightGoldenrodYellow);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawpos = lineposition + new Vector2(0, 2 * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                    Utils.DrawBorderString(Main.spriteBatch, line.Text, drawpos, Color.Goldenrod);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                return false;
            }
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (Katana)
            {
                int index = tooltips.FindIndex(x => x.Name == "Damage");
                if (index < 0)
                    return;

                // ダメージ表記を新たに挿入
                TooltipLine dam = new TooltipLine(Mod, "Verbose:NewDamage", $"{item.damage} / [c/FFB6C1:{AltDamage}] {item.DamageType.DisplayName}");
                tooltips.Insert(index + 1, dam);

                // 元々のダメージ表記のラインを非表示にする
                foreach (var i in tooltips)
                {
                    if (i.Name.EndsWith("Damage") && !i.Name.EndsWith(":NewDamage"))
                        i.Hide();
                }

                // クールダウン表記
                TooltipLine cd = new TooltipLine(Mod, "CD", "- " + Language.GetTextValue("Mods.MoreKatana.Tooltips.ActiveSkillCD") + ": " + ((float)ActiveSkillDelay / 60).ToString("F1") + " " + Language.GetTextValue("Mods.MoreKatana.Tooltips.Second"));

                if (item.type is ItemID.Katana or ItemID.Muramasa)
                {
                    // "Material" なのはカタナとムラマサにはツールチップ用のラインが存在しないため
                    int index2 = tooltips.FindIndex(x => x.Name == "Material");
                    if (index2 < 0)
                        return;

                    TooltipLine tip;
                    if (!ItemSlot.ShiftInUse)
                        tip = new TooltipLine(Mod, "DefaultText", Language.GetTextValue("Mods.MoreKatana.Tooltips.DefaultText"));
                    else
                    {
                        if (item.type == ItemID.Katana)
                            tip = new TooltipLine(Mod, "FunctionText", Language.GetTextValue($"Mods.MoreKatana.Items.Katana.FunctionText"));
                        else
                            tip = new TooltipLine(Mod, "FunctionText", Language.GetTextValue($"Mods.MoreKatana.Items.Muramasa.FunctionText"));
                        
                        // クールダウンを挿入
                        tooltips.Insert(index2 + 1, cd);
                    }

                    // スキル説明を挿入
                    tooltips.Insert(index2 + 1, tip);
                }

                int index3 = tooltips.FindIndex(x => x.Name == "Tooltip0");
                if (index3 < 0)
                    return;

                TooltipLine tip2;
                if (!ItemSlot.ShiftInUse)
                    tip2 = new TooltipLine(Mod, "DefaultText", Language.GetTextValue("Mods.MoreKatana.Tooltips.DefaultText"));
                else
                {
                    tip2 = new TooltipLine(Mod, "FunctionText", (string)(item.ModItem as KatanaItem).FunctionText);

                    // クールダウンを挿入
                    tooltips.Insert(index3, cd);
                }

                // スキル説明を挿入
                tooltips.Insert(index3, tip2);
            }
        }
    }
}