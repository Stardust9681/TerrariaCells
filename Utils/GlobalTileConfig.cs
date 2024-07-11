using Terraria.ModLoader;
using Terraria;

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
			isBuilder = false;
			Player.noBuilding = true;
		}
		public override void UpdateEquips()
		{
			Player.noBuilding = !isBuilder;
		}
	}
}
