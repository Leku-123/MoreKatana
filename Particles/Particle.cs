using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoreKatana.Particles
{
    /// <summary>
	/// By SpiritMod
	/// 位置、速度、回転、スケール、透明度を持つパーティクルです。
    /// TL;DR: Better dust.
    /// </summary>
    public class Particle
    {
        public int ID; // you don't have to use this
        public int Type; // you don't have to use this
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Origin;
        public Color Color;
        public float Rotation;
        public float Scale;
        public uint TimeActive;

        public Texture2D Texture => ParticleHandler.GetTexture(Type);

        /// <summary>
        /// これをtrueに設定すると、デフォルトのパーティクル描画が無効になり、代わりにParticle.CustomDraw()が呼び出される。
        /// </summary>
        public virtual bool UseCustomDraw => false;

        /// <summary>
        /// これをtrueに設定すると、パーティクルはアルファブレンドの代わりに加算ブレンドを使用する。
        /// </summary>
        public virtual bool UseAdditiveBlend => false;

        /// <summary>
        /// 任意のティックでこのパーティクルがスポーンする確率。
        /// パーティクルを自然にスポーンさせたくない場合は0fを返す。(自分でスポーンさせたい場合)
        /// Particle.ActiveConditionがfalseを返した場合、このフックは実行されません。
        /// </summary>
        public virtual float SpawnChance => 0f;

        /// <summary>
        /// パーティクルを消去し、ワールドから削除したいときに呼び出す。
        /// </summary>
        public void Kill() => ParticleHandler.DeleteParticleAtIndex(ID);

        /// <summary>
        /// このメソッドでティックごとにパーティクルを更新する。
        /// パーティクルの速度が自動的にパーティクルの位置に追加され、TimeAliveが増加される。
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// パーティクルのカスタム描画を許可する。Particle.UseCustomDrawingがtrueの場合のみ呼び出される。
        /// </summary>
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }

        /// <summary>
        /// パーティクルが自然にスポーンしようとした場合に呼び出される。
        /// Particle.SpawnChanceが0fより大きい値を返し、Particle.ActiveConditionがtrueの場合のみ呼び出される。
        /// パーティクルをランダムにスポーンさせる。
        /// </summary>
        public virtual void OnSpawnAttempt() { }
    }
}