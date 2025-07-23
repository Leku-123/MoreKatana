using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

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