using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.UI.Components
{
    public class Button : UIElement
    {
        public Color buttonColor;
        public Color hoverColor;
        public string hoverText;

        public ReLogic.Content.Asset<Texture2D>? img = null;
        public Color imgColor = Color.White;

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
            
            if(img is not null && img.IsLoaded)
            {
                spriteBatch.Draw(img.Value, GetDimensions().Center(), null, imgColor, 0f, img.Size() * 0.5f, Vector2.One, SpriteEffects.None, 0f);
            }
        }
    }
}
