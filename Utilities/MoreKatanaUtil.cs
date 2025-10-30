using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items;
using MoreKatana.Projectiles;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana
{
    public static partial class MoreKatanaUtil
    {
        #region -------- General Extension Utils --------
        public static MoreKatanaPlayer MKPlayer(this Player player) => player.GetModPlayer<MoreKatanaPlayer>();
        public static MoreKatanaGlobalItem MKItem(this Item item) => item.GetGlobalItem<MoreKatanaGlobalItem>();
        public static MoreKatanaGlobalNPC MKNPC(this NPC npc) => npc.GetGlobalNPC<MoreKatanaGlobalNPC>();
        public static MoreKatanaGlobalProjectile MKProj(this Projectile projectile) => projectile.GetGlobalProjectile<MoreKatanaGlobalProjectile>();
        #endregion

        #region -------- Player Utils --------
        /// <summary> プレイヤーが現在選択しているアイテム </summary>
        //public static Item ActiveItem(this Player player) => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
        public static Item ActiveItem(this Player player) => player.HeldItem;

        /// <summary> "Player.altFunctionUse == 2"の簡略型 </summary>
        public static bool IsUsingAlt(this Player player) => player.altFunctionUse == 2;

        /// <summary>
        /// 発射体をホールド出来ない状態かどうかのチェック
        /// </summary>
        /// <param name="player"></param>
        /// <param name="needsToHold"> channelを考えない場合はここをfalseにします </param>
        /// <returns></returns>
        public static bool CantUseHoldout(this Player player, bool needsToHold = true) => player == null || !player.active || player.dead || (!player.channel && needsToHold) || player.CCed || player.noItems;

        /// <summary>
        /// スクリーンシェイク
        /// </summary>
        /// <param name="player"></param>
        /// <param name="timer"> 揺らす時間 </param>
        /// <param name="strength"> 揺らす強度 </param>
        public static void ScreenShake(this Player player, int timer, int strength)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.ScreenShakeTimer = timer;
            mk.ScreenShakeStrength = strength;
        }

        /// <summary>
        /// フリップエフェクト
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"> どの程度フリップさせるか </param>
        public static void FlipEffect(this Player player, float value)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.Flipping = value;
        }

        /// <summary>
        /// ダッシュエフェクト
        /// </summary>
        /// <param name="player"></param>
        /// <param name="direction"> ダッシュの方向 </param>
        /// <param name="distance"> ダッシュの距離 </param>
        /// <param name="timer"> ダッシュの時間 </param>
        /// <param name="stop"> ダッシュ後に勢いが止まるかどうか </param>
        public static void GeneralDashEffect(this Player player, Vector2 direction, int distance, float timer, bool stop = false)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.GeneralDash = true;
            mk.DashDirection = direction;
            mk.DashDistance = distance;
            mk.DashTimerMax = timer;
            mk.SuddenStop = stop;
        }

        /// <summary>
        /// プレイヤーの回転
        /// </summary>
        /// <param name="player"></param>
        /// <param name="count"> 回転数 </param>
        /// <param name="direction"> 回転の向き </param>
        /// <param name="timer"> 回転に要する時間 </param>
        public static void UpdateRotation(this Player player, int count, int direction, float timer)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.Rolling = true;
            mk.RollingCount = count;
            mk.RollingDirection = direction;
            mk.RollingTimerMax = timer;
        }

        /// <summary> プレイヤーの色を変更する </summary>
        public static void DrawColorEffect(this Player player, float r, float g, float b, float a)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.R = r;
            mk.G = g;
            mk.B = b;
            mk.A = a;
        }
        public static void DrawColorEffect(this Player player, float value) => player.DrawColorEffect(value, value, value, value);
        public static void DrawColorEffect(this Player player, Vector3 rgb, float a = 1) => player.DrawColorEffect(rgb.X, rgb.Y, rgb.Z, a);

        public static void FullBright(this Player player)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.FullBright = true;
        }
        #endregion

        #region -------- Projectile Utils --------
        /// <summary>
        /// 発射体のヒットボックスを変更する
        /// </summary>
        public static void ExpandHitboxBy(this Projectile projectile, int width, int height)
        {
            projectile.position = projectile.Center;
            projectile.width = width;
            projectile.height = height;
            projectile.position -= projectile.Size * 0.5f;
        }
        public static void ExpandHitboxBy(this Projectile projectile, int newSize) => projectile.ExpandHitboxBy(newSize, newSize);
        public static void ExpandHitboxBy(this Projectile projectile, Vector2 newSize) => projectile.ExpandHitboxBy((int)newSize.X, (int)newSize.Y);
        public static void ExpandHitboxBy(this Projectile projectile, float expandRatio) => projectile.ExpandHitboxBy((int)(projectile.width * expandRatio), (int)(projectile.height * expandRatio));

        public static Projectile ProjectileExists(int whoAmI, params int[] types)
        {
            return whoAmI > -1 && whoAmI < Main.maxProjectiles && Main.projectile[whoAmI].active && (types.Length == 0 || types.Contains(Main.projectile[whoAmI].type)) ? Main.projectile[whoAmI] : null;
        }

        public static Projectile ProjectileExists(float whoAmI, params int[] types)
        {
            return ProjectileExists((int)whoAmI, types);
        }

        public static Projectile CreateShockwave(IEntitySource source, Vector2 position, int owner = 255, int rippleCount = 10, int rippleSize = 5, float rippleSpeed = 15f)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int p = Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<ShockWaveEffect>(), 0, 0f, owner);
                ShockWaveEffect shockwave = (ShockWaveEffect)Main.projectile[p].ModProjectile;
                shockwave.RippleCount = rippleCount;
                shockwave.RippleSize = rippleSize;
                shockwave.RippleSpeed = rippleSpeed;

                return p < Main.maxProjectiles ? Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<ShockWaveEffect>()] == 0 ? Main.projectile[p] : null : null;
            }
            return null;
        }

        /// <summary>
        /// 発射体を全方位に発射させる
        /// </summary>
        /// <param name="source"></param>
        /// <param name="spawnPosition"> 発射体がスポーンする位置 </param>
        /// <param name="projVelocity"> 発射体の速度 </param>
        /// <param name="amount"> 何方位に発射するか </param>
        /// <param name="projType"> 発射体のタイプ </param>
        /// <param name="damage"></param>
        /// <param name="knockback"></param>
        /// <param name="owner"></param>
        public static void ProjectileSplitInAllDirections(IEntitySource source, Vector2 spawnPosition, float projVelocity, int amount, int projType, int damage, float knockback, int owner, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f)
        {
            for (int i = 0; i < amount; i++)
            {
                float rad = MathHelper.TwoPi / amount * i;
                Vector2 vector = Vector2.UnitY.RotatedBy(rad);
                vector *= projVelocity;
                Projectile.NewProjectile(source, spawnPosition, vector, projType, damage, knockback, owner, ai0, ai1, ai2);
            }
        }
        #endregion

        #region -------- NPC Utils --------
        /// <summary>
        /// 指定した地点から近くの敵対NPCを取得する
        /// </summary>
        /// <param name="origin"> NPCをチェックする位置 </param>
        /// <param name="maxDistanceToCheck"> originを中心にチェックする距離 </param>
        /// <param name="ignoreTiles"> ターゲットを見つけるときにタイルを無視するかどうか </param>
        /// <param name="bossPriority"> ボスを優先的に狙うべきかどうか </param>
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
        {
            NPC closestTarget = null; // ターゲット
            float distance = maxDistanceToCheck; // 索敵範囲

            if (bossPriority) // ボス優先の場合
            {
                bool bossFound = false; // falseの場合、有効なボスのターゲットはまだ見つかっていない。
                for (int index = 0; index < Main.npc.Length; index++) // 全NPCを確認
                {
                    // 有効なボスのターゲットが見つかった場合、ボス以外のターゲットはすべて無視する。
                    if (bossFound && !(Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye))
                        continue;

                    if (Main.npc[index].CanBeChasedBy(null, false)) // NPCがホーミング可能なNPCかどうか
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2); // ターゲットの位置

                        // ターゲットにヒットできるかを確認する
                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        // ターゲットとの距離がdistanceより小さく、ターゲットにヒットできる場合
                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            if (Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye)
                                bossFound = true; // 有効なボスのターゲットが見つかった場合trueにする

                            distance = Vector2.Distance(origin, Main.npc[index].Center); // 索敵範囲をターゲットとの距離で更新
                            closestTarget = Main.npc[index]; // ターゲットのNPCを更新
                        }
                    }
                }
            }
            else
            {
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    if (Main.npc[index].CanBeChasedBy(null, false))
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            return closestTarget;
        }

        public static NPC ClosestNPCfromTwoPoints(Vector2 point1, Vector2 point2, float maxDistanceToCheck)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck + Vector2.Distance(point1, point2);

            for (int index = 0; index < Main.npc.Length; index++)
            {
                if (!(Vector2.Distance(point1, Main.npc[index].Center) < maxDistanceToCheck))
                    continue;

                if (Main.npc[index].CanBeChasedBy(null, false))
                {
                    if (Vector2.Distance(point2, Main.npc[index].Center) < distance)
                    {
                        distance = Vector2.Distance(point2, Main.npc[index].Center);
                        closestTarget = Main.npc[index];
                    }
                }
            }
            return closestTarget;
        }
        #endregion

        #region -------- Drawing Utils --------
        public static void SetEndBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
        {
            spriteBatch.End();
            spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        }
        public static void SetEndBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
            => spriteBatch.SetEndBegin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Main.UIScaleMatrix);
        public static void SetEndBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode, BlendState blendState)
            => spriteBatch.SetEndBegin(sortMode, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        /// <summary>
        /// リング状にダストをスポーンする
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="density"></param>
        /// <param name="speed"></param>
        /// <param name="color"></param>
        /// <param name="dustScale"></param>
        /// <param name="noLight"></param>
        public static void DrawRing(Vector2 position, int[] dustType, int density, float speed, Color color = default, float dustScale = 1f, bool noLight = false)
        {
            for (int i = 0; i < density; i++)
            {
                int type = Utils.SelectRandom(Main.rand, dustType);
                Vector2 velocity = speed * Vector2.UnitY.RotatedBy(MathHelper.TwoPi / density * i);
                int d = Dust.NewDust(position, 0, 0, type, newColor: color);
                Main.dust[d].noLight = noLight;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = velocity;
                Main.dust[d].scale = dustScale;
            }
        }

        /// <summary>
        /// 円形にダストをスポーンする。楕円も可能
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="mainSize"></param>
        /// <param name="RatioX"></param>
        /// <param name="RatioY"></param>
        /// <param name="dustDensity"></param>
        /// <param name="dustSize"></param>
        /// <param name="randomAmount"></param>
        /// <param name="rotationAmount"></param>
        /// <param name="noGravity"></param>
        public static void DrawCircle(Vector2 position, int dustType, float mainSize = 1, float RatioX = 1, float RatioY = 1, float dustDensity = 1, float dustSize = 1f, float randomAmount = 0, float rotationAmount = 0, bool noGravity = false)
        {
            float rot;
            if (rotationAmount < 0) { rot = Main.rand.NextFloat(0, (float)Math.PI * 2); } else { rot = rotationAmount; }

            float density = 1 / dustDensity * 0.1f;

            for (float k = 0; k < 6.28f; k += density)
            {
                float rand = 0;
                if (randomAmount > 0) { rand = Main.rand.NextFloat(-0.01f, 0.01f) * randomAmount; }

                float x = (float)Math.Cos(k + rand) * RatioX;
                float y = (float)Math.Sin(k + rand) * RatioY;
                if (dustType == 222 || dustType == 130 || noGravity)
                {
                    Dust.NewDustPerfect(position, dustType, new Vector2(x, y).RotatedBy(rot) * mainSize, 0, default, dustSize).noGravity = true;
                }
                else
                {
                    Dust.NewDustPerfect(position, dustType, new Vector2(x, y).RotatedBy(rot) * mainSize, 0, default, dustSize);
                }
            }
        }

        public static void DrawElectricity(Vector2 point1, Vector2 point2, int dusttype, float scale = 1, int armLength = 30, Color color = default, float density = 0.05f)
        {
            int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
            Vector2[] nodes = new Vector2[nodeCount + 1];

            nodes[nodeCount] = point2;

            for (int k = 1; k < nodes.Length; k++)
            {
                nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
                    (k == nodes.Length - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * Main.rand.NextFloat(-armLength / 2, armLength / 2));

                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];
                for (float i = 0; i < 1; i += density)
                {
                    Dust d = Dust.NewDustPerfect(Vector2.Lerp(prevPos, nodes[k], i), dusttype, Vector2.Zero, 0, color, scale);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// テクスチャの背面にアウトラインのようなテクスチャを描画する
        /// 光っているような演出にしたり、拍動するような演出にしたりなど
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="drawPosition"></param>
        /// <param name="frame"></param>
        /// <param name="backglowColor"></param>
        /// <param name="rotation"></param>
        /// <param name="backglowArea"></param>
        /// <param name="scale"></param>
        /// <param name="spriteEffects"></param>
        public static void DrawBackglow(Texture2D texture, Vector2 drawPosition, Rectangle frame, Color backglowColor, float rotation, float backglowArea, Vector2 scale, SpriteEffects spriteEffects)
        {
            Vector2 origin = frame.Size() * 0.5f;
            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * backglowArea;
                Main.EntitySpriteDraw(texture, drawPosition + backglowOffset, frame, backglowColor, rotation, origin, scale, spriteEffects, 0f);
            }
        }

        /// <summary>
        /// 簡素なゲージを描画する。自動でスクリーン位置に変換される
        /// </summary>
        /// <param name="drawPos"></param>
        /// <param name="ratio"></param>
        /// <param name="frontColor"></param>
        /// <param name="lightColor"></param>
        /// <param name="backColor"></param>
        /// <param name="dustType"></param>
        public static void DrawGauge(Vector2 drawPos, float ratio, Color frontColor, Color? lightColor = null, Color? backColor = null, int dustType = -1, Vector2? size = null)
        {
            Texture2D texture = TextureAssets.MagicPixel.Value;
            Vector2 backSize = size ?? new Vector2(54, 8);
            Vector2 frontSize = new Vector2(backSize.X - 4, backSize.Y - 4);
            Vector2 backPos = new Vector2(drawPos.X - backSize.X * 0.5f, drawPos.Y - backSize.Y * 0.5f) - Main.screenPosition;
            Vector2 frontPos = new Vector2(drawPos.X - frontSize.X * 0.5f, drawPos.Y - frontSize.Y * 0.5f) - Main.screenPosition;
            Color c1 = backColor ?? Color.Black;
            Color c2 = frontColor;
            Color c3 = lightColor ?? Color.White;

            Main.spriteBatch.Draw(texture, backPos, new Rectangle(0, 0, 1, 1), c1, 0f, Vector2.Zero, new Vector2(backSize.X, backSize.Y), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, frontPos, new Rectangle(0, 0, 1, 1), c2, 0f, Vector2.Zero, new Vector2(frontSize.X * ratio, frontSize.Y), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, frontPos, new Rectangle(0, 0, 1, 1), c3 * 0.5f, 0f, Vector2.Zero, new Vector2(frontSize.X * ratio, frontSize.Y / 2), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, frontPos, new Rectangle(0, 0, 1, 1), c3 * 0.5f, 0f, Vector2.Zero, new Vector2(frontSize.X * ratio, frontSize.Y / 4), SpriteEffects.None, 0f);

            if (dustType != -1)
            {
                for (int i = 0; i < 2; i++)
                {
                    int newDust = Dust.NewDust(new Vector2(frontPos.X + (frontSize.X * ratio) - 3f, frontPos.Y - 3f), 1, (int)frontSize.Y * 2, dustType, Main.rand.Next(2, 3), 0, 150, default, 0.5f);
                    Main.dust[newDust].fadeIn = 0;
                    Main.dust[newDust].noGravity = true;
                }
            }
        }

        /// <summary>
        /// スパークルを描画する。発射体などの演出に
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="dir"></param>
        /// <param name="drawpos"></param>
        /// <param name="drawColor"></param>
        /// <param name="shineColor"></param>
        /// <param name="flareCounter"></param>
        /// <param name="fadeInStart"></param>
        /// <param name="fadeInEnd"></param>
        /// <param name="fadeOutStart"></param>
        /// <param name="fadeOutEnd"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="fatness"></param>
        public static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawpos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
        {
            Texture2D texture2D = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Color color1 = shineColor * opacity * 0.5f;
            color1.A = 0;
            Vector2 origin = texture2D.Size() / 2f;
            Color color2 = drawColor * 0.5f;
            float num = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, true);
            Vector2 scale1 = new Vector2((float)(fatness.X * 0.5), scale.X) * num;
            Vector2 scale2 = new Vector2((float)(fatness.Y * 0.5), scale.Y) * num;
            Color color3 = color1 * num;
            Color color4 = color2 * num;
            Main.EntitySpriteDraw(texture2D, drawpos, new Rectangle?(), color3, 1.570796f + rotation, origin, scale1, dir);
            Main.EntitySpriteDraw(texture2D, drawpos, new Rectangle?(), color3, 0.0f + rotation, origin, scale2, dir);
            Main.EntitySpriteDraw(texture2D, drawpos, new Rectangle?(), color4, 1.570796f + rotation, origin, scale1 * 0.6f, dir);
            Main.EntitySpriteDraw(texture2D, drawpos, new Rectangle?(), color4, 0.0f + rotation, origin, scale2 * 0.6f, dir);
        }

        /// <summary>
        /// テクスチャマッピングで圧縮、引き延ばしをする
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="opacity"></param>
        /// <param name="Scale"></param>
        /// <param name="Direction"></param>
        /// <param name="CircularRotation"></param>
        /// <param name="blendMode"></param>
        public static void DrawCompression(Texture2D texture, Color color, float rotation, float opacity, Vector2 Scale, float Direction, float CircularRotation, BlendState blendMode)
        {
            Main.spriteBatch.SetEndBegin(SpriteSortMode.Immediate, blendMode);
            Matrix viewMatrix;
            Matrix projectionMatrix;
            ShaderHelpers.CalculatePerspectiveMatricies(out viewMatrix, out projectionMatrix, 0);
            GameShaders.Misc["Compression"].UseColor(color);
            GameShaders.Misc["Compression"].UseSaturation(rotation);
            GameShaders.Misc["Compression"].UseOpacity(opacity);
            GameShaders.Misc["Compression"].Shader.Parameters["usc"].SetValue(Scale);
            GameShaders.Misc["Compression"].Shader.Parameters["uDirection"].SetValue((float)Direction);
            GameShaders.Misc["Compression"].Shader.Parameters["uCircularRotation"].SetValue(CircularRotation);
            GameShaders.Misc["Compression"].Shader.Parameters["uImageSize0"].SetValue(Utils.Size(texture));
            GameShaders.Misc["Compression"].Shader.Parameters["overallImageSize"].SetValue(Utils.Size(texture));
            GameShaders.Misc["Compression"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["Compression"].Apply(default);
        }

        /// <summary>
        /// <see cref="Texture2D"/> から全ての色を取得し<see cref="Color"/>配列として返す
        /// </summary>
        /// <param name="texture"> 読み込むテクスチャ </param>
        /// <returns></returns>
        public static Color[] GetColors(this Texture2D texture)
        {
            int x = texture.Width;
            int y = texture.Height;
            Color[] xy = new Color[x * y];
            texture.GetData(xy); // テクスチャ内のすべての色で色の配列を埋める
            return xy;
        }

        public static Color ToXnaColor(this in System.Drawing.Color c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static Color ColorFromHex(this string hex)
        {
            return System.Drawing.ColorTranslator.FromHtml(hex).ToXnaColor();
        }

        public static bool GetHexColor(string hexString, out Vector3 hsl)
        {
            if (hexString.StartsWith("#"))
            {
                hexString = hexString.Substring(1);
            }
            uint result = default(uint);
            if (hexString.Length <= 6 && uint.TryParse(hexString, (NumberStyles)515, (IFormatProvider)(object)CultureInfo.CurrentCulture, out result))
            {
                uint b = result & 0xFFu;
                uint g = (result >> 8) & 0xFFu;
                uint r = (result >> 16) & 0xFFu;
                hsl = RgbToScaledHsl(new Color((int)r, (int)g, (int)b));
                return true;
            }
            hsl = Vector3.Zero;
            return false;
        }
        public static Vector3 RgbToScaledHsl(Color color)
        {
            Vector3 vector = Main.rgbToHsl(color);
            vector.Z = (vector.Z - 0.15f) / 0.85f;
            return Vector3.Clamp(vector, Vector3.Zero, Vector3.One);
        }
        public static Color ScaledHslToRgb(Vector3 hsl)
        {
            return ScaledHslToRgb(hsl.X, hsl.Y, hsl.Z);
        }
        public static Color ScaledHslToRgb(float hue, float saturation, float luminosity)
        {
            return Main.hslToRgb(hue, saturation, luminosity * 0.85f + 0.15f, 255);
        }
        #endregion

        #region -------- Localization Utils --------
        /// <summary>
        /// ローカライズの簡略化
        /// 指定されたキーの前に"Mods.MoreKatana."を付けてローカライズを取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTextValue(string key)
        {
            return Language.GetTextValue("Mods.MoreKatana." + key);
        }
        public static LocalizedText GetOrRegister(string key)
        {
            return Language.GetOrRegister("Mods.MoreKatana." + key);
        }

        public static bool IsJapanese(string text)
        {
            return Regex.IsMatch(text, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]+"); ;
        }
        #endregion

        #region -------- Misc Utils --------
        public static Vector2 TurnRight(this Vector2 vec) => new Vector2(-vec.Y, vec.X);

        public static Vector2 TurnLeft(this Vector2 vec) => new Vector2(vec.Y, -vec.X);

        public static Vector2 Normalized(this Vector2 vec) => vec == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vec);

        /// <summary>
        /// エンティティの中心を基準として任意の目的地に向かう単位ベクトルを取得する。<see cref="float.NaN"/>の安全性をfallbackの形で持っている
        /// </summary>
        /// <param name="entity"> チェックする対象のエンティティ </param>
        /// <param name="destination"> 目的地に向かう方向 </param>
        /// <param name="fallback"> 安全でない正規化が行われた場合に使用するfallback値 </param>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2? fallback = null)
        {
            if (!fallback.HasValue)
                fallback = Vector2.Zero;

            return (destination - entity.Center).SafeNormalize(fallback.Value);
        }

        public static Vector2 CollisionPoint(Vector2 point1, Vector2 point2)
        {
            float[] scanarray = new float[3];
            float dist = point1.Distance(point2);
            Collision.LaserScan(point1, point1.DirectionTo(point2), 0, dist, scanarray);
            dist = 0;

            foreach (float fl in scanarray)
                dist += fl / scanarray.Length;

            return point1 + point1.DirectionTo(point2) * dist;
        }

        public static int ToDirection(this float dir)
        {
            if (dir >= 0)
                return 1;
            else
                return -1;
        }

        /// <summary>
        /// オブジェクトの名前空間と名前からTextureを手動で取得します
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetTexture(this object type, string name) => (type.GetType().Namespace + "." + name).Replace('.', '/');

        /// <summary>
        /// CameraModifierを追加します
        /// </summary>
        /// <param name="modifier"></param>
        public static void AddCameraModifier(this ICameraModifier modifier) => Main.instance.CameraModifiers.Add(modifier);

        /// <summary>
        /// デバッグ用
        /// </summary>
        /// <param name="value"></param>
        public static void InChatText(this object value) => Main.NewText($"{value}");
        #endregion
    }
}