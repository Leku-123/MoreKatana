using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace MoreKatana.Prim
{
    public class PrimTrailManager
    {
        public const int DrawProjectile = 1;
        public const int DrawNPC = 2;
        public List<PrimTrail> _trails = new List<PrimTrail>();

        public RenderTarget2D primTargetNPC;
        public RenderTarget2D primTargetProjectile;

        public void LoadContent(GraphicsDevice GD)
        {
            Main.QueueMainThreadAction(() =>
            {
                InitializeTargets(GD);
            });
        }

        public void InitializeTargets(GraphicsDevice GD)
        {
            primTargetNPC = new RenderTarget2D(GD, Main.screenWidth / 2, Main.screenHeight / 2);
            primTargetProjectile = new RenderTarget2D(GD, Main.screenWidth / 2, Main.screenHeight / 2);
        }

        public void DrawTargetNPC(SpriteBatch spriteBatch)
        {
            List<PrimTrail> primTrails = new List<PrimTrail>();
            foreach (PrimTrail trail in _trails.ToArray().Where(x => x.DrawType == DrawNPC))
            {
                if (!trail.Disabled)
                    primTrails.Add(trail);
            }

            foreach (PrimTrail trail in primTrails)
                trail.Draw();
        }

        public void DrawTargetProj(SpriteBatch spriteBatch)
        {
            List<PrimTrail> primTrails = new List<PrimTrail>();
            foreach (PrimTrail trail in _trails.ToArray().Where(x => x.DrawType == DrawProjectile))
            {
                if (!trail.Disabled)
                    primTrails.Add(trail);
            }

            foreach (PrimTrail trail in primTrails)
                trail.Draw();
        }

        public void UpdateTrails()
        {
            foreach (PrimTrail trail in _trails.ToArray())
                trail.Update();
        }

        public void CreateTrail(PrimTrail trail)
        {
            if (!Main.dedServ) // サーバーにトレイルを作成しない
                _trails.Add(trail);
        }
    }
}