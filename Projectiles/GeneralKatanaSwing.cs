using MoreKatana.Projectiles.Base;
using Terraria;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles
{
    /// <summary>
    /// 汎用のカタナの振りの発射体
    /// </summary>
    public class GeneralKatanaSwing : CustomSword
    {
        /// <summary> 初期設定 </summary>
        public override void Initialize(Item item, int type)
        {
            // 1振りで同じターゲットに2回ヒットしないようにする
            Projectile.localNPCHitCooldown = -1;

            // 振りの弧の描きかたをランダムに設定
            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            // アイテムテクスチャからサイズと色を取得する
            GetTextureValues();

            // ちょっとスクリーンシェイク
            Owner.ScreenShake(2, 3);
        }

        /// <summary> 全ての振りの設定 </summary>
        public SwingData Down => new SwingData(SwordItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f)); // 1振り目
        public SwingData Up => new SwingData(SwordItem.useAnimation, Main.rand.NextFloat(0.7f, 0.8f), backspin: true); // 2振り目
        public override SwingData GetSwingData(int type) => SwingData.SwingRegister(type, Down, Up);

        /// <summary> 
        /// 振りのアニメーションの設定 
        /// <see cref="GeneralSwingAnimation(float)"/> は基本的な振りと振りの減衰のみのアニメーション。困ったときはこれを使えばよい
        /// </summary>
        public override float GetProgress(int type) => GeneralSwingAnimation(Progress);

        /// <summary> 追加AI </summary>
        public override void AdditionalAI(Item item, int type, bool delay) => Owner.SetDummyItemTime(2); // プレイヤーのアイテム使用時間を延長する
    }
}