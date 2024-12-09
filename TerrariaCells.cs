global using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using TerrariaCells.Content.UI;

namespace TerrariaCells
{
    //Contributions already present are by no means absolute, conventions are negotiable.
    public class TerrariaCells : Mod { }

    public class TerraCellsSystem : ModSystem
    {
        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (Main.gameMenu || TerrariaCellsConfig.Instance.DisableZoom)
                return;

            // Caps zoom at 175%-200%
            float zoomClamp = Main.GameViewMatrix.Zoom.X;
            zoomClamp = Math.Max(zoomClamp, 1.75f);
            Transform.Zoom = Vector2.One * zoomClamp;
        }
    }
}
