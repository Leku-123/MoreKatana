using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.UI
{
    public class ActiveSkillCooldownUI : ModSystem
    {
        private static Texture2D UITexture;

        public override void OnModLoad()
        {
            UITexture = ModContent.Request<Texture2D>("MoreKatana/UI/ActiveSkillCooldownUI", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            UITexture = null;
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
        }
    }
}