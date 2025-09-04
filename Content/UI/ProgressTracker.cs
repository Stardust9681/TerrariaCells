using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

using TerrariaCells.Common.Systems;
using TerrariaCells.Common.UI.Components.Windows;
using TerrariaCells.Common.Utilities;

using static TerrariaCells.Content.UI.ProgressTracker;

namespace TerrariaCells.Content.UI
{
    public class ProgressTracker : WindowState
    {
        internal override string Name => "ProgressTracker";

        ProgressTabs Tabs;
        DisplayPanel Display;

        public override void OnInitialize()
        {
            Tabs = new ProgressTabs();
            Tabs.Append(new Weapons());
            Tabs.Append(new Abilities());
            Tabs.Append(new Accessories());
            Append(Tabs);

            Display = new DisplayPanel();
            Append(Display);

            Recalculate();
        }

        bool isOpen = false;
        protected override void OnOpened()
        {
            isOpen = true;
        }
        protected override void OnClosed()
        {
            isOpen = false;
        }


        protected override void WindowUpdate(GameTime time)
        {
            Recalculate();
        }
        protected override bool PreDrawSelf(SpriteBatch spriteBatch)
        {
            PostDrawSelf(spriteBatch);
            return false;
        }
        public override void Recalculate()
        {
            base.Recalculate();

            if(!isOpen) return;

            Top.Set(0, 0.2f);
            Left.Set(0, 0.2f);
            Width.Set(0, 0.6f);
            Height.Set(0, 0.6f);
        }
        public override void RecalculateChildren()
        {
            base.RecalculateChildren();

            if(!isOpen) return;

            const int TAB_HEIGHT = 48;

            Tabs?.Width.Set(0, 0.6f);
            Tabs?.Height.Set(TAB_HEIGHT, 0);

            Display?.Width.Set(0, 1);
            Display?.Height.Set(WindowSize.Y - TAB_HEIGHT, 0);
            Display?.Top.Set(TAB_HEIGHT, 0);
        }

        private void SetViewport(UIElement element)
        {
            Display.Assign(element);
        }
        public class ProgressTabs : UIElement
        {
            public class Tab : UIElement
            {
                public bool Selected { get; internal set; }
                private bool hasHoverText = false;
                private LocalizedText? hoverText = null;
                protected virtual bool HasHoverText(ref LocalizedText? text) => false;

                private Asset<Texture2D> Icon;
                public virtual string IconTexture => "";
                public virtual UIElement GetViewport() => default(UIPanel);

                public override void OnInitialize()
                {
                    Icon = ModContent.Request<Texture2D>(IconTexture);
                    hoverText = LocalizedText.Empty;
                    hasHoverText = HasHoverText(ref hoverText);
                }

                protected sealed override void DrawSelf(SpriteBatch spriteBatch)
                {
                    base.DrawSelf(spriteBatch);

                    Rectangle bounds = GetDimensions().ToRectangle();
                    bool isMouseHover = bounds.Contains(Main.MouseScreen.ToPoint());
                    Color drawColor = UIHelper.InventoryColour;
                    if (Selected)
                        drawColor = Main.OurFavoriteColor;
                    else if (isMouseHover)
                        drawColor = Color.AliceBlue;

                    UIHelper.PANEL.Draw(spriteBatch, bounds, drawColor);
                    if (Icon?.IsLoaded == true)
                    {
                        spriteBatch.Draw(Icon.Value, bounds.Center(), null, Color.Black, 0f, Icon.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                    }

                    if (hasHoverText && isMouseHover)
                        Main.instance.MouseText(hoverText?.Value ?? "");
                }
            }
            public int TabCount => Children.Where(x => x is Tab).Count();

            public override void OnInitialize()
            {
                Rectangle bounds = GetDimensions().ToRectangle();
                foreach (Tab tab in from e in Elements where e is Tab select (Tab)e)
                {
                    tab.Width.Set(0, 1f / TabCount);
                    tab.Height.Set(0, 1);
                    tab.OnLeftClick += Tab_OnLeftClick;
                }
            }

            private void Tab_OnLeftClick(UIMouseEvent evt, UIElement elem)
            {
                if (elem is not Tab tab)
                    return;
                if (tab.Selected)
                    return;
                if (Parent is not ProgressTracker tracker)
                    return;

                tracker.SetViewport(tab.GetViewport());
                foreach (Tab tab1 in from e in Elements where e is Tab select (Tab)e)
                {
                    tab1.Selected = false;
                }
                tab.Selected = true;
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            }

            public override void RecalculateChildren()
            {
                int i = 0;
                foreach (Tab tab in from e in Elements where e is Tab select (Tab)e)
                {
                    tab.Width.Set(0, 1f / (float)TabCount);
                    tab.Left.Set(0, (float)(i++) / (float)TabCount);
                }
                base.RecalculateChildren();
            }
        }
    }
    internal class Weapons : ProgressTabs.Tab
    {
        internal class WeaponsViewport : UIElement
        {
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                base.DrawSelf(spriteBatch);
                UIHelper.PANEL.Draw(spriteBatch, GetDimensions().ToRectangle(), Color.DarkSlateBlue);
            }
        }
        public override string IconTexture => "Terraria/Images/Item_" + Terraria.ID.ItemID.SilverBroadsword;
        public override UIElement GetViewport() => new WeaponsViewport();
    }
    internal class Abilities : ProgressTabs.Tab
    {
        internal class AbilitiesViewport : UIElement
        {
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                base.DrawSelf(spriteBatch);
                UIHelper.PANEL.Draw(spriteBatch, GetDimensions().ToRectangle(), Color.DarkSlateBlue);
            }
        }
        public override string IconTexture => "Terraria/Images/Item_" + Terraria.ID.ItemID.MolotovCocktail;
        public override UIElement GetViewport() => new AbilitiesViewport();
    }
    internal class Accessories : ProgressTabs.Tab
    {
        internal class AccessoriesViewport : UIElement
        {
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                base.DrawSelf(spriteBatch);
                UIHelper.PANEL.Draw(spriteBatch, GetDimensions().ToRectangle(), Color.DarkSlateBlue);
            }
        }
        public override string IconTexture => "Terraria/Images/Item_" + Terraria.ID.ItemID.HermesBoots;
        public override UIElement GetViewport() => new AccessoriesViewport();
    }

    public class DisplayPanel : UIElement
    {
        public override void OnInitialize()
        {
            this.SetPadding(10);
        }
        internal void Assign(UIElement child)
        {
            child.Width.Set(0, 1);
            child.Height.Set(0, 1);

            if(Elements.Count != 0)
            {
                UIElement other = Children.First();
                Append(child);
                RemoveChild(other);
            }
            else
            {
                Append(child);
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            UIHelper.PANEL.Draw(spriteBatch, GetDimensions().ToRectangle(), UIHelper.InventoryColour);
        }
    }

    public class UICloser : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (triggersSet.Inventory)
            {
                DeadCellsUISystem.ToggleActive<ProgressTracker>(false);
            }
        }
    }
}
