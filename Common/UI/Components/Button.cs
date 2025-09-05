using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Terraria.UI;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.UI.Components
{
    public class Button : UIElement
    {
        public Color buttonColor;
        public Color hoverColor;
        public string hoverText;

        protected override void DrawSelf(SpriteBatch spriteBatch) 
        {
            base.DrawSelf(spriteBatch);

            Color drawColor = buttonColor;
            if(IsMouseHovering)
            {
                drawColor = hoverColor;
                Terraria.Main.instance.MouseText(hoverText);
            }
            UIHelper.PANEL.Draw(spriteBatch, GetDimensions().ToRectangle(), drawColor);
        }
    }
}
