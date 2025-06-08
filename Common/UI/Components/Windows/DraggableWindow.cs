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

namespace TerrariaCells.Common.UI.Components.Windows
{
    public abstract class DraggableWindow : WindowState
    {
        public const float NoDragZone = 10;
        public abstract Rectangle GrabBox { get; }
        public bool Dragging { get; private set; }
        private Vector2 dragOffset;

        protected virtual void UpdateChildPositions(Vector2 newPosition) { }

        protected virtual void Init() { }
        public sealed override void OnInitialize()
        {
            WindowPosition = new Vector2(Main.screenWidth - WindowSize.X, Main.screenHeight - WindowSize.Y) * 0.5f;
            Init();
            dragOffset = Vector2.Zero;
            Dragging = false;
            UpdateChildPositions(WindowPosition);
        }

        protected virtual void DraggableUpdate(GameTime time) { }
        protected sealed override void WindowUpdate(GameTime time)
        {
            Recalculate();

            if (!Main.mouseLeft && Dragging)
            {
                Dragging = false;
            }
            else if (GrabBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && !Dragging)
            {
                Dragging = true;
                dragOffset = (Main.MouseScreen - WindowPosition);
            }

            if (Dragging)
            {
                Vector2 newPos = Main.MouseScreen - dragOffset;
                if (newPos.X < NoDragZone)
                    newPos.X = NoDragZone;
                else if (newPos.X + WindowSize.X > Main.screenWidth - NoDragZone)
                    newPos.X = Main.screenWidth - NoDragZone - WindowSize.X;
                if (newPos.Y < NoDragZone)
                    newPos.Y = NoDragZone;
                else if (newPos.Y + WindowSize.Y > Main.screenHeight - NoDragZone)
                    newPos.Y = Main.screenHeight - NoDragZone - WindowSize.Y;
                UpdateChildPositions(newPos);
                WindowPosition = newPos;
            }

            Recalculate();

            if (IsMouseOnWindow())
                Main.LocalPlayer.mouseInterface = true;

            DraggableUpdate(time);
            base.WindowUpdate(time);
        }
    }
}
