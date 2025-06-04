using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria;

namespace TerrariaCells.Common.UI {
    internal class DraggableUIPanel : UIPanel {
        private Vector2 offset;
        private bool dragging;

        public override void LeftMouseDown(UIMouseEvent evt) {
            base.LeftMouseDown(evt);
            if (evt.Target == this) {
                drag(evt);
            }
        }
        void drag(UIMouseEvent evt) {
            offset = new(
                evt.MousePosition.X - Left.Pixels,
                evt.MousePosition.Y - Top.Pixels
            );
            dragging = true;
        }

        public override void RightMouseDown(UIMouseEvent evt) {
            base.RightMouseDown(evt);
            if (evt.Target == this) {
                release(evt);
            }
        }
        void release(UIMouseEvent evt) {
            Left.Set(evt.MousePosition.X - offset.X, 0);
            Top.Set(evt.MousePosition.Y - offset.Y, 0);
            Recalculate();
            dragging = false;
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (dragging) {
                Left.Set(Main.mouseX - offset.X, 0);
                Top.Set(Main.mouseY - offset.Y, 0);
                Recalculate();
            }

            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace)) {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}
