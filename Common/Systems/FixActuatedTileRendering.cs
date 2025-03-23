using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems {
    internal class FixActuatedTileRendering : ModSystem {
        static bool shouldHackLighting = false;
        public override void Load() {
            On_Lighting.GetColor_int_int += HackLighting;
            On_TileDrawing.DrawTrees += HackTrees;
            On_TileDrawing.DrawGrass += HackGrass;
            On_TileDrawing.DrawAnyDirectionalGrass += HackAnyDirectionalGrass;
            On_TileDrawing.DrawMasterTrophies += HackMasterTrophies;
            On_TileDrawing.DrawTeleportationPylons += HackTeleportationPylons;
            On_TileDrawing.DrawMultiTileGrass += HackMultiTileGrass;
            On_TileDrawing.DrawMultiTileVines += HackMultiTileVines;
            On_TileDrawing.DrawVines += HackVines;
            On_TileDrawing.DrawReverseVines += HackReverseVines;
        }

        private static void HackReverseVines(On_TileDrawing.orig_DrawReverseVines orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackVines(On_TileDrawing.orig_DrawVines orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackMultiTileVines(On_TileDrawing.orig_DrawMultiTileVines orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackMultiTileGrass(On_TileDrawing.orig_DrawMultiTileGrass orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackTeleportationPylons(On_TileDrawing.orig_DrawTeleportationPylons orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackMasterTrophies(On_TileDrawing.orig_DrawMasterTrophies orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackAnyDirectionalGrass(On_TileDrawing.orig_DrawAnyDirectionalGrass orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackGrass(On_TileDrawing.orig_DrawGrass orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static void HackTrees(On_TileDrawing.orig_DrawTrees orig, TileDrawing self) {
            shouldHackLighting = true;
            orig(self);
            shouldHackLighting = false;
        }

        private static Microsoft.Xna.Framework.Color HackLighting(On_Lighting.orig_GetColor_int_int orig, int x, int y) {
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
