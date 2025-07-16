using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class UIKatanaryEntryButton : UIElement
    {
        // Token: 0x17000724 RID: 1828
        // (get) Token: 0x06003DC4 RID: 15812 RVA: 0x005CC9A0 File Offset: 0x005CABA0
        // (set) Token: 0x06003DC5 RID: 15813 RVA: 0x005CC9A8 File Offset: 0x005CABA8
        public KatanaryEntryItem Entry { get; private set; }

        // Token: 0x06003DC6 RID: 15814 RVA: 0x005CC9B4 File Offset: 0x005CABB4
        public UIKatanaryEntryButton(KatanaryEntryItem entry, bool isAPrettyPortrait)
        {
            Entry = entry;
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
                Asset<Texture2D> asset = TryGettingBackgroundImageProvider(entry);
                if (asset != null)
                {
                    uIElement.Append(new UIImage(asset)
                    {
                        HAlign = 0.5f,
                        VAlign = 0.5f
                    });
                }
            }
            UIKatanaryEntryIcon uIBestiaryEntryIcon = new UIKatanaryEntryIcon(entry, isAPrettyPortrait);
            uIElement.Append(uIBestiaryEntryIcon);
            Append(uIElement);
            _icon = uIBestiaryEntryIcon;
            int? num = TryGettingDisplayIndex(entry);
            if (num != null)
            {
                UIText element = new UIText(num.Value.ToString(), 0.9f, false)
                {
                    Top = new StyleDimension(10f, 0f),
                    Left = new StyleDimension(10f, 0f),
                    IgnoresMouseInteraction = true
                };
                Append(element);
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
            Append(_bordersOverlay);
            UIImage uIImage = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Front"))
            {
                VAlign = 0.5f,
                HAlign = 0.5f,
                IgnoresMouseInteraction = true
            };
            Append(uIImage);
            _borders = uIImage;
            if (isAPrettyPortrait)
            {
                RemoveChild(_bordersOverlay);
            }
            if (!isAPrettyPortrait)
            {
                OnMouseOver += MouseOver;
                OnMouseOut += MouseOut;
            }
        }

        // Token: 0x06003DC7 RID: 15815 RVA: 0x005CCC50 File Offset: 0x005CAE50
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
            if (IsMouseHovering)
            {
                Main.instance.MouseText(_icon.GetHoverText(), 0, 0, -1, -1, -1, -1, 0);
            }
        }

        private void MouseOver(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            RemoveChild(_borders);
            RemoveChild(_bordersGlow);
            RemoveChild(_bordersOverlay);
            Append(_borders);
            Append(_bordersGlow);
            _icon.ForceHover = true;
        }

        private void MouseOut(UIMouseEvent evt, UIElement listeningElement)
        {
            RemoveChild(_borders);
            RemoveChild(_bordersGlow);
            RemoveChild(_bordersOverlay);
            Append(_bordersOverlay);
            Append(_borders);
            _icon.ForceHover = false;
        }


        private UIImage _bordersGlow;

        private UIImage _bordersOverlay;

        private UIImage _borders;

        private UIKatanaryEntryIcon _icon;
    }
}
