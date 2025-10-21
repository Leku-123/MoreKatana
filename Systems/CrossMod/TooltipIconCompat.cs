using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MoreKatana.Systems.CrossMod
{
    public class TooltipIconCompat : ModSystem
    {
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("TooltipIcon", out Mod tooltipIcon))
            {
                tooltipIcon.Call("AddNormalIcon", "MoreKatana", "NewDamage", ModContent.Request<Texture2D>("TooltipIcon/Textures/Weapons/" + "MeleeDamage", AssetRequestMode.AsyncLoad));
            }
        }
    }
}