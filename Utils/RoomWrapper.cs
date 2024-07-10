using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using TerrariaCells.Content.Tiles;

public enum Dir {
	Up,
	Right,
	Down,
	Left
}

public struct Socket {
	public int x;
	public int y;
	public Dir dir;
	public Socket(int X, int Y, Dir d) {
		x = X;
		y = Y;
		dir = d;
	}
}

namespace TerrariaCells.Utils
{
	public class RoomWrapper
	{
		public string roomPath;
		public int width, height;
		public List<Socket> sockets = new List<Socket>();
		public void ParseRoom(string path, Mod mod) {
			roomPath = path;
			System.IO.Stream stream = mod.GetFileStream(path);
			TagCompound tag = TagIO.FromStream(stream);
			stream.Close();
			width = tag.GetInt("Width");
			height = tag.GetInt("Height");
			var data = (List<StructureHelper.TileSaveData>)tag.GetList<StructureHelper.TileSaveData>("TileData");
			int connectorType = ModContent.TileType<RoomConnectorTile>();
			// index for x, y on structurehelper: y + x * (height + 1)
			// for left side
			for (int i = 0; i < height / 5; i++) {
				int index = 5 * i + 2; // i is just y, x is 0
				StructureHelper.TileSaveData tile = data[index];
				if(int.TryParse(tile.tile, out int type)) {
					if (type == connectorType) {
						sockets.Add(new Socket(0, i, Dir.Left));
					}
				}
			}
			// for right side
			for (int i = 0; i < height / 5; i++) {
				int index = 5 * i + 2 + (width - 1) * (height + 1); // i is just y, x is width - 1 
				StructureHelper.TileSaveData tile = data[index];
				if(int.TryParse(tile.tile, out int type)) {
					if (type == connectorType) {
						sockets.Add(new Socket(width - 1, i, Dir.Right));
					}
				}
			}
			// for top
			for (int i = 0; i < height / 5; i++) {
				int index = (5 * i + 2) * (height + 1) + (height - 1); // i is just x, y is height - 1
				StructureHelper.TileSaveData tile = data[index];
				if(int.TryParse(tile.tile, out int type)) {
					if (type == connectorType) {
						sockets.Add(new Socket(i, height - 1, Dir.Up));
					}
				}
			}
			// for bottom
			for (int i = 0; i < height / 5; i++) {
				int index = (5 * i + 2) * (height + 1); // i is just x, y is 0
				StructureHelper.TileSaveData tile = data[index];
				if(int.TryParse(tile.tile, out int type)) {
					if (type == connectorType) {
						sockets.Add(new Socket(i, 0, Dir.Down));
					}
				}
			}
		}
	}
}
