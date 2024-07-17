global using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerrariaCells.WorldGen;

namespace TerrariaCells
{
	//Contributions already present are by no means absolute, conventions are negotiable.
	public class TerrariaCells : Mod
	{
		public override void Load() {
			Room.LoadRooms(this);

            IL_Player.Update += PatchPlayerSpaceGravity;
		}

        // TODO: Put this IL edit somewhere better.
        private static void PatchPlayerSpaceGravity(ILContext ctx) {
            var cursor = new ILCursor(ctx);

			cursor.GotoNext(i => i.MatchLdloc3() && i.Next.MatchMul());
            cursor.Remove();
            cursor.EmitLdcR4(1.0f);
        }
	}

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
