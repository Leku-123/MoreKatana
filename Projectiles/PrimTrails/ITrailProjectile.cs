using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class TrailManager
    {
        public void CreateTrail(Projectile projectile, Color trailColor, Texture2D trailTex, int trailWidth = 8, int trailCap = 15, float trailScroll = 0.05f, float trailAlpha = 0.5f)
        {
            TextureMapPrimTrail trail = new(projectile, trailColor, trailTex, trailWidth, trailCap, trailScroll, trailAlpha);
            MoreKatana.primitives.CreateTrail(trail);
        }
    }

    public interface IManualTrailProjectile
    {
        void DoTrailCreation(TrailManager tManager);

        bool DoTrailDeletion() => default;
    }

    public interface ITrailProjectile : IManualTrailProjectile
    {

    }
}