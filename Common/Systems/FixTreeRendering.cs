using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems {
    internal class FixTreeRendering : ModSystem {
        static bool shouldHackLighting = false;
        public override void Load() {
            On_Lighting.GetColor_int_int += HackLighting;
            On_TileDrawing.DrawTrees += DetectTreeRendering;
        }

        private static void DetectTreeRendering(On_TileDrawing.orig_DrawTrees orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static Color HackLighting(On_Lighting.orig_GetColor_int_int orig, int x, int y) {
            var result = orig(x, y);
            if (shouldHackLighting && Main.tile[x, y].IsActuated) {
                return new(
                    (byte) (0.4 * result.R),
                    (byte) (0.4 * result.G),
                    (byte) (0.4 * result.B),
                    result.A
                );
            }
            return result;
        }
    }
}
