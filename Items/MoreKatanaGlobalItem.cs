using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Assets.ExtraTextures;
using MoreKatana.Items.Weapons;
using MoreKatana.Projectiles;
using MoreKatana.Projectiles.Misc;
using MoreKatana.Projectiles.TerraKatanaTree;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MoreKatana.Items
{
    public partial class MoreKatanaGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool Katana;             // 刀のアイテム
        public int AltDamage;           // アクティブスキルのダメージ
        public int ActiveSkillDelay;    // アクティブスキルのCDの時間
        public int SwingComboCount = 1; // 振りのコンボ数
        public int SwingType = 0;       // 振りの種類

        public int AttackType;

        public SoundStyle? UseSound;

        /// <summary>
        /// 刀の基本的なステータス
        /// </summary>
        /// <param name="item"></param>
        /// <param name="delay"> アクティブスキルのCD </param>
        /// <param name="type"> 振りの種類 </param>
        /// <param name="combo"> 振りのコンボ数 </param>
        public void SetKatanaDefaults(Item item, int delay, int? type = null, int combo = 1)
        {
            item.DamageType = DamageClass.Melee;
            item.useStyle = ItemUseStyleID.Shoot;

            item.UseSound = null;

            item.autoReuse = true;
            item.useTurn = false;
            item.noUseGraphic = true;
            item.noMelee = true;

            // 発射体が指定されていない場合、ダミーの発射体を発射する
            // Shoot()を適用させたいため
            item.shoot = item.shoot == ProjectileID.None ? ModContent.ProjectileType<Empty>() : item.shoot;
            item.shootSpeed = item.shootSpeed == 0f ? 1f : item.shootSpeed;

            Katana = true;
            ActiveSkillDelay = delay;

            // 何もしない場合汎用の振りが適用される
            SwingType = type ?? ModContent.ProjectileType<GeneralKatanaSwing>();
            SwingComboCount = combo;
        }

        /// <summary>
        /// クールダウンを有効化する
        /// </summary>
        /// <param name="player"></param>
        public void ActivateCooldown(Player player) => player.MKPlayer().ActiveSkillCD = player.MKPlayer().ActiveSkillCDMax = ActiveSkillDelay;

        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Katana)
            {
                item.StatsModifiedBy.Add(Mod);
            }
            if (item.type == ItemID.Muramasa)
            {
                item.StatsModifiedBy.Add(Mod);
            }

            SetDefaultsVanillaItem(item);

            if (item.ModItem is KatanaItem)
                (item.ModItem as KatanaItem).SetDefaultsItem();
        }

        private void SetDefaultsVanillaItem(Item item)
        {
            if (item.type == ItemID.Katana && MoreKatanaConfig.Instance.KatanaRework)
            {
                item.useTime = 30;
                item.useAnimation = 30;
                UseSound = SoundID.Item1;
                AltDamage = 36;
                SetKatanaDefaults(item, 60, ModContent.ProjectileType<KatanaHoldout>());
                item.autoReuse = false;
            }
            if (item.type == ItemID.Muramasa && MoreKatanaConfig.Instance.MuramasaRework)
            {
                UseSound = SoundID.Item1;
                AltDamage = 48;
                SetKatanaDefaults(item, 60, combo: 2);
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (Katana)
            {
                if (item.type is ItemID.Katana or ItemID.Muramasa)
                {
                    return player.MKPlayer().ActiveSkillCD == 0 && VanillaAltFunctionUse(item, player);
                }
                else
                {
                    return player.MKPlayer().ActiveSkillCD == 0 && (item.ModItem as KatanaItem).AltFunctionUseItem(player);
                }
            }
            return base.AltFunctionUse(item, player);
        }

        private bool VanillaAltFunctionUse(Item item, Player player)
        {
            if (item.type == ItemID.Muramasa)
            {
                return player.ownedProjectileCounts[ModContent.ProjectileType<MuramasaGhost>()] == 0;
            }

            return true;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            // NoUsingItemsのタイマーがある場合はアイテムを使用できない
            if (player.MKPlayer().NoUsingItems > 0)
                return false;

            if (Katana)
            {
                if (!Main.mouseItem.IsAir)
                    return false;

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
                    if (item.type is ItemID.Katana or ItemID.Muramasa)
                    {
                        // アイテムの設定を更新する
                        SetDefaultsVanillaItem(item);

                        // バニラアイテムのパッシブスキル
                        VanillaPassiveSkill(item, player, false);
                    }
                    else
                    {
                        // アイテムの設定を更新する
                        (item.ModItem as KatanaItem).SetDefaultsItem();

                        // Modアイテムのパッシブスキル
                        (item.ModItem as KatanaItem).PassiveSkill(player, false);
                    }
                }
                else
                {
                    AttackType = 0;
                }
            }
        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (Katana)
            {
                if (item.type == ItemID.Katana)
                    velocity = new Vector2(player.direction, 0);
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Katana)
            {
                if (!player.IsUsingAlt())
                {
                    Projectile.NewProjectile(source, position, velocity.Normalized(), SwingType, damage, knockback, player.whoAmI, AttackType);
                    AttackType = (AttackType + 1) % SwingComboCount;
                }
            }

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            if (Katana)
            {
                if (item.type == ItemID.Muramasa)
                {
                    if (Main.rand.NextBool(2))
                    {
                        int newDust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.DungeonWater);
                        Main.dust[newDust].noGravity = true;
                    }
                }
            }
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Katana)
            {
                if (item.type == ItemID.Muramasa)
                {
                    if (Main.myPlayer == player.whoAmI && (target == null || target.HittableForOnHitRewards()))
                    {
                        Vector2 vector = Main.rand.NextVector2Unit() * 80;
                        Projectile.NewProjectile(item.GetSource_FromThis(), target.Center + vector, -vector / 5, ModContent.ProjectileType<MuramasaSlash>(), item.damage / 2, item.knockBack, player.whoAmI);
                    }
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

        private void VanillaPassiveSkill(Item item, Player player, bool equipment)
        {
            if (item.type == ItemID.Muramasa)
            {
                player.MKPlayer().muramasaCounterattack = true;
            }
        }

        private void VanillaActiveSkill(Item item, Player player)
        {
            if (item.type == ItemID.Katana)
            {
                SoundEngine.PlaySound(SoundID.Item1, player.Center);
                Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, player.SafeDirectionTo(player.MKPlayer().MouseWorld), ModContent.ProjectileType<KatanaSlashHoldout>(), AltDamage, item.knockBack, player.whoAmI);
            }
            if (item.type == ItemID.Muramasa)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath33, player.Center);
                Projectile.NewProjectile(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<MuramasaGhost>(), AltDamage, item.knockBack, player.whoAmI);
            }
        }

        private bool Japanese;
        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (Katana)
            {
                if (line.Text.IsJapanese())
                    Japanese = true;

                if (line.Name == "DefaultText")
                {
                    Vector2 lineposition = new Vector2(line.OriginalX, line.OriginalY);
                    Utils.DrawBorderString(Main.spriteBatch, line.Text, lineposition, Color.LightGoldenrodYellow);
                    Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                    for (int i = 0; i < 4; i++)
                    {
                        float amount = 2f;
                        if (Japanese)
                            amount = 1.4f;

                        Vector2 drawpos = lineposition + new Vector2(0, amount * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(i * MathHelper.PiOver2);
                        Utils.DrawBorderString(Main.spriteBatch, line.Text, drawpos, Color.Goldenrod);
                    }
                    Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                    return false;
                }

                if (line.Name == "FunctionText")
                {
                    //line.X += 26;

                    string text = line.Text;
                    string[] linebreak = text.Split('\n');
                    Vector2 lineposition = new Vector2(line.OriginalX, line.OriginalY);

                    Utils.DrawBorderString(Main.spriteBatch, text, lineposition, line.Color);

                    int t = 0;
                    foreach (string i in linebreak)
                    {
                        if (i.Contains('<') && i.Contains('>') && !i.Contains('-'))
                        {
                            Color lineColor = line.Color;
                            string[] entry = i.Split('<');
                            if (i.Contains('[') && i.Contains(']'))
                            {
                                entry = i.Split('[');

                                Match matchedHex = Regex.Match(i, @"c/.*:");
                                string hex = matchedHex.ToString().Replace("c/", "#").Replace(":", "");
                                lineColor = hex.ColorFromHex();
                            }

                            Match matchedObject = Regex.Match(i, @"<.*>");

                            Vector2 entrySize = ChatManager.GetStringSize(FontAssets.MouseText.Value, entry[0], Vector2.One);
                            lineposition = new Vector2(line.OriginalX + entrySize.X, line.OriginalY + (entrySize.Y * t));
                            if (Japanese)
                                lineposition.Y -= 2f * t;

                            Texture2D bloom = MoreKatanaTextures.BloomTexture.Value;
                            Vector2 nameSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, matchedObject.ToString(), Vector2.One);
                            Main.spriteBatch.Draw(bloom, lineposition + nameSize / 2, null, lineColor with { A = 0 } * 0.4f, 0f, bloom.Size() / 2f, new Vector2(nameSize.X / 150, nameSize.Y / 150), SpriteEffects.None, 0);

                            Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);
                            for (int j = 0; j < 4; j++)
                            {
                                float amount = 2f;
                                if (Japanese)
                                    amount = 1.4f;

                                Vector2 drawpos = lineposition + new Vector2(0, amount * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 4) / 2)).RotatedBy(j * MathHelper.PiOver2);
                                Utils.DrawBorderString(Main.spriteBatch, matchedObject.ToString(), drawpos, Color.White * 0.5f);
                            }
                            Main.spriteBatch.SetEndBegin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                        }
                        t++;
                    }
                    return false;
                }
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
                // TO-DO: Onフックで処理するかも
                string defDamage = $"{item.damage}";
                if (item.type == ItemID.Katana)
                    defDamage = $"{item.damage}-{item.damage * 3}";

                TooltipLine dam = new TooltipLine(Mod, "NewDamage", $"{defDamage} / [c/FFB6C1:{AltDamage}] {item.DamageType.DisplayName}");
                tooltips.Insert(index + 1, dam);

                // 元々のダメージ表記のラインを非表示にする
                foreach (var i in tooltips)
                {
                    if (i.Name.EndsWith("Damage") && !i.Name.EndsWith("NewDamage"))
                        i.Hide();
                }

                // クールダウン表記
                TooltipLine cd = new TooltipLine(Mod, "CD", "- " + MoreKatanaUtil.GetTextValue("Tooltips.ActiveSkillCD") + ": " + ((float)ActiveSkillDelay / 60).ToString("F1") + " " + MoreKatanaUtil.GetTextValue("Tooltips.Second"));

                if (item.type is ItemID.Katana or ItemID.Muramasa)
                {
                    // "Material" なのはカタナとムラマサにはツールチップ用のラインが存在しないため
                    int index2 = tooltips.FindIndex(x => x.Name == "Material");
                    if (index2 < 0)
                        return;

                    TooltipLine tooltip0 = new TooltipLine(Mod, "Tooltip0", MoreKatanaUtil.GetTextValue("Items." + ItemID.Search.GetName(item.type) + ".Tooltip"));
                    tooltips.Insert(index2 + 1, tooltip0);

                    TooltipLine tip;
                    if (!ItemSlot.ShiftInUse)
                        tip = new TooltipLine(Mod, "DefaultText", MoreKatanaUtil.GetTextValue("Tooltips.DefaultText"));
                    else
                    {
                        tip = new TooltipLine(Mod, "FunctionText", MoreKatanaUtil.GetTextValue("Items." + ItemID.Search.GetName(item.type) + ".FunctionText"));

                        // クールダウンを挿入
                        tooltips.Insert(index2 + 1, cd);
                    }

                    // スキル説明を挿入
                    tooltips.Insert(index2 + 1, tip);
                }
                else
                {
                    int index2 = tooltips.FindIndex(x => x.Name == "Tooltip0");
                    if (index2 < 0)
                        return;

                    TooltipLine tip;
                    if (!ItemSlot.ShiftInUse)
                        tip = new TooltipLine(Mod, "DefaultText", MoreKatanaUtil.GetTextValue("Tooltips.DefaultText"));
                    else
                    {
                        tip = new TooltipLine(Mod, "FunctionText", (string)(item.ModItem as KatanaItem).FunctionText);

                        // クールダウンを挿入
                        tooltips.Insert(index2, cd);
                    }

                    // スキル説明を挿入
                    tooltips.Insert(index2, tip);
                }
            }
        }
    }
}