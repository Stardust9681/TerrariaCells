using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Humanizer.Bytes;

using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Graphics;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.UI.Components;
using TerrariaCells.Common.UI.Components.Windows;
using TerrariaCells.Common.Utilities;

using static AssGen.Assets;
using static TerrariaCells.Content.UI.ProgressTracker;

using FixedUIScrollbar = Terraria.GameContent.UI.Elements.FixedUIScrollbar;

namespace TerrariaCells.Content.UI
{
    public class ProgressTracker : WindowState
    {
        internal override string Name => "ProgressTracker";

        Button Button;
        ProgressTabs Tabs;
        DisplayPanel Display;

        //internal RasterizerState Terraria_UI_UIElement_OverflowHiddenRasterizerState;
        public override void OnInitialize()
        {
            Button = new Button();
            Button.buttonColor = Color.Red;
            Button.hoverColor = Color.PaleVioletRed;
            Button.hoverText = "Close";
            Button.img = Terraria.GameContent.TextureAssets.Cd;
            Button.imgColor = Color.Black;
            Button.OnLeftClick += (x, y) => DeadCellsUISystem.ToggleActive<ProgressTracker>(false);
            Append(Button);

            Tabs = new ProgressTabs();
            Tabs.Append(new ItemProgress($"Terraria/Images/Item_{ItemID.Starfury}", () => Common.GlobalNPCs.VanillaNPCShop.Weapons));
            Tabs.Append(new ItemProgress($"Terraria/Images/Item_{ItemID.MolotovCocktail}", () => Common.GlobalNPCs.VanillaNPCShop.Skills));
            Tabs.Append(new ItemProgress($"Terraria/Images/Item_{ItemID.BandofRegeneration}", () => Common.GlobalNPCs.VanillaNPCShop.Accessories));
            Tabs.Append(new ItemProgress($"Terraria/Images/Item_{ItemID.WizardHat}", () => Common.GlobalNPCs.VanillaNPCShop.Armors));
            Tabs.Append(new MetaProgress());

            Tabs.OnUpdate += (x) => { if(x.IsMouseHovering) Main.LocalPlayer.mouseInterface = true; };
            Append(Tabs);

            Display = new DisplayPanel();
            Display.OnUpdate += (x) => { if(x.IsMouseHovering) Main.LocalPlayer.mouseInterface = true; };
            Append(Display);

            Recalculate();

            //Terraria_UI_UIElement_OverflowHiddenRasterizerState = (RasterizerState)typeof(UIElement).GetField("OverflowHiddenRasterizerState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

            //On_UIElement.ContainsPoint += On_UIElement_ContainsPoint;
            //On_UIElement.GetElementAt += On_UIElement_GetElementAt;
            //On_UserInterface.GetMousePosition += On_UserInterface_GetMousePosition;
        }

