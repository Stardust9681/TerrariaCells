using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.WorldGen {

	internal enum RoomConnectionSide {
		Left,
		Right,
		Top,
		Bottom
	}

	internal static class RoomConnectionSideExt {
		public static RoomConnectionSide Opposite(this RoomConnectionSide side) {
			return side switch {
				RoomConnectionSide.Left => RoomConnectionSide.Right,
				RoomConnectionSide.Right => RoomConnectionSide.Left,
				RoomConnectionSide.Top => RoomConnectionSide.Bottom,
				RoomConnectionSide.Bottom => RoomConnectionSide.Top,
				_ => throw new ArgumentException("unknown side")
			};
		}
	}

	internal class RoomConnection(Room room, RoomConnectionSide side, int offset, int length) {
		public readonly Room Room = room;
		public readonly RoomConnectionSide Side = side;
		public readonly int Offset = offset;
		public readonly int Length = length;
	}

	internal class Room {

		public const string RoomPrefix = "WorldGen/Rooms/";

		public static readonly string[] RoomNames = [
			"test_end_l",
			"test_end_r",
			"test_end_t",
			"test_end_b",
			"test_corridor",
			"test_vert",
			"test_vert_mixed",
			"test_height_change",
			"test_4way"
		];

		public static readonly List<Room> Rooms = [];

		public List<RoomConnection> Connections { get; private set; } = [];

		public TagCompound Tag { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		private static bool IsConnector(IList<TagCompound> data, int width, int height, int x, int y, Mod mod) {
			if (x < 0 || x >= width) { throw new IndexOutOfRangeException("x is out of range"); }
			if (y < 0 || y >= height) { throw new IndexOutOfRangeException("y is out of range"); }
			
			int index = x * height + y;
			var tileTag = data[index];
			return tileTag.GetString("Tile") == mod.Name + " " + ModContent.GetModTile(ModContent.TileType<RoomConnectorTile>()).Name;
		}

		private static void ClearTile(IList<TagCompound> data, int width, int height, int x, int y) {
			if (x < 0 || x >= width) { throw new IndexOutOfRangeException("x is out of range"); }
			if (y < 0 || y >= height) { throw new IndexOutOfRangeException("y is out of range"); }

			int index = x * height + y;
			var tileTag = data[index];
			
			tileTag.Set("Tile", "0", true);

			var wallWireData = tileTag.GetInt("WallWireData");
			wallWireData &= ~0b1;
			tileTag.Set("WallWireData", wallWireData, true);
		}

		public Room(string path, Mod mod) {

			using (var stream = mod.GetFileStream(path)) {
				this.Tag = TagIO.FromStream(stream);
			}

			if (this.Tag == null) {
				throw new Exception($"unable to load structure file ${path}");
			}

			var data = this.Tag.GetList<TagCompound>("TileData");
			var width = this.Tag.GetInt("Width") + 1;
			var height = this.Tag.GetInt("Height") + 1;
			
			this.Width = width;
			this.Height = height;

			// Check for connections at the top.
			int x = 0;
			while (x < width) {
				if (IsConnector(data, width, height, x, 0, mod)) {
					ClearTile(data, width, height, x, 0);

					int end = x + 1;
					while (end < width) {
						if (!IsConnector(data, width, height, end, 0, mod)) {
							break;
						}
						ClearTile(data, width, height, end, 0);
						end++;
					}

					var connection = new RoomConnection(this, RoomConnectionSide.Top, x, end - x);
					this.Connections.Add(connection);

					x = end;
				} else {
					x++;
				}
			}

			// Check for connections at the bottom.
			x = 0;
			while (x < width) {
				if (IsConnector(data, width, height, x, height - 1, mod)) {
					ClearTile(data, width, height, x, height - 1);

					int end = x + 1;
					while (end < width) {
						if (!IsConnector(data, width, height, end, height - 1, mod)) {
							break;
						}
						ClearTile(data, width, height, end, height - 1);
						end++;
					}

					var connection = new RoomConnection(this, RoomConnectionSide.Bottom, x, end - x);
					this.Connections.Add(connection);

					x = end;
				} else {
					x++;
				}
			}

			// Check for connections at the left.
			int y = 0;
			while (y < height) {
				if (IsConnector(data, width, height, 0, y, mod)) {
					ClearTile(data, width, height, 0, y);

					int end = y + 1;
					while (end < height) {
						if (!IsConnector(data, width, height, 0, end, mod)) {
							break;
						}
						ClearTile(data, width, height, 0, end);
						end++;
					}

					var connection = new RoomConnection(this, RoomConnectionSide.Left, y, end - y);
					this.Connections.Add(connection);

					y = end;
				} else {
					y++;
				}
			}

			// Check for connections at the right.
			y = 0;
			while (y < height) {
				if (IsConnector(data, width, height, width - 1, y, mod)) {
					ClearTile(data, width, height, width - 1, y);

					int end = y + 1;
					while (end < height) {
						if (!IsConnector(data, width, height, width - 1, end, mod)) {
							break;
						}
						ClearTile(data, width, height, width - 1, end);
						end++;
					}

					var connection = new RoomConnection(this, RoomConnectionSide.Right, y, end - y);
					this.Connections.Add(connection);

					y = end;
				} else {
					y++;
				}
			}
		}

		public static void LoadRooms(Mod mod) {
			foreach (var name in RoomNames) {
				Rooms.Add(new Room(RoomPrefix + name, mod));
			}
		}
	}
}
