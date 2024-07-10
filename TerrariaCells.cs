using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using TerrariaCells.WorldGen;

namespace TerrariaCells
{
	//Contributions already present are by no means absolute, conventions are negotiable.
	public class TerrariaCells : Mod
	{
		public override void Load() {

			Room.LoadRooms(this);

			this.Logger.Debug(Room.Rooms[8].Connections.Count);

			foreach (var conn in  Room.Rooms[8].Connections) {
				this.Logger.Debug(conn.side);
			}
		}
	}
}