        private void On_UserInterface_GetMousePosition(On_UserInterface.orig_GetMousePosition orig, UserInterface self)
        {
            orig.Invoke(self);
            if(self.CurrentState is ProgressTracker)
            {
                self.MousePosition = Main.MouseScreen * 0.5f;
            }
        }
        private UIElement On_UIElement_GetElementAt(On_UIElement.orig_GetElementAt orig, UIElement self, Vector2 point)
        {
            if(self is ProgressTracker)
            {
                point *= 0.5f;
                Main.NewText(Main.MouseScreen + " : " + Main.MouseScreen * 0.5f + " : " + point);
            }

            return orig.Invoke(self, point);
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

        protected override bool PreUpdate(GameTime time)
        {
            uiScaleForUpdating = Main.UIScale;
            //Main.UIScale = 2;

            return base.PreUpdate(time);
        }
        protected override void WindowUpdate(GameTime time)
        {
            Recalculate();
            Main.LocalPlayer.mouseInterface = true;

            //Main.UIScale = uiScaleForUpdating;
        }
        protected override bool PreDrawSelf(SpriteBatch spriteBatch)
        {
            PostDrawSelf(spriteBatch);
            return false;
        }
        private float uiScaleForUpdating;
        public override void Recalculate()
        {
            base.Recalculate();

            if(!isOpen) return;

            Left.Set(0, 0.3f);
            Top.Set(0, 0.25f);
            Width.Set(0, 0.4f);
            Height.Set(0, 0.5f);
        }
        public override void RecalculateChildren()
        {
            base.RecalculateChildren();

            if(!isOpen) return;

            const int TAB_HEIGHT = 52;

            if(Button is not null)
                Button.HAlign = 1;
            Button?.Width.Set(TAB_HEIGHT, 0);
            Button?.Height.Set(TAB_HEIGHT, 0);

            Tabs?.Width.Set(-TAB_HEIGHT, 1f);
            Tabs?.Height.Set(TAB_HEIGHT, 0);

            Display?.Width.Set(0, 1);
            Display?.Height.Set(WindowSize.Y - TAB_HEIGHT, 0);
            Display?.Top.Set(TAB_HEIGHT, 0);
        }

        private float uiScaleForDrawing;
        /*protected override bool PreDraw(SpriteBatch spriteBatch)
        {
            uiScaleForDrawing = Main.UIScale;
            Main.UIScale = 2;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, (this.OverrideSamplerState != null) ? this.OverrideSamplerState : SamplerState.AnisotropicClamp, DepthStencilState.None, Terraria_UI_UIElement_OverflowHiddenRasterizerState, (Effect)null, Main.UIScaleMatrix);
            return base.PreDraw(spriteBatch);
        }
        protected override void PostDraw(SpriteBatch spriteBatch)
        {
            base.PostDraw(spriteBatch);
            Main.UIScale = uiScaleForDrawing;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Terraria_UI_UIElement_OverflowHiddenRasterizerState, (Effect)null, Main.UIScaleMatrix);
        }*/

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
                    bool isMouseHover = IsMouseHovering;
                    Color drawColor = UIHelper.InventoryColour;
                    if (Selected)
                        drawColor = Main.OurFavoriteColor;
                    else if (isMouseHover)
                        drawColor = Color.MediumSlateBlue;

                    UIHelper.PANEL.Draw(spriteBatch, bounds, drawColor);
                    if (Icon?.IsLoaded == true)
                    {
                        spriteBatch.Draw(Icon.Value, bounds.Center(), null, Color.Black, 0f, Icon.Size() * 0.5f, MathF.Min(1.25f, (float)(bounds.Height-8) / Icon.Height()), SpriteEffects.None, 0f);
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
            if(triggersSet.Inventory)
            {
                DeadCellsUISystem.ToggleActive<ProgressTracker>(false);
            }
        }
    }

