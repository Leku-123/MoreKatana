using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MoreKatana.Projectiles.PrimTrails
{
    public class TrailManager
    {
        public void CreateTrail(Projectile projectile, Color trailColor, Texture2D trailTex, int trailWidth, int trailCap)
        {
            TextureMapPrimTrail trail = new(projectile, trailColor, trailTex, trailWidth, trailCap);
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