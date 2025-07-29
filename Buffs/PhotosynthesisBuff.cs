using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MoreKatana.Buffs
{
    public class PhotosynthesisBuff : ModBuff
    {
        private const int TextureFrames = 2;
        private int Frame;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (Main.dayTime)
            {
                player.statDefense += 3;
                player.lifeRegen += 5;
                Frame = 0;
            }
            else
            {
                Lighting.AddLight(player.position, Color.LimeGreen.ToVector3());
                Frame = 1;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
        {
            Texture2D texture = drawParams.Texture;
            Vector2 position = drawParams.Position;
            Rectangle rectangle = texture.Frame(1, TextureFrames, 0, Frame);
            Color color = drawParams.DrawColor;
            spriteBatch.Draw(texture, position, rectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            drawParams.MouseRectangle.Height /= TextureFrames;
            return false;
        }
    }
}