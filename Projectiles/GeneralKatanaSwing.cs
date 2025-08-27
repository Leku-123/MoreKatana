using MoreKatana.Projectiles.Base;
using Terraria;
using static MoreKatana.MoreKatanaUtil;

namespace MoreKatana.Projectiles
{
    /// <summary>
    /// 基本的な剣の振り
    /// </summary>
    public class GeneralKatanaSwing : CustomSword
    {
        public override void Initialization(Item item, int type)
        {
            Projectile.localNPCHitCooldown = -1; // 1振りで同じターゲットに2回ヒットしないようにする
            GetTextureValues(this, item);
        }

        public override bool SwingPattern(Item item, int type)
        {
            // 振りの描く弧にランダム性を持たせる
            float x = Main.rand.NextFloat(1f, 1.3f);
            float y = Main.rand.NextFloat(0.7f, 0.9f);
            SwingEllipse = new(x, y);

            // 振る範囲にランダム性を持たせる
            float swingRange = Main.rand.NextFloat(0.7f, 0.8f);
            SwingStats(item.useAnimation, swingRange, (0.9f - swingRange) / 2f, type % 2 != 0);

            return base.SwingPattern(item, type);
        }

        public CurveSegment execute = new CurveSegment(SineOutEasing, 0f, 0f, 0.95f); // 振りのアニメーション
        public CurveSegment unwind = new CurveSegment(LinearEasing, 0.5f, 0.95f, 0.05f); // 振りの減衰のアニメーション
        public override float GetProgress(int type) => PiecewiseAnimation(Progress, execute, unwind);

        public override void AdditionalAI(Item item, int type, bool delay)
        {
            // プレイヤーのアイテム使用時間を延長する
            Owner.SetDummyItemTime(2);

            // スクリーンシェイク
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Owner.ScreenShake(2, 2);
            }
        }
    }
}