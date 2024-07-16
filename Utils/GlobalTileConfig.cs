using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;

namespace TerrariaCells.Utils
{
	public class GlobalTileConfig : GlobalTile
	{
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			return GlobalPlayer.isBuilder;
		}

		public override bool CanExplode(int i, int j, int type)
		{
			return GlobalPlayer.isBuilder;
		}
	}

	public class GlobalWallConfig : GlobalWall
	{
		public override void KillWall(int i, int j, int type, ref bool fail)
		{
			fail = !GlobalPlayer.isBuilder;
		}
	} 

	
	public class GlobalPlayer : ModPlayer 
	{
		public static bool isBuilder = false;
		public override void OnEnterWorld() 
		{	
			ushort tileId = TileID.Pots;
			int style = 0;
			FlexibleTileWand.ForModders_AddPotsToWand(FlexibleTileWand.RubblePlacementMedium, ref style, ref tileId);
			isBuilder = false;
			Player.noBuilding = true;
		}
		public override void UpdateEquips()
		{
			Player.noBuilding = !isBuilder;
		}
	}
}
