using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

enum Dir {
	Up,
	Right,
	Down,
	Left
}

struct Socket {
	public int x;
	public int y;
	public Dir dir;
}

namespace TerrariaCells.Utils
{
	public class RoomWrapper
	{
		public string roomPath;
		public int width, height;
		public List<Socket> sockets;
		public void ParseRoom(string path, Mod mod) {
			roomPath = path;
			System.IO.Stream stream = mod.GetFileStream(path);
			TagCompound tag = TagIO.FromStream(stream);
			stream.Close();
			width = tag.GetInt("Width");
			height = tag.GetInt("Height");
			var data = (List<TileSaveData>)tag.GetList<TileSaveData>("TileData");
			for (int i = 0; i < height / 5; i++) {
				int index = (width + 1) * (5 * i + 2);
				TileSaveData tile = data[index];
				// do this later
			}
		}
	}
}
