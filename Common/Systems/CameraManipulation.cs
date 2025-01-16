using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.Systems;

public class CameraManipulation : ModSystem
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