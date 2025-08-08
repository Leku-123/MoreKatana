using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items;
using MoreKatana.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoreKatana
{
    /// <summary>
    /// 今後分ける可能性あるのでpartialです
    /// </summary>
    public static partial class MoreKatanaUtil
    {
        #region -------- General Extension Utils --------
        public static MoreKatanaPlayer MKPlayer(this Player player) => player.GetModPlayer<MoreKatanaPlayer>();
        public static MoreKatanaGlobalItem MKItem(this Item item) => item.GetGlobalItem<MoreKatanaGlobalItem>();
        public static MoreKatanaGlobalNPC MKNPC(this NPC npc) => npc.GetGlobalNPC<MoreKatanaGlobalNPC>();
        public static MoreKatanaGlobalProjectile MKProjectile(this Projectile projectile) => projectile.GetGlobalProjectile<MoreKatanaGlobalProjectile>();
        #endregion

        #region -------- Player Utils --------
        /// <summary>
        /// プレイヤーが現在選択しているアイテム
        /// </summary>
        public static Item ActiveItem(this Player player) => player.HeldItem;

        public static bool IsUsingAlt(this Player player) => player.altFunctionUse == 2;

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
        /// スクリーンロック
        /// </summary>
        /// <param name="player"></param>
        /// <param name="entity"> 固定するフラグを立てるエンティティ </param>
        /// <param name="screenLockPos"> 固定する位置 </param>
        public static void ScreenLock(this Player player, Entity entity, Vector2? screenLockPos = null)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.ScreenLockEntity = entity;
            mk.ScreenLockPos = screenLockPos == null ? entity.Center : (Vector2)screenLockPos;
        }

        /// <summary>
        /// フリップエフェクト
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
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

        /// <summary>
        /// ダッシュ切り発射体を簡単に処理する
        /// <param name="source">はItemUse系にしてください
        /// </summary>
        /// <param name="player"></param>
        /// <param name="source"> ItemUse系 </param>
        /// <param name="damage"></param>
        /// <param name="knockBack"></param>
        /// <param name="distance"> ダッシュの距離 </param>
        /// <param name="timer"> ダッシュの時間 </param>
        /// <param name="dir"> ダッシュの方向。デフォルトはマウス方向 </param>
        /// <param name="stop"> ダッシュ後に勢いが止まるかどうか </param>
        public static void CreateDashSlash(this Player player, IEntitySource source, int damage, float knockBack, int distance, float timer, Vector2? dir = null, bool stop = true)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                int p = Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<GeneralDashSlash>(), damage, knockBack, player.whoAmI);
                GeneralDashSlash dash = (GeneralDashSlash)Main.projectile[p].ModProjectile;
                dash.DashDirection = dir == null ? player.SafeDirectionTo(Main.MouseWorld) : (Vector2)dir;
                dash.DashDistance = distance;
                dash.DashTimerMax = timer;
                dash.SuddenStop = stop;
            }
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
        public static void ProjectileSplitInAllDirections(IEntitySource source, Vector2 spawnPosition, float projVelocity, int amount, int projType, int damage, float knockback, int owner)
        {
            for (int i = 0; i < amount; i++)
            {
                float rad = MathHelper.TwoPi / amount * i;
                Vector2 vector = Vector2.UnitY.RotatedBy(rad);
                vector *= projVelocity;
                Projectile.NewProjectile(source, spawnPosition, vector, projType, damage, knockback, owner);
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
        #endregion

        #region -------- Drawing Utils --------
        /// <summary>
        /// リング状にダストをスポーンする
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="density"></param>
        /// <param name="speed"></param>
        /// <param name="color"></param>
        /// <param name="dustSize"></param>
        /// <param name="noLight"></param>
        public static void DrawRing(Vector2 position, int dustType, int density, float speed, Color color = default, float dustSize = 1f, bool noLight = false)
        {
            for (int i = 0; i < density; i++)
            {
                Vector2 velocity = speed * Vector2.UnitY.RotatedBy(MathHelper.TwoPi / density * i);
                int d = Dust.NewDust(position, 0, 0, dustType, newColor: color);
                Main.dust[d].noLight = noLight;
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = velocity;
                Main.dust[d].scale = dustSize;
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
            Texture2D texture2D = TextureAssets.Extra[98].Value;
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
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
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
        #endregion

        #region -------- Localization Utils --------
        /// <summary>
        /// ローカライズの簡略化
        /// 指定されたキーの前に"Mods.MoreKatana."を付けてローカライズを指定する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTextValue(string key)
        {
            return Language.GetTextValue("Mods.MoreKatana." + key);
        }
        #endregion

        #region -------- Misc Utils --------
        public static Vector2 TurnRight(this Vector2 vec) => new Vector2(-vec.Y, vec.X);

        public static Vector2 TurnLeft(this Vector2 vec) => new Vector2(vec.Y, -vec.X);

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
        #endregion

        #region -------- Debug --------
        public static void InChatText(this int value) => Main.NewText($"{value}");
        public static void InChatText(this float value) => Main.NewText($"{value}");
        public static void InChatText(this bool value) => Main.NewText($"{value}");
        public static void InChatText(this Vector2 value) => Main.NewText($"{value}");
        public static void InChatText(this string value) => Main.NewText($"{value}");
        #endregion
    }
}