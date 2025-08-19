using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace MoreKatana
{
    /// <summary>
    /// ICameraModifierを使用し、カメラを指定した位置に移動させます
    /// </summary>
    public class CameraModifier : ICameraModifier
    {
        private int framesToLast;
        private int framesElapsed;
        private Vector2 targetPosition;

        private readonly float InAmount;
        private readonly float OutAmount;

        public bool AutoReturn;
        public bool IsFinished;

        public string UniqueIdentity { get; private set; }
        public bool Finished { get; private set; }

        /// <summary>
        /// カメラを指定した位置に移動させます
        /// </summary>
        /// <param name="position"> カメラの新しい位置 </param>
        /// <param name="frames"> カメラの一連の動きにかかるフレーム </param>
        /// <param name="inAmount"> 新しい位置に向かうまでにかかるフレームの割合 </param>
        /// <param name="outAmount"> 元の位置に戻るまでにかかるフレームの割合 </param>
        /// <param name="autoReturn"> 自動で元の位置に戻るかどうか </param>
        /// <param name="uniqueIdentity"> 同じアイデンティティを持つ他のモディファイアを同時に機能させないためのキー </param>
        public CameraModifier(Vector2 position, int frames, float inAmount, float outAmount, bool autoReturn = true, string uniqueIdentity = null)
        {
            targetPosition = position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            framesToLast = frames;
            InAmount = inAmount;
            OutAmount = outAmount;
            AutoReturn = autoReturn;
            UniqueIdentity = uniqueIdentity;
        }

        public void Update(ref CameraInfo cameraInfo)
        {
            float progress = Utils.GetLerpValue(0, framesToLast, framesElapsed);
            float firstStep = InAmount;
            float finalStep = 1 - OutAmount;
            float lerpAmount;
            if (progress < firstStep)
                lerpAmount = Utils.Remap(progress, 0, firstStep, 0, 1);
            else if (progress > finalStep)
                lerpAmount = Utils.Remap(progress, finalStep, 1f, 1, 0);
            else
                lerpAmount = 1;

            cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, targetPosition, lerpAmount);

            if (!Main.gameInactive && !Main.gamePaused && (lerpAmount != 1f || AutoReturn))
                framesElapsed++;

            if (framesElapsed >= framesToLast || IsFinished)
                Finished = true;

            Player player = Main.LocalPlayer;
            if (player.dead || player.ghost || !player.active)
                Finished = true;
        }
    }
}