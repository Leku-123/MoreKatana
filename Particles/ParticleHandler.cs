using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;

namespace MoreKatana.Particles
{
    public static class ParticleHandler
    {
        private static readonly int MaxParticlesAllowed = (int)MoreKatanaConfig.Instance.MaxParticles;

        private static Particle[] particles;
        private static int nextVacantIndex;
        private static int activeParticles;
        private static Dictionary<Type, int> particleTypes;
        private static Dictionary<int, Texture2D> particleTextures;
        private static List<Particle> particleInstances;
        private static List<Particle> batchedAlphaBlendParticles;
        private static List<Particle> batchedAdditiveBlendParticles;

        internal static void RegisterParticles()
        {
            particles = new Particle[MaxParticlesAllowed];
            particleTypes = new Dictionary<Type, int>();
            particleTextures = new Dictionary<int, Texture2D>();
            particleInstances = new List<Particle>();
            batchedAlphaBlendParticles = new List<Particle>(MaxParticlesAllowed);
            batchedAdditiveBlendParticles = new List<Particle>(MaxParticlesAllowed);

            Type baseParticleType = typeof(Particle);
            MoreKatana MmoreKatana = ModContent.GetInstance<MoreKatana>();

            foreach (Type type in MmoreKatana.Code.GetTypes())
            {
                if (type.IsSubclassOf(baseParticleType) && !type.IsAbstract && type != baseParticleType)
                {
                    int assignedType = particleTypes.Count;
                    particleTypes[type] = assignedType;

                    string texturePath = type.Namespace.Replace('.', '/') + "/" + type.Name;
                    particleTextures[assignedType] = ModContent.Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

                    particleInstances.Add((Particle)RuntimeHelpers.GetUninitializedObject(type));
                }
            }
        }

        internal static void Unload()
        {
            particles = null;
            particleTypes = null;
            particleTextures = null;
            particleInstances = null;
            batchedAlphaBlendParticles = null;
            batchedAdditiveBlendParticles = null;
        }

        /// <summary>
		/// 提供されたパーティクルインスタンスをワールドにスポーンします（パーティクルの上限に達していない場合）。
        /// </summary>
        public static void SpawnParticle(Particle particle)
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server || activeParticles == MaxParticlesAllowed)
                return;

            particles[nextVacantIndex] = particle;
            particle.ID = nextVacantIndex;
            particle.Type = particleTypes[particle.GetType()];

            if (nextVacantIndex + 1 < particles.Length && particles[nextVacantIndex + 1] == null)
                nextVacantIndex++;
            else
                for (int i = 0; i < particles.Length; i++)
                    if (particles[i] == null)
                        nextVacantIndex = i;

            activeParticles++;
        }

        public static void SpawnParticle(int type, Vector2 position, Vector2 velocity, Vector2 origin = default, float rotation = 0f, float scale = 1f)
        {
            Particle particle = new Particle(); // yes i know constructors exist. yes i'm doing this so you dont have to make constructors over and over.
            particle.Position = position;
            particle.Velocity = velocity;
            particle.Color = Color.White;
            particle.Origin = origin;
            particle.Rotation = rotation;
            particle.Scale = scale;
            particle.Type = type;

            SpawnParticle(particle);
        }

        public static void SpawnParticle(int type, Vector2 position, Vector2 velocity)
        {
            Particle particle = new Particle();
            particle.Position = position;
            particle.Velocity = velocity;
            particle.Color = Color.White;
            particle.Origin = Vector2.Zero;
            particle.Rotation = 0f;
            particle.Scale = 1f;
            particle.Type = type;

            SpawnParticle(particle);
        }

        /// <summary>
		/// 指定されたインデックスのパーティクルを削除します。代わりにParticle.Kill()を使います。
        /// </summary>
        public static void DeleteParticleAtIndex(int index)
        {
            particles[index] = null;
            activeParticles--;
            nextVacantIndex = index;
        }

        /// <summary>
		/// 現在スポーンしているパーティクルをすべてクリアします。
        /// </summary>
        public static void ClearAllParticles()
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i] = null;

            activeParticles = 0;
            nextVacantIndex = 0;
        }

        internal static void UpdateAllParticles()
        {
            foreach (Particle particle in particles)
            {
                if (particle == null)
                    continue;

                particle.TimeActive++;
                particle.Position += particle.Velocity;

                particle.Update();
            }
        }

        internal static void RunRandomSpawnAttempts()
        {
            foreach (Particle particle in particleInstances)
                if (Main.rand.NextFloat() < particle.SpawnChance)
                    particle.OnSpawnAttempt();
        }

        internal static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in particles)
            {
                if (particle == null || particle is ScreenParticle && ModContent.GetInstance<MoreKatanaConfig>().ForegroundParticles == false)
                    continue;

                if (particle.UseAdditiveBlend)
                    batchedAdditiveBlendParticles.Add(particle);
                else
                    batchedAlphaBlendParticles.Add(particle);
            }
            spriteBatch.End();

            if (batchedAlphaBlendParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null, Main.GameViewMatrix.ZoomMatrix);

                foreach (Particle particle in batchedAlphaBlendParticles)
                {
                    if (particle.UseCustomDraw)
                        particle.CustomDraw(spriteBatch);
                    else
                        spriteBatch.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, null, particle.Color, particle.Rotation, particle.Origin, particle.Scale * Main.GameViewMatrix.Zoom, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
            }

            if (batchedAdditiveBlendParticles.Count > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                foreach (Particle particle in batchedAdditiveBlendParticles)
                {
                    if (particle.UseCustomDraw)
                        particle.CustomDraw(spriteBatch);
                    else
                        spriteBatch.Draw(particleTextures[particle.Type], particle.Position - Main.screenPosition, null, particle.Color, particle.Rotation, particle.Origin, particle.Scale * Main.GameViewMatrix.Zoom, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
            }

            batchedAlphaBlendParticles.Clear();
            batchedAdditiveBlendParticles.Clear();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        /// <summary>
		/// 指定されたパーティクルタイプのテクスチャを取得します。
        /// </summary>
        public static Texture2D GetTexture(int type) => particleTextures[type];

        /// <summary>
		/// 指定されたパーティクルの数値型を返します。
        /// </summary>
        public static int ParticleType<T>() => particleTypes[typeof(T)];
    }
}