    public enum UnlockState
    {
        Locked,
        Unlocked,
        Found,
    }
    internal class ItemProgress : ProgressTabs.Tab
    {
        private class ItemViewport : UIElement
        {
            private class ItemDisplaySlot : UIElement
            {
                const float MAX_SCALE = 1.5f;
                public ItemDisplaySlot(int type, int size = 48, UnlockState unlocked = UnlockState.Locked)
                {
                    itemType = type;
                    this.Width.Set(-8, 0.5f);
                    this.Height.Set(size * MAX_SCALE, 0);
                    unlockState = unlocked;
                }
                internal int itemType;
                internal UnlockState unlockState;
                private Item Item
                {
                    get
                    {
                        Item result = new Item(itemType, 1, 0);
                        if(result.TryGetGlobalItem<Common.GlobalItems.FunkyModifierItemModifier>(out var gItem))
                        {
                            gItem.modifiers = Array.Empty<Common.GlobalItems.FunkyModifier>();
                        }
                        if(result.TryGetGlobalItem<Common.GlobalItems.TierSystemGlobalItem>(out var tItem))
                        {
                            tItem.SetLevel(result, ModContent.GetInstance<Common.Systems.TeleportTracker>().level);
                        }
                        return result;
                    }
                }
                public override int CompareTo(object obj)
                {
                    if(obj is not ItemDisplaySlot other)
                        return base.CompareTo(obj);
                    return this.itemType.CompareTo(other.itemType);
                }
                protected override void DrawSelf(SpriteBatch spriteBatch)
                {
                    base.DrawSelf(spriteBatch);
                    Rectangle bounds = GetDimensions().ToRectangle();
                    bool isMouseHover = IsMouseHovering;

                    Color panelColor = (!isMouseHover || unlockState == UnlockState.Locked) ? UIHelper.InventoryColour : Color.MediumSlateBlue;
                    UIHelper.PANEL.Draw(spriteBatch, bounds, panelColor);
                    Item drawItem = Item;

                    /*UIElement findParent = this;
                    while(findParent is not null and not UIState)
                    {
                        findParent = findParent.Parent;
                        if(findParent is ItemViewport viewPort)
                        {
                            unlockState = Main.LocalPlayer.GetModPlayer<MetaPlayer>().CheckUnlocks(drawItem);
                            break;
                        }
                    }*/

                    if(isMouseHover && unlockState == UnlockState.Found)
                    {
                        Main.HoverItem = drawItem;
                        Main.hoverItemName = drawItem.HoverName;
                    }

                    //Modify Item Draw Here
                    Main.instance.LoadItem(drawItem.type);
                    Texture2D value = TextureAssets.Item[drawItem.type].Value;
                    Rectangle frame = ((Main.itemAnimations[drawItem.type] == null) ? value.Frame() : Main.itemAnimations[drawItem.type].GetFrame(value));
                    Vector2 origin = frame.Size() * 0.5f;
                    Color itemUnlockColour = unlockState switch { UnlockState.Locked => Color.Black, UnlockState.Unlocked => Color.Black, UnlockState.Found => Color.White, _ => Color.Transparent };
                    ItemSlot.DrawItem_GetColorAndScale(drawItem, MAX_SCALE, ref itemUnlockColour, bounds.Height/2 - 8, ref frame, out Color drawColor, out float scale);
                    spriteBatch.Draw(value, bounds.TopLeft() + new Vector2(bounds.Height * 0.5f), frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

                    if(unlockState == UnlockState.Locked)
                    {
                        Main.instance.LoadItem(ItemID.ChestLock);
                        value = TextureAssets.Item[ItemID.ChestLock].Value;
                        frame = ((Main.itemAnimations[ItemID.ChestLock] == null) ? value.Frame() : Main.itemAnimations[ItemID.ChestLock].GetFrame(value));
                        origin = frame.Size() * 0.5f;
                        itemUnlockColour = Color.White;
                        ItemSlot.DrawItem_GetColorAndScale(drawItem, 1, ref itemUnlockColour, bounds.Height - 8, ref frame, out drawColor, out scale);
                        spriteBatch.Draw(value, bounds.TopLeft() + new Vector2(bounds.Height * 0.5f), frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                    //ItemSlot.DrawItemIcon(drawItem, 0, spriteBatch, bounds.TopLeft() + new Vector2(bounds.Height * 0.5f), (float)itemSize / 64f, itemSize - 8, itemUnlockColour);

                    //Modify Text Draw Here
                    DynamicSpriteFont font = FontAssets.MouseText.Value;
                    string localizedName = unlockState switch { UnlockState.Locked => "???", UnlockState.Unlocked => "???", UnlockState.Found => drawItem.Name, _ => string.Empty };
                    Color textColour = unlockState switch { UnlockState.Locked => Color.Gray, UnlockState.Unlocked => Color.Lerp(Drawing.GetRarityColor(drawItem.rare), Color.DarkGray, 0.67f), UnlockState.Found => Drawing.GetRarityColor(drawItem.rare), _ => Color.Transparent };
                    Vector2 size = font.MeasureString(localizedName);
                    Vector2 position = bounds.TopRight() + new Vector2(-8, 8);
                    ChatManager.DrawColorCodedStringWithShadow(
                        spriteBatch,
                        FontAssets.MouseText.Value,
                        localizedName,
                        position,
                        textColour,
                        0f,
                        new Vector2(size.X, 0),
                        new Vector2(MathF.Min(MAX_SCALE, ((bounds.Width - bounds.Height - 8)/size.X)))
                    );

                    position.Y += size.Y + 1;
                    textColour = unlockState switch { UnlockState.Locked => Color.PaleVioletRed, UnlockState.Unlocked => Color.DarkGray, UnlockState.Found => Color.LightGreen, _ => Color.Transparent };
                    localizedName = unlockState.ToString();
                    size = font.MeasureString(localizedName);
                    ChatManager.DrawColorCodedStringWithShadow(
                        spriteBatch,
                        FontAssets.MouseText.Value,
                        localizedName,
                        position,
                        textColour,
                        0f,
                        new Vector2(size.X, 0),
                        new Vector2(0.5f * MAX_SCALE)
                    );
                }
            }
            public ItemViewport(IEnumerable<int> collection) : base()
            {
                const int SCROLL_WIDTH = 20;
                const int PANEL_SIZE = 48;
                const int GRID_PADDING = 8;

                Scroller = new BetterFixedUIScrollbar(static () => DeadCellsUISystem.GetWindow<ProgressTracker>().UserInterface);
                //Scroller.SetView(100, 1000);
                Scroller.HAlign = 1;
                Scroller.Width.Set(SCROLL_WIDTH, 0);
                Scroller.Height.Set(0, 1);
                Append(Scroller);

                Unlocks = new UIGrid();
                Unlocks.Width.Set(-Scroller.Width.Pixels, 1);
                Unlocks.Height.Set(0, 1);
                foreach(int type in collection)
                {
                    ItemDisplaySlot slot = new ItemDisplaySlot(type, PANEL_SIZE, Main.LocalPlayer.GetModPlayer<MetaPlayer>().CheckUnlocks(type));
                    Unlocks.Add(slot);
                }
                Unlocks.SetScrollbar(Scroller);
                Unlocks.ListPadding = GRID_PADDING;
                Append(Unlocks);
            }
            private UIGrid Unlocks;
            private UIScrollbar Scroller;
        }
        private string iconTexture;
        public override string IconTexture => iconTexture;
        public ItemProgress(string texPath, Func<IEnumerable<int>> fetchItemCollection) : base()
        {
            iconTexture = texPath;
            fetchCollection = fetchItemCollection;
        }
        Func<IEnumerable<int>> fetchCollection;
        public override UIElement GetViewport() => new ItemViewport(fetchCollection.Invoke());
    }
    internal class MetaProgress : ProgressTabs.Tab
    {
        private class MetaViewport : UIElement
        {
            private class MetaToggleSlot : UIElement
            {
                LocalizedText DisplayText;
                int whoAmI;
                public MetaToggleSlot(int index)
                {
                    whoAmI = index;
                    DisplayText = TerrariaCells.Instance.GetLocalization($"ui.metaprogress.entry_{whoAmI}", () => whoAmI.ToString());
                }

