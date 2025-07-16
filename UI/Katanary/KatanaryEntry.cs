using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreKatana.Items.Katana;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace MoreKatana.UI.Katanary
{
    public class KatanaryEntry : UIPanel
    {
        public KatanaryEntryItem entryItem;

        // 新しく入手した刀を強調表示するフラグ（未実装）
        //public bool katanaShiny;

        public KatanaryEntry(KatanaItem katana, bool isAPrettyPortrait)
        {
            entryItem = KatanaryEntryItem.Katana(katana.Item.type);
            Height.Set(72f, 0f);
            Width.Set(72f, 0f);
            SetPadding(0f);
            UIElement uIElement = new UIElement
            {
                Width = new StyleDimension(-4f, 1f),
                Height = new StyleDimension(-4f, 1f),
                IgnoresMouseInteraction = true,
                OverflowHidden = true,
                HAlign = 0.5f,
                VAlign = 0.5f
            };
            uIElement.SetPadding(0f);
            uIElement.Append(new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Back"))
            {
                VAlign = 0.5f,
                HAlign = 0.5f
            });
            if (isAPrettyPortrait)
            {
                Asset<Texture2D> asset = TryGettingBackgroundImageProvider(entryItem);
                if (asset != null)
                {
                    uIElement.Append(new UIImage(asset)
                    {
                        HAlign = 0.5f,
                        VAlign = 0.5f
                    });
                }
            }
            UIKatanaryEntryIcon uIBestiaryEntryIcon = new UIKatanaryEntryIcon(entryItem, isAPrettyPortrait);
            uIElement.Append(uIBestiaryEntryIcon);
            base.Append(uIElement);
            _icon = uIBestiaryEntryIcon;
            int? num = TryGettingDisplayIndex(entryItem);
            if (num != null)
            {
                UIText element = new UIText(num.Value.ToString(), 0.9f, false)
                {
                    Top = new StyleDimension(10f, 0f),
                    Left = new StyleDimension(10f, 0f),
                    IgnoresMouseInteraction = true
                };
                base.Append(element);
            }
            _bordersGlow = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Selection"))
            {
                VAlign = 0.5f,
                HAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
            _bordersOverlay = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Overlay"))
            {
                VAlign = 0.5f,
                HAlign = 0.5f,
                IgnoresMouseInteraction = true,
                Color = Color.White * 0.6f
            };
            base.Append(_bordersOverlay);
            UIImage uIImage = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Front"))
            {
                VAlign = 0.5f,
                HAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
            base.Append(uIImage);
            _borders = uIImage;
            if (isAPrettyPortrait)
            {
                base.RemoveChild(_bordersOverlay);
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
            RemoveChild(_borders);
            RemoveChild(_bordersGlow);
            RemoveChild(_bordersOverlay);
            Append(_borders);
            Append(_bordersGlow);
            _icon.ForceHover = true;
        }

        private Asset<Texture2D> TryGettingBackgroundImageProvider(KatanaryEntryItem entry)
        {
            IEnumerable<IBestiaryBackgroundImagePathAndColorProvider> enumerable = from x in entry.Info
                                                                                   where x is IBestiaryBackgroundImagePathAndColorProvider
                                                                                   select x as IBestiaryBackgroundImagePathAndColorProvider;
            IEnumerable<IPreferenceProviderElement> preferences = entry.Info.OfType<IPreferenceProviderElement>();
            foreach (IBestiaryBackgroundImagePathAndColorProvider bestiaryBackgroundImagePathAndColorProvider in enumerable.Where((IBestiaryBackgroundImagePathAndColorProvider provider) => preferences.Any((IPreferenceProviderElement preference) => preference.Matches(provider))))
            {
                Asset<Texture2D> asset = bestiaryBackgroundImagePathAndColorProvider.GetBackgroundImage();
                if (asset != null)
                {
                    return asset;
                }
            }
            foreach (IBestiaryBackgroundImagePathAndColorProvider bestiaryBackgroundImagePathAndColorProvider2 in enumerable)
            {
                Asset<Texture2D> asset = bestiaryBackgroundImagePathAndColorProvider2.GetBackgroundImage();
                if (asset != null)
                {
                    return asset;
                }
            }
            return null;
        }

        private int? TryGettingDisplayIndex(KatanaryEntryItem entry)
        {
            int? result = null;
            IBestiaryInfoElement bestiaryInfoElement = entry.Info.FirstOrDefault((IBestiaryInfoElement x) => x is IBestiaryEntryDisplayIndex);
            if (bestiaryInfoElement != null)
            {
                result = new int?((bestiaryInfoElement as IBestiaryEntryDisplayIndex).BestiaryDisplayIndex);
            }
            return result;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (base.IsMouseHovering)
            {
                Main.instance.MouseText(_icon.GetHoverText(), 0, 0, -1, -1, -1, -1, 0);
            }
        }

        private UIImage _bordersGlow;

        private UIImage _bordersOverlay;

        private UIImage _borders;
        private UIKatanaryEntryIcon _icon;
    }
}
