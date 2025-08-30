using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Projectiles.PrimTrails;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreKatana.Projectiles.Base
{
    public abstract class CustomSword : ModProjectile
    {
        #region -------- Variables --------
        /// <summary> 現在の剣の振りのタイプ </summary>
        public int SwingType
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

        /// <summary> 剣の振りの進行状況を記録するタイマー。ディレイは除く </summary>
        public ref float Timer => ref Projectile.ai[1];

        /// <summary> 剣の振りのディレイのタイマー </summary>
        public ref float DelayTimer => ref Projectile.ai[2];

        #region -------- SwordSize --------
        /// <summary> 剣のサイズ </summary>
        private int SwordWidth, SwordHeight;

        /// <summary> 剣の長さ </summary>
        protected int SwordLength => (int)((SwordWidth / 2f + SwordHeight / 2f) / 2f / Math.Sin(Math.PI * 45 / 180) * Projectile.scale);
        #endregion

        #region -------- Trail --------
        /// <summary> トレイルを描画したかどうか </summary>
        public bool PrimsCreated;

        /// <summary> trueなら、トレイルを消滅させる </summary>
        public bool KillPrims;

        /// <summary> トレイルの色 </summary>
        public Color TrailColor;
        #endregion

        #region -------- SwingStats --------
        /// <summary> 剣の振る速度 </summary>
        protected float SwingTime;

        /// <summary> 剣の振る範囲 </summary>
        protected float SwingRange;

        /// <summary> 剣の振りの開始角度の調整 </summary>
        protected float ModifiedAngle;

        /// <summary> 剣の振りの向き </summary>
        protected int SwingDirection;

        /// <summary> ディレイの長さ </summary>
        protected float SwingDelay;
        #endregion

        #region -------- Private --------
        /// <summary> 剣の位置 </summary>
        private Vector2 swordPos;

        /// <summary> 剣の振りの開始角度 </summary>
        private float startRotation;

        /// <summary> ターゲットにヒットした際のタイマー </summary>
        private int hitTimer = -1;
        #endregion

        /// <summary> 剣の振りのAIの進行状況 </summary>
        protected float Progress;

        /// <summary> ディレイの進行状況 </summary>
        protected float DelayProgress;

        /// <summary> 
        /// ヒットした際のちょっとした"溜め"の量 
        /// 10まで位が演出としての限度
        /// </summary>
        protected int ImpactCharge;

        /// <summary> ImpactChargeを使用した場合の、"溜め"が無くなるタイミング </summary>
        protected bool OnImpact => ImpactCharge != 0 && hitTimer == 1;

        /// <summary>
        /// 剣の振りの描く弧の比率の設定
        /// (x: 1f, y: 1f)で円形、(x: 1f, y: 0.5f)で楕円形になる
        /// </summary>
        protected Vector2 SwingEllipse = Vector2.One;

        /// <summary> trueなら、剣の振りを強制的に終了してディレイに移行する </summary>
        protected bool SwingStop;

        /// <summary> trueなら、全ての振りのパターンを連続的に行う </summary>
        protected bool ContinuousSwing;

        /// <summary> trueなら、全ての振りのパターンの方向が固定される </summary>
        protected bool FixedDirection;

        /// <summary> trueなら、速度ボーナスを無くす </summary>
        protected bool NoSpeedBonus;

        /// <summary> 速度ボーナス </summary>
        protected float ModifiedAttackSpeed => !NoSpeedBonus ? Owner.GetTotalAttackSpeed(Projectile.DamageType) : 1f;

        protected Player Owner => Main.player[Projectile.owner];
        protected Item SwordItem => Owner.ActiveItem();
        protected CustomSwordPrimTrail SwordTrail;
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
        /// 剣の振りの動きの設定
        /// </summary>
        /// <param name="time"> 剣の振る速度 </param>
        /// <param name="range"> 剣の振る範囲 </param>
        /// <param name="angle"> 剣の初期位置の調整 </param>
        /// <param name="backspin"> 剣の振りと向きを逆方向にするかどうか </param>
        protected void SwingStats(float time, float range, float? angle = null, bool backspin = false, float delay = 1f)
        {
            SwingTime = time;
            SwingRange = range;
            ModifiedAngle = angle == null ? (1f - range) / 2f : (float)angle;
            SwingDirection = (!backspin).ToDirectionInt();
            SwingDelay = delay;
        }

        /// <summary>
        /// アイテムのテクスチャからサイズと色を取得
        /// </summary>
        /// <param name="customSword"></param>
        /// <param name="item"></param>
        protected void GetTextureValues()
        {
            Texture2D texture = TextureAssets.Item[SwordItem.type].Value;

            // サイズ
            int frame = Main.itemAnimations[SwordItem.type] == null ? 1 : Main.itemAnimations[SwordItem.type].FrameCount;
            SwordSize(texture.Width, texture.Height / frame);

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
            TrailColor = new Color(vector4.X, vector4.Y, vector4.Z, 0);
        }
        #endregion

        public override string Texture => MoreKatana.EmptyTexture;

        public sealed override void SetStaticDefaults()
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
            Projectile.timeLeft = 9999;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.noEnchantmentVisuals = true;
            Projectile.MKProjectile().SourceIsItemUse = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(SwordWidth);
            writer.Write7BitEncodedInt(SwordHeight);
            writer.WriteVector2(swordPos);
            writer.Write(startRotation);
            writer.WriteVector2(SwingEllipse);
            writer.Write(SwingTime);
            writer.Write(SwingRange);
            writer.Write(ModifiedAngle);
            writer.Write((sbyte)SwingDirection);
            writer.Write(TrailColor.R);
            writer.Write(TrailColor.G);
            writer.Write(TrailColor.B);
            writer.Write(TrailColor.A);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwordWidth = reader.Read7BitEncodedInt();
            SwordHeight = reader.Read7BitEncodedInt();
            swordPos = reader.ReadVector2();
            startRotation = reader.ReadSingle();
            SwingEllipse = reader.ReadVector2();
            SwingTime = reader.ReadSingle();
            SwingRange = reader.ReadSingle();
            ModifiedAngle = reader.ReadSingle();
            SwingDirection = reader.ReadSByte();
            TrailColor.R = (byte)reader.Read7BitEncodedInt();
            TrailColor.G = (byte)reader.Read7BitEncodedInt();
            TrailColor.B = (byte)reader.Read7BitEncodedInt();
            TrailColor.A = (byte)reader.Read7BitEncodedInt();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float _ = float.NaN;
            Vector2 offset = new Vector2(SwordWidth, SwordHeight) * Projectile.scale * Projectile.rotation.ToRotationVector2();
            Vector2 tip = Projectile.Center + offset;
            Vector2 end = Projectile.Center - offset;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), tip, end, Projectile.scale, ref _);
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
                Projectile.velocity.Normalize();

                Projectile.alpha = 0;

                // 振りのパターンを取得
                SwingPattern(SwordItem, SwingType);

                // 剣の振りの開始角度はProjectile.velocityの反対方向
                // 基本的にはプレイヤーの背中側になる
                startRotation = (-Projectile.velocity).ToRotation();

                // 初期設定
                Initialization(SwordItem, SwingType);

                Projectile.localNPCHitCooldown = (int)(Projectile.localNPCHitCooldown / ModifiedAttackSpeed * Projectile.MaxUpdates);

                Projectile.netUpdate = true;
            }

            hitTimer--;

            // タイマーを増加
            if (!SwingStop && hitTimer <= 0)
                Timer++;

            SetSwordPosition(swordPos);
            SwingAnimation();
        }

        /// <summary>
        /// 剣の位置
        /// オーバーライドすれば、剣をプレイヤー以外の位置を中心に動かすこともできる
        /// </summary>
        public virtual void SetSwordPosition(Vector2 v)
        {
            // 振りのパターンの方向を固定しない場合、プレイヤーが発射体の方向を向く
            if (!FixedDirection)
                Owner.ChangeDir(Math.Sign(Projectile.velocity.X));

            // 発射体の位置と向き
            Projectile.Center = Owner.MountedCenter + (v * Projectile.scale);
            Projectile.spriteDirection = Owner.direction;

            // 発射体の回転を調節する
            Projectile.rotation = (Projectile.Center - Owner.MountedCenter).ToRotation()
                + (MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection)
                * SwingDirection;

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
            // 剣の振りのAIの進行度
            Progress = Timer / (SwingTime / ModifiedAttackSpeed * Projectile.MaxUpdates);
            Progress = MathHelper.Clamp(Progress, 0f, 1f);

            float modifiedProgress = GetProgress(SwingType); // 剣の振りの動きの進行度
            float swingRange = (float)Math.PI * 2f * Projectile.spriteDirection * SwingRange; // 剣の振る範囲
            float modifiedAngle = (float)Math.PI * 2f * Projectile.spriteDirection * ModifiedAngle; // 剣の振りの開始角度

            // 剣の動きの処理
            float x = SwordWidth / 2 * SwingEllipse.X * MathF.Cos((modifiedProgress * swingRange + modifiedAngle) * SwingDirection);
            float y = SwordHeight / 2 * SwingEllipse.Y * MathF.Sin((modifiedProgress * swingRange + modifiedAngle) * SwingDirection);
            Vector2 ellipse = new Vector2(x, y);
            swordPos = ellipse.RotatedBy(startRotation, default) * 1.75f;

            bool execute = Progress != 1f && !SwingStop; // 剣の振りが実行できるか
            if (execute)
            {
                Projectile.friendly = true;

                // タイル衝突の処理
                Vector2 collisionBase = Projectile.position;
                const int checkpoint = 5;
                for (int j = 0; j < checkpoint; j++)
                {
                    collisionBase += (Projectile.Center - Owner.MountedCenter).ToRotation().ToRotationVector2() * SwordLength / checkpoint;
                    bool validTile = Collision.SolidTiles(collisionBase, 2, 2, true);
                    if (validTile)
                    {
                        SafeTileCollide(SwordItem, SwingType, collisionBase, modifiedProgress);
                    }
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
                // ディレイの進行度
                // 速度ボーナスも適用する
                DelayProgress = DelayTimer / (SwingDelay / ModifiedAttackSpeed * Projectile.MaxUpdates);
                DelayProgress = MathHelper.Clamp(DelayProgress, 0f, 1f);

                if (DelayProgress == 1f)
                {
                    Projectile.friendly = false;

                    // 次の振りへの見た目のズレを防ぐため透明にする
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

                    PrimsCreated = false;
                    KillPrims = false;

                    Projectile.netUpdate = true;
                }

                DelayTimer++;
            }

            DrawTrail(SwingDirection, SwingType);
            AdditionalAI(SwordItem, SwingType, !execute);
        }

        /// <summary>
        /// トレイルの処理
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        public virtual void DrawTrail(int dir, int type)
        {
            // トレイルをスポーン
            if (!PrimsCreated)
            {
                PrimsCreated = true;
                SwordTrail = new CustomSwordPrimTrail(Projectile, TrailColor, SwordLength, (int)(SwingTime * 1.5f));
                MoreKatana.primitives.CreateTrail(SwordTrail);
            }

            UpdateTrail(SwordTrail);
        }

        /// <summary>
        /// トレイルの情報を更新する
        /// </summary>
        /// <param name="t"></param>
        /// <param name="kill"></param>
        /// <param name="dir"></param>
        /// <param name="center"></param>
        /// <param name="point"></param>
        /// <param name="type"></param>
        /// <param name="width"></param>
        public void UpdateTrail(CustomSwordPrimTrail t, bool? kill = null, int? dir = null, Vector2? center = null, Vector2? point = null, int type = 0, int width = 0)
        {
            if (PrimsCreated && Timer != 0f)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    // トレイルの向き
                    int primDir = (dir == null) ? Owner.direction * -SwingDirection : (int)dir;
                    t.Direction = primDir;

                    // トレイルの位置
                    Vector2 primCenter = (center == null) ? Owner.MountedCenter : (Vector2)center;
                    t.PrimCenter = primCenter;

                    // トレイルの横幅の調節
                    if (width != 0)
                        t.ModifiedWidth = width;

                    // トレイルのテクスチャータイプ
                    t.TextureType = type;

                    // トレイルの描画ポイント
                    Vector2 primPoint = (point == null) ? Projectile.Center - Owner.MountedCenter : (Vector2)point;
                    if (hitTimer <= 0)
                        t.Points.Add(primPoint);

                    // トレイルを消す
                    bool killPrims = (kill == null) ? GetProgress(SwingType) >= 0.95f || KillPrims : (bool)kill;
                    if (killPrims)
                        t?.OnDestroy();
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
        /// 剣の振りの動きの進行度
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual float GetProgress(int type) => Progress;

        /// <summary>
        /// 追加で行うAI
        /// </summary>
        /// <param name="type"></param>
        public virtual void AdditionalAI(Item item, int type, bool delay)
        {

        }

        public sealed override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }

        /// <summary>
        /// タイルに衝突したときの処理
        /// <see cref="OnTileCollide(Vector2)"/>と別物
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <param name="collisionPoint"></param>
        public virtual void SafeTileCollide(Item item, int type, Vector2 collisionPoint, float oldProgress)
        {

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            hitTimer = ImpactCharge * Projectile.MaxUpdates;

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
            Texture2D texture = TextureAssets.Item[SwordItem.type].Value;

            Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle? rectangle = new Rectangle?(Main.itemAnimations[SwordItem.type] == null ? texture.Frame(1, 1, 0, 0, 0, 0) : Main.itemAnimations[SwordItem.type].GetFrame(texture, -1));

            float frame = Main.itemAnimations[SwordItem.type] == null ? 1 : Main.itemAnimations[SwordItem.type].FrameCount;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frame / 2);

            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = SwingDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, rectangle, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);

            return false;
        }

        public void DrawBasicSword(Texture2D texture, Vector2 position, Color color)
        {
            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            SpriteEffects spriteEffects2 = SwingDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, position, rectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects | spriteEffects2, 0);
        }
    }
}