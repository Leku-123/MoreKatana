using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MoreKatana.Particles
{
    /// <summary>
    /// 視差効果やスクリーンのラッピングで描画できるパーティクル。ゲーム内のズームと自動フェードインアウトを調整する
    /// </summary>
    public abstract class ScreenParticle : Particle
    {
        public Vector2 OriginalScreenPosition;
        public float ParallaxStrength;
        protected float activeOpacity;

        /// <summary>
        /// パーティクルが自然にスポーンできるかどうかを決定する条件。ScreenParticle.ScreenSpawnChanceと組み合わせて使用する
        /// trueの場合は完全な不透明度になるまでフェードインする。そうしないと完全に透明になるまでフェードアウトしてしまう
        /// その場合はParticle.Kill()を使用する
        /// </summary>
        public virtual bool ActiveCondition => true;

        /// <summary>
        /// Particle.SpawnChanceの代わりに使用する
        /// ScreenParticle.ActiveCondition がfalseの場合、このプロパティの値に関係なくパーティクルは自然にスポーンしない
        /// </summary>
        public virtual float ScreenSpawnChance => 0f;

        /// <summary>
        /// Particle.UseCustomDrawの代わりに使用する
        /// </summary>
        public virtual bool UseCustomScreenDraw => false;

        public sealed override bool UseCustomDraw => true;

        public sealed override float SpawnChance
        {
            get
            {
                if (!ActiveCondition)
                    return 0f;

                return ScreenSpawnChance;
            }
        }

        /// <summary>
        /// ScreenParticlesのデフォルトのスプライトバッチ描画で使用される。カスタム描画が必要な場合にこれを使用する
        /// </summary>
        /// <returns>パーティクルを描画する画面上の位置。視差効果、画面の折り返し、ゲームのズームに合わせた調整が可能</returns>
        public Vector2 GetDrawPosition()
        {
            // 現在の画面位置とスポーンしたときの元の画面位置との差、および視差効果の強さに基づいて描画位置を修正する
            Vector2 drawPosition = Position - Vector2.Lerp(Main.screenPosition, Main.screenPosition - 2 * (OriginalScreenPosition - Main.screenPosition), ParallaxStrength);

            // 視差効果によってすべてのパーティクルが簡単に見えなくならないようにパーティクルが常に画面上にあるように位置を修正する
            Vector2 ScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            Vector2 UiScreenSize = ScreenSize * Main.UIScale;

            while (drawPosition.X < 0)
                drawPosition.X += UiScreenSize.X;

            while (drawPosition.Y < 0)
                drawPosition.Y += UiScreenSize.Y;

            drawPosition = new Vector2(drawPosition.X % UiScreenSize.X, drawPosition.Y % UiScreenSize.Y) * Main.GameViewMatrix.Zoom;

            return drawPosition - 3 * (ScreenSize * Main.GameViewMatrix.Zoom - ScreenSize) / 4;
        }

        public sealed override void Update()
        {
            UpdateOnScreen();

            if (ActiveCondition)
                activeOpacity = Math.Min(activeOpacity + 0.075f, 1);
            else
                activeOpacity = Math.Max(activeOpacity - 0.075f, 0);
        }

        public sealed override void CustomDraw(SpriteBatch spriteBatch)
        {
            if (UseCustomScreenDraw)
                CustomScreenDraw(spriteBatch);
            else
                spriteBatch.Draw(Texture, GetDrawPosition(), null, Color * activeOpacity, Rotation, Origin, Scale * Main.GameViewMatrix.Zoom, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Particle.Updateの代わりに使用する
        /// </summary>
        public virtual void UpdateOnScreen() { }

        /// <summary>
        /// Particle.CustomDrawの代わりに使用する
        /// </summary>
        public virtual void CustomScreenDraw(SpriteBatch spriteBatch) { }
    }
}