//Imported from one of my mods
//Inspired in large by DragonLens' UI
// -Stardust

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.UI.Components.Windows
{
    public abstract class WindowState : UIState
    {
        //===== Window Content =====
        public UserInterface UserInterface { get; internal set; }
        public bool Active { get; private set; } = false;
        internal abstract string Name { get; }
        internal virtual string InsertionIndex => "Vanilla: Mouse Text";

        public Vector2 WindowPosition
        {
            get
            {
                return new Vector2(Left.GetValue(Main.screenWidth), Top.GetValue(Main.screenHeight));
            }
            protected set
            {
                Left.Set(0, value.X/Main.screenWidth);
                Top.Set(0, value.Y/Main.screenHeight);
            }
        }
        public Vector2 WindowSize
        {
            get
            {
                return new Vector2(Width.GetValue(Main.screenWidth), Height.GetValue(Main.screenHeight));
            }
            protected set
            {
                Width.Set(value.X, 0);
                Height.Set(value.Y, 0);
            }
        }
        public Rectangle Bounds => new Rectangle((int)WindowPosition.X, (int)WindowPosition.Y, (int)WindowSize.X, (int)WindowSize.Y);
        protected virtual void OnOpened() { }
        public void Open()
        {
            if (!Active)
                OnOpened();
            Active = true;
        }
        protected virtual void OnClosed() { }
        public void Close()
        {
            if (Active)
                OnClosed();
            Active = false;
        }

        //===== Element-Managing Methods =====
        public void AddElement(UIElement element, int x, int y, int width, int height)
        {
            element.Left.Set(x, 0);
            element.Top.Set(y, 0);
            element.Width.Set(width, 0);
            element.Height.Set(height, 0);
            Append(element);
        }
        public IEnumerable<T> GetChildrenOfType<T>() where T : UIElement
        {
            return from child in Children where child.GetType().IsAssignableTo(typeof(T)) select (T)child;
        }
        public bool IsMouseOnWindow()
        {
            return ContainsPoint(Main.MouseScreen);
        }

        //===== Inheritence-Helping Methods =====
        //note: Issue with these being overridden directly, is that it stops the thing that makes these work in the first place
        #region Left Mouse Events
        protected virtual void MouseLeftTwice(UIMouseEvent _event) { }
        public sealed override void LeftDoubleClick(UIMouseEvent evt)
        {
            base.LeftDoubleClick(evt);
            MouseLeftTwice(evt);
        }
        protected virtual void MouseLeftOnce(UIMouseEvent _event) { }
        public sealed override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            MouseLeftOnce(evt);
        }
        protected virtual void MouseLeftDown(UIMouseEvent _event) { }
        public sealed override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            MouseLeftDown(evt);
        }
        protected virtual void MouseLeftUp(UIMouseEvent _event) { }
        public sealed override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            MouseLeftUp(evt);
        }
        #endregion
        #region Right Mouse Events
        protected virtual void MouseRightTwice(UIMouseEvent _event) { }
        public sealed override void RightDoubleClick(UIMouseEvent evt)
        {
            base.RightDoubleClick(evt);
            MouseRightTwice(evt);
        }
        protected virtual void MouseRightOnce(UIMouseEvent _event) { }
        public sealed override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            MouseRightOnce(evt);
        }
        protected virtual void MouseRightDown(UIMouseEvent _event) { }
        public sealed override void RightMouseDown(UIMouseEvent evt)
        {
            base.RightMouseDown(evt);
            MouseRightDown(evt);
        }
        protected virtual void MouseRightUp(UIMouseEvent _event) { }
        public sealed override void RightMouseUp(UIMouseEvent evt)
        {
            base.RightMouseUp(evt);
            MouseRightUp(evt);
        }
        #endregion
        #region Mouse Events
        protected virtual void MouseEnter(UIMouseEvent _event) { }
        public sealed override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            MouseEnter(evt);
        }
        protected virtual void MouseExit(UIMouseEvent _event) { }
        public sealed override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            MouseExit(evt);
        }
        protected virtual void MouseScroll(UIMouseEvent _event) { }
        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            MouseScroll(evt);
        }
        #endregion
        #region Update
        ///<summary>Only return false if you know what you are doing. Called before <see cref="WindowUpdate(GameTime)"/></summary>
        /// <returns><b>True</b> by default.</returns>
        protected virtual bool PreUpdate(GameTime time) { return true; }
        ///<summary>Only called if <see cref="PreUpdate(GameTime)"/> return true.</summary>
        protected virtual void WindowUpdate(GameTime time) { }
        public sealed override void Update(GameTime gameTime)
        {
            if (PreUpdate(gameTime))
            {
                _update(gameTime);
                if (IsMouseOnWindow())
                    Main.LocalPlayer.mouseInterface = true;
                WindowUpdate(gameTime);
            }
        }
        internal void _update(GameTime time) => base.Update(time);
        ///<summary>Only return false if you know what you are doing. Called before <see cref="DrawChildren(SpriteBatch)"/></summary>
        /// <returns><b>True</b> by default.</returns>
        protected virtual bool PreDrawChildren(SpriteBatch spriteBatch) { return true; }
        protected sealed override void DrawChildren(SpriteBatch spriteBatch)
        {
            if (PreDrawChildren(spriteBatch)) base.DrawChildren(spriteBatch);
        }
        ///<summary>Only return false if you know what you are doing. Called before <see cref="Draw(SpriteBatch)"/></summary>
        /// <returns><b>True</b> by default.</returns>
        protected virtual bool PreDraw(SpriteBatch spriteBatch) { return true; }
        public sealed override void Draw(SpriteBatch spriteBatch)
        {
            if (PreDraw(spriteBatch)) _draw(spriteBatch);
        }
        internal void _draw(SpriteBatch spriteBatch) => base.Draw(spriteBatch);
        /// <summary>Only return false if you know what you are doing. Called before <see cref="DrawSelf(SpriteBatch)"/></summary>
        /// <returns><b>True</b> by default.</returns>
        protected virtual bool PreDrawSelf(SpriteBatch spriteBatch) { return true; }
        ///<summary>Only called if <see cref="PreDrawSelf(SpriteBatch)"/> returns true. Use this to draw over the default background panel.</summary>
        protected virtual void PostDrawSelf(SpriteBatch spriteBatch) { }
        protected sealed override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (PreDrawSelf(spriteBatch))
            {
                UIHelper.PANEL.Draw(spriteBatch, Bounds, UIHelper.InventoryColour);
                PostDrawSelf(spriteBatch);                
            }
        }
        #endregion
    }
}