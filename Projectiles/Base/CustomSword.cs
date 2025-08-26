using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.PrimTrails;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MoreKatana.Projectiles.Base
{
    /// <summary>
    /// progressを再調節するとき、<see cref="EaseFunction"/>で使えないクラスがあります
    /// ワンチャン<see cref="EaseFunction"/>が悪い
    /// </summary>
    public abstract class CustomSword : ModProjectile
    {
        #region -------- Variables --------
        /// <summary>
        /// 現在の剣の振りのタイプ
        /// </summary>
        private int SwingType
        {
            get => (int)Projectile.ai[0];
            set
            {
                Projectile.ai[0] = value;

                // 剣の振りのタイプが切り替わったらタイマーをリセットする
                Timer = 0;
                DelayTimer = 0;
            }
        }

        /// <summary>
        /// タイマー
        /// </summary>
        public ref float Timer => ref Projectile.ai[1];      // 剣の振りの進行状況を記録するタイマー。ディレイは除く
        public ref float DelayTimer => ref Projectile.ai[2]; // 剣の振りのディレイのタイマー

        /// <summary>
        /// 剣の振りの動きに関する変数
        /// <see cref="SwingStats(float, float, float?, bool)"/>でまとめて設定できる
        /// </summary>
        protected float SwingTime;     // 剣の振る速度
        protected float SwingRange;    // 剣の振る範囲
        protected float ModifiedAngle; // 剣の振りの開始角度の調整
        protected bool Backspin;       // 剣の振りと向きを逆方向にするかどうか
        protected int BackspinDirection => (!Backspin).ToDirectionInt();

        /// <summary>
        /// 剣のサイズ
        /// <see cref="SwordSize(int, int)"/>で設定できる
        /// </summary>
        private int SwordWidth, SwordHeight;
        protected int SwordLength => (int)((SwordWidth / 2f + SwordHeight / 2f) / 2f / Math.Sin(Math.PI * 45 / 180) * Projectile.scale);

        /// <summary>
        /// 剣の振りの描く弧の比率
        /// <see cref="GetEllipse(float, float)"/>で設定できる
        /// </summary>
        private float X, Y;

        /// <summary>
        /// 剣の振りのAIに関する変数
        /// </summary>
        private Vector2 swordPos;    // 剣の位置
        private float startRotation; // 剣の振りの開始角度
        protected float progress;    // 剣の振りの進行状況

        protected bool timerStop;    // 剣の振りのタイマーを止めるかどうか
        protected bool invisible;    // 剣を描画を無くすかどうか
        protected bool primsCreated; // トレイルを描画したかどうか

        /// <summary>
        /// 雑多な変数
        /// </summary>
        protected bool ContinuousSwing; // 全ての振りを連続的に行うかどうか
        protected bool FixedDirection;  // 全ての振りの方向を固定するかどうか
        protected bool NoSpeedBonus;    // 速度ボーナスを無くすかどうか
        protected Color TrailColor;     // トレイルの色

        protected Player Owner => Main.player[Projectile.owner];
        protected Item SwordItem => Owner.ActiveItem();
        private CustomSwordPrimTrail trail;
        #endregion

        #region -------- Helper Methods --------
        /// <summary>
        /// 剣のサイズ設定
        /// </summary>
        /// <param name="width"> 剣の横幅 </param>
        /// <param name="height"> 剣の縦幅 </param>
        protected void SwordSize(int width, int height)
        {
            SwordWidth = width;
            SwordHeight = height;
        }
        protected void SwordSize(int size) => SwordSize(size, size);

        /// <summary>
        /// 剣の振りの描く弧の比率の設定
        /// 1f, 1fで円形、1f, 0.5fで楕円形になる
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void GetEllipse(float x, float y) { X = x; Y = y; }

        /// <summary>
        /// 剣の振りの動きの設定
        /// </summary>
        /// <param name="time"> 剣の振る速度 </param>
        /// <param name="range"> 剣の振る範囲 </param>
        /// <param name="angle"> 剣の初期位置の調整 </param>
        /// <param name="backspin"> 剣の振りと向きを逆方向にするかどうか </param>
        protected void SwingStats(float time, float range, float? angle = null, bool backspin = false)
        {
            SwingTime = time;
            SwingRange = range;
            ModifiedAngle = angle == null ? (1f - range) / 2f : (float)angle;
            Backspin = backspin;
        }

        /// <summary>
        /// 剣のテクスチャからサイズと色を取得
        /// </summary>
        /// <param name="customSword"></param>
        /// <param name="item"></param>
        public static void GetTextureValues(CustomSword customSword, Item item)
        {
            Texture2D texture = TextureAssets.Item[item.type].Value;

            // サイズ
            int frame = Main.itemAnimations[item.type] == null ? 1 : Main.itemAnimations[item.type].FrameCount;
            customSword.SwordSize(texture.Width, texture.Height / frame);

            // 色
            Color[] colors = MoreKatanaUtil.GetColors(texture);
            int a = 0;
            Vector4 vector4 = new Vector4(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != new Color(0, 0, 0, 0))
                {
                    a++;
                    vector4 += colors[i].ToVector4();
                }
            }
            vector4 /= a * 2;
            customSword.TrailColor = new Color(vector4.X, vector4.Y, vector4.Z, 0);
        }
        #endregion

        public override string Texture => MoreKatana.EmptyTexture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true; // ブロックやハーフブロックの昇降時のズレを防ぐ
            SafeSetStaticDefaults();
        }

        public virtual void SafeSetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults()
        {

        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(SwordWidth);
            writer.Write7BitEncodedInt(SwordHeight);
            writer.Write((sbyte)Projectile.spriteDirection);
            writer.WriteVector2(swordPos);
            writer.Write(SwingTime);
            writer.Write(SwingRange);
            writer.Write(ModifiedAngle);
            writer.WriteFlags(Backspin);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwordWidth = reader.Read7BitEncodedInt();
            SwordHeight = reader.Read7BitEncodedInt();
            Projectile.spriteDirection = reader.ReadSByte();
            swordPos = reader.ReadVector2();
            SwingTime = reader.ReadSingle();
            SwingRange = reader.ReadSingle();
            ModifiedAngle = reader.ReadSingle();
            Backspin = reader.ReadBoolean();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float dummy = 0f;
            Vector2 offset = new Vector2(SwordWidth, SwordHeight) * Projectile.scale * Projectile.rotation.ToRotationVector2();
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public override void CutTiles()
        {
            Vector2 offset = new Vector2(SwordWidth, SwordHeight) * Projectile.scale * Projectile.rotation.ToRotationVector2();
            Vector2 top = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            Utils.PlotTileLine(top, end, Projectile.scale, DelegateMethods.CutTiles);
        }

        public override void AI()
        {
            // 剣が振れるかのチェック
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }

            // 初期設定
            if (Timer == 0f)
            {
                SwingPattern(SwordItem, SwingType);

                Projectile.alpha = 0;

                if (!FixedDirection)
                    Owner.direction = Main.MouseWorld.X < Owner.Center.X ? -1 : 1;

                startRotation = (-Projectile.velocity).ToRotation(); // Projectile.velocityの反対方向
                DelayTimer *= Projectile.MaxUpdates;

                Initialization(SwordItem, SwingType);

                Projectile.netUpdate = true;
            }

            // タイマーを増加 (手動で止めない限り)
            if (!timerStop)
                Timer++;

            SetSwordPosition(swordPos);
            SwingAnimation();
        }

        /// <summary>
        /// 剣の位置
        /// </summary>
        public virtual void SetSwordPosition(Vector2 v)
        {
            // 発射体の位置と向き
            Projectile.Center = Owner.MountedCenter + (v * Projectile.scale);
            Projectile.spriteDirection = Owner.direction;

            // 発射体の回転を調節する。Backspinも考慮する
            Projectile.rotation = (Projectile.Center - Owner.MountedCenter).ToRotation()
                + (MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection)
                * BackspinDirection;

            /*
            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - Owner.MountedCenter).ToRotation() + MathHelper.ToRadians(45f) - (!Backspin ? 0f : (float)Math.PI / 2);
            else
                Projectile.rotation = (Projectile.Center - Owner.MountedCenter).ToRotation() + MathHelper.ToRadians(135f) + (!Backspin ? 0f : (float)Math.PI / 2);
            */

            // プレイヤーの保持する発射体のIDを更新する
            Owner.heldProj = Projectile.whoAmI;

            // 腕の回転の設定をする
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Owner.MountedCenter - Projectile.Center).ToRotation() + (float)Math.PI / 2f);
        }

        /// <summary>
        /// 剣の動き
        /// </summary>
        private void SwingAnimation()
        {
            // Timerを剣の振る速度で除算してprogressを計算する
            // 速度ボーナスも適用する
            progress = Timer / (SwingTime * Projectile.MaxUpdates / (!NoSpeedBonus ? Owner.GetTotalAttackSpeed(Projectile.DamageType) : 1));
            progress = MathHelper.Clamp(GetProgress(SwingType), 0f, 1f);

            float swingRange = (float)Math.PI * 2f * Projectile.spriteDirection * SwingRange; // 振る回転角
            float modifiedAngle = (float)Math.PI * 2f * Projectile.spriteDirection * ModifiedAngle; // 振る初期位置
            //int swingDirection = Backspin ? -1 : 1; // 振る向き
            bool execute = progress != 1f && !timerStop;

            if (execute)
            {
                Projectile.friendly = true;

                // 剣の動き
                // SwingStatus()から取得した変数、GetEllipse()で取得したXとYをもとにswordPosを求める
                float x = SwordWidth / 2 * X * MathF.Cos((progress * swingRange + modifiedAngle) * BackspinDirection);
                float y = SwordHeight / 2 * Y * MathF.Sin((progress * swingRange + modifiedAngle) * BackspinDirection);
                Vector2 ellipse = new Vector2(x, y);
                swordPos = ellipse.RotatedBy(startRotation, default) * 1.75f;

                // タイル衝突の処理
                Vector2 collisionBase = Projectile.position;
                const int checkpoint = 5;
                for (int j = 0; j < checkpoint; j++)
                {
                    collisionBase += (Projectile.Center - Owner.MountedCenter).ToRotation().ToRotationVector2() * SwordLength / checkpoint;
                    bool validTile = Collision.SolidTiles(collisionBase, 2, 2, true);
                    if (validTile)
                        SafeTileCollide(SwordItem, SwingType, collisionBase);
                }

                // ビジュアル効果
                if (Main.rand.NextBool(4))
                {
                    Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center, new Vector2(SwordWidth * Projectile.scale, SwordHeight * Projectile.scale));

                    // アイテムから効果を取得する
                    ItemLoader.MeleeEffects(SwordItem, Owner, rectangle);
                }
            }
            else
            {
                if (DelayTimer > 0f)
                    DelayTimer--;

                if (DelayTimer <= 0f)
                {
                    Projectile.friendly = false;
                    Projectile.alpha = 255;

                    // 連続する振りの場合そのまま次の振りのパターンにする
                    // それ以外は消滅
                    if (ContinuousSwing)
                    {
                        if (!SwingPattern(SwordItem, SwingType))
                        {
                            Projectile.Kill();
                            return;
                        }

                        if (!FixedDirection) // 全ての振りの方向を固定しない場合
                        {
                            // Projectile.velocityをマウスの方向にする
                            if (Main.myPlayer == Projectile.owner)
                                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                        }

                        SwingType++;
                    }
                    else
                    {
                        Projectile.Kill();
                        return;
                    }

                    primsCreated = false;

                    Projectile.netUpdate = true;
                }
            }

            DrawTrail(BackspinDirection);
            AdditionalAI(SwordItem, SwingType, !execute);
        }

        /// <summary>
        /// トレイル
        /// </summary>
        /// <param name="dir"></param>
        public virtual void DrawTrail(int dir)
        {
            if (Timer != 0f)
            {
                // トレイルを描画する
                if (!primsCreated)
                {
                    primsCreated = true;
                    trail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                    MoreKatana.primitives.CreateTrail(trail);
                }

                if (Main.netMode != NetmodeID.Server)
                {
                    // トレイルの設定
                    trail.Direction = Owner.direction * -dir;
                    trail.PrimCenter = Owner.MountedCenter;
                    trail.Points.Add(Projectile.Center - Owner.MountedCenter);

                    // 剣を描画しない場合トレイルを消す
                    if (invisible || progress >= 0.98f)
                        trail?.OnDestroy();
                }
            }
        }

        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        public virtual void Initialization(Item item, int type)
        {

        }

        /// <summary>
        /// 剣の振りのパターン
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool SwingPattern(Item item, int type) => false;

        /// <summary>
        /// <see cref="progress"/>の修正
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual float GetProgress(int type) => progress;

        /// <summary>
        /// 追加で行うAI
        /// </summary>
        /// <param name="type"></param>
        public virtual void AdditionalAI(Item item, int type, bool delay)
        {

        }

        /// <summary>
        /// タイルに衝突したときの処理
        /// <see cref="OnTileCollide(Vector2)"/>と別物
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        public virtual void SafeTileCollide(Item item, int type, Vector2 collisionPoint)
        {

        }

        public override bool? CanDamage() => !invisible; // 描画しない場合はダメージを与えないようにする

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // ノックバックをプレイヤーから遠ざける
            modifiers.HitDirectionOverride = target.position.X > Owner.Center.X ? 1 : -1;

            // アイテムから効果を取得する
            ItemLoader.ModifyHitNPC(SwordItem, Owner, target, ref modifiers);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.netUpdate = true;

            Owner.StatusToNPC(SwordItem.type, target.whoAmI);
            if (target.life > 5)
                Owner.OnHit(target.Center.X, target.Center.Y, target);

            // アイテムから効果を取得する
            ItemLoader.OnHitNPC(SwordItem, Owner, target, hit, damageDone);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (invisible)
                return false;

            Texture2D texture = TextureAssets.Item[SwordItem.type].Value;

            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? rectangle = new Rectangle?(Main.itemAnimations[SwordItem.type] == null ? texture.Frame(1, 1, 0, 0, 0, 0) : Main.itemAnimations[SwordItem.type].GetFrame(texture, -1));

            float frame = Main.itemAnimations[SwordItem.type] == null ? 1 : Main.itemAnimations[SwordItem.type].FrameCount;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frame / 2);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = Backspin ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            return false;
        }

        public void DrawBasicSword(Texture2D texture, Vector2 position, Color color)
        {
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = Backspin ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);
        }
    }
}