                public override int CompareTo(object obj)
                {
                    if(obj is MetaToggleSlot slot)
                        return this.whoAmI.CompareTo(slot.whoAmI);
                    return base.CompareTo(obj);
                }
                protected override void DrawSelf(SpriteBatch spriteBatch)
                {
                    base.DrawSelf(spriteBatch);
                    Rectangle bounds = GetDimensions().ToRectangle();

                    var meta = Main.LocalPlayer.GetModPlayer<MetaPlayer>();
                    bool unlocked = meta.HasFlag(whoAmI);
                    Color drawColor = UIHelper.InventoryColour;
                    if(unlocked && IsMouseHovering)
                    {
                        drawColor = Color.MediumSlateBlue;
                    }
                    UIHelper.PANEL.Draw(spriteBatch, bounds, drawColor);

                    DynamicSpriteFont font = FontAssets.MouseText.Value;
                    string text = DisplayText.Value;
                    if(!unlocked)
                        text = "???";
                    Vector2 size = font.MeasureString(text);
                    ChatManager.DrawColorCodedStringWithShadow(
                        spriteBatch,
                        FontAssets.MouseText.Value,
                        text,
                        bounds.Left() + new Vector2(8, 0),
                        Color.White,
                        0f,
                        new Vector2(0, size.Y*0.5f),
                        new Vector2(MathF.Min(0.8f, (bounds.Width-16)/(size.X!=0?size.X:1)))
                    );
                }
                public override void LeftClick(UIMouseEvent evt)
                {
                    var meta = Main.LocalPlayer.GetModPlayer<MetaPlayer>();
                    if(!meta.HasFlag(whoAmI))
                    {
                        SoundEngine.PlaySound(SoundID.Tink);
                    }
                }
            }

            public MetaViewport()
            {
                const int SCROLL_WIDTH = 20;
                const int PANEL_SIZE = 32;
                const int GRID_PADDING = 8;

                Scroller = new BetterFixedUIScrollbar(() => DeadCellsUISystem.GetWindow<ProgressTracker>().UserInterface);
                Scroller.HAlign = 1;
                Scroller.Width.Set(SCROLL_WIDTH, 0);
                Scroller.Height.Set(0, 1);
                Append(Scroller);

                List = new UIList();
                List.Width.Set(-Scroller.Width.Pixels, 1);
                List.Height.Set(0, 1);
                var metaPlayer = Main.LocalPlayer.GetModPlayer<MetaPlayer>();
                for(int i = 0; i < MetaPlayer.ProgressionCount; i++)
                {
                    MetaToggleSlot slot = new MetaToggleSlot(i);
                    slot.Width.Set(-GRID_PADDING, 1);
                    slot.Height.Set(PANEL_SIZE, 0);
                    List.Add(slot);
                }
                List.SetScrollbar(Scroller);
                List.ListPadding = GRID_PADDING;
                Append(List);
            }
            UIList List;
            BetterFixedUIScrollbar Scroller;
        }
        public override UIElement GetViewport() => new MetaViewport();
        public override string IconTexture => $"Terraria/Images/Item_{ItemID.TrifoldMap}";
    }

    internal class BetterFixedUIScrollbar : UIScrollbar
    {
        private Func<UserInterface> fetch;
        private UserInterface? @interface;
        public BetterFixedUIScrollbar(Func<UserInterface> getInterface)
        {
            fetch = getInterface;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            @interface ??= fetch();
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = @interface;
            base.DrawSelf(spriteBatch);
            UserInterface.ActiveInstance = temp;
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            @interface ??= fetch();
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = @interface;
            base.LeftMouseDown(evt);
            UserInterface.ActiveInstance = temp;
        }
    }
}
