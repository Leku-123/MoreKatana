using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MoreKatana.Systems.CrossMod
{
    /// <summary>
    /// Tooltip IconのModサポート
    /// Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3582340033
    /// </summary>
    public class TooltipIconCompat : ModSystem
    {
        /// <summary>
        /// ツールチップをDamageからNewDamageにしているためここで適用する
        /// </summary>
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("TooltipIcon", out Mod tooltipIcon))
            {
                tooltipIcon.Call("AddNormalIcon", "MoreKatana", "NewDamage", ModContent.Request<Texture2D>("TooltipIcon/Textures/Weapons/" + "MeleeDamage", AssetRequestMode.AsyncLoad));
            }
        }
    }
}