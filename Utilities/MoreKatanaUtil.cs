using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items;
using MoreKatana.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana
{
    public static partial class MoreKatanaUtil
    {
        public static MoreKatanaPlayer MKPlayer(this Player player) => player.GetModPlayer<MoreKatanaPlayer>();
        public static MoreKatanaGlobalItem MKItem(this Item item) => item.GetGlobalItem<MoreKatanaGlobalItem>();
        public static MoreKatanaGlobalNPC MKNPC(this NPC npc) => npc.GetGlobalNPC<MoreKatanaGlobalNPC>();
        public static MoreKatanaGlobalProjectile MKProjectile(this Projectile projectile) => projectile.GetGlobalProjectile<MoreKatanaGlobalProjectile>();

        public static Item ActiveItem(this Player player) => player.inventory[player.selectedItem];

        public static bool IsUsingAlt(this Player player) => player.altFunctionUse == 2;

        public static Vector2 PolarVector(float radius, float theta) => new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * radius;

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

        public static void ScreenShake(this Player player, int timer, int strength)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.ScreenShakeTimer = timer;
            mk.ScreenShakeStrength = strength;
        }

        public static void ScreenLock(this Player player, Entity entity, Vector2? screenLockPos = null)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.ScreenLockEntity = entity;
            mk.ScreenLockPos = screenLockPos == null ? entity.Center : (Vector2)screenLockPos;
        }

        public static void GeneralDashEffect(this Player player, Vector2 direction, int distance, float timer, bool stop = false)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.GeneralDash = true;
            mk.DashDirection = direction;
            mk.DashDistance = distance;
            mk.DashTimerMax = timer;
            mk.SuddenStop = stop;
        }

        public static void FlipEffect(this Player player, float value)
        {
            MoreKatanaPlayer mk = player.MKPlayer();
            mk.Flipping = value;
        }

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

        public static void CreateDashSlash(this Player player, IEntitySource source, int damage, float knockBack, int distance, float timer, Vector2? dir = null, bool stop = true)
        {
            int p = Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<GeneralDashSlash>(), damage, knockBack, player.whoAmI);
            GeneralDashSlash dash = (GeneralDashSlash)Main.projectile[p].ModProjectile;
            dash.DashDirection = dir == null ? player.SafeDirectionTo(Main.MouseWorld) : (Vector2)dir;
            dash.DashDistance = distance;
            dash.DashTimerMax = timer;
            dash.SuddenStop = stop;
        }

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
    }
}