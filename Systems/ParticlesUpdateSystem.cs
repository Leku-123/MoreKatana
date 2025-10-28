using MoreKatana.Particles;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Systems
{
    public class ParticlesUpdateSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            if (!Main.dedServ)
            {
                ParticleHandler.RunRandomSpawnAttempts();
                ParticleHandler.UpdateAllParticles();
            }
        }
    }
}