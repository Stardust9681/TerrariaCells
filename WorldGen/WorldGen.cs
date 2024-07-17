using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TerrariaCells.WorldGen {
	class WorldGen : ModSystem {
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {

			// Disable vanilla world gen tasks.
			foreach (var task in tasks) {
				// TODO: I'm not sure if anything non-obvious breaks by skipping the Reset task.
				if (task.Name != "Reset") {
				   task.Disable();
				}
			}

			tasks.Add(new GenerateRoomsPass());
		}
	}

	class GenerateRoomsPass : GenPass {

		const int ConnectorFrontClearance = 9;
		const int ConnectorSideClearance = 2;

		private class PlacedRoom {
			public readonly Room Room;
			public Point Position;
			public readonly List<PlacedConnection> ExposedConnections;
			public bool IsSpawnRoom = false;
			public readonly List<PlacedConnection> OpenConnections = [];

			public PlacedRoom(Room room, Point position, RoomConnection coveredConnection = null) {
				this.Room = room;
				this.Position = position;

				this.ExposedConnections = (
					from conn in room.Connections
					where conn != coveredConnection
					select new PlacedConnection(this, conn)
				).ToList();
			}

			public List<Rectangle> GetRectangles() {

				var roomRect = new Rectangle(
					this.Position.X, this.Position.Y,
					this.Room.Width, this.Room.Height
				);

				if (this.Room.IsSurface) {
					roomRect.Y = -(2 << 16);
					roomRect.Height += (2 << 16) + this.Position.Y;
				}

				List<Rectangle> rects = [roomRect];

				foreach (var conn in this.ExposedConnections) {
					var clearanceRect = conn.GetClearanceRect();

					// TODO: This is a hack which extends the clearance of surface connections to (effectively) the top
					//       of the world, like what is done for the room rectangles.
					if (this.Room.IsSurface && conn.Connection.Length == 1) {
						clearanceRect.Height = clearanceRect.Y + (2 << 16);
						clearanceRect.Y = -(2 << 16);
					}
					
					rects.Add(clearanceRect);
				}

				return rects;
			}

			public bool Intersects(PlacedRoom other) {
				var thisRects = this.GetRectangles();
				var otherRects = other.GetRectangles();

				return (
					from rectA in thisRects
					from rectB in otherRects
					where rectA.Intersects(rectB)
					select new { }
				).Any();
			}

		}

		private class PlacedConnection {
			public readonly Point Position;
			public readonly PlacedRoom PlacedRoom;
			public readonly RoomConnection Connection;

			public PlacedConnection(PlacedRoom room, RoomConnection connection) {

				var offset = connection.Side switch {
					RoomConnectionSide.Left => new Point(0, connection.Offset),
					RoomConnectionSide.Right => new Point(room.Room.Width, connection.Offset),
					RoomConnectionSide.Top => new Point(connection.Offset, 0),
					RoomConnectionSide.Bottom => new Point(connection.Offset, room.Room.Height),
					_ => throw new Exception("invalid room connection side"),
				};

				this.Position = room.Position + offset;
				this.PlacedRoom = room;
				this.Connection = connection;
			}

			public Rectangle GetClearanceRect() {
				return this.Connection.Side switch {
					RoomConnectionSide.Left => new Rectangle(
						this.Position.X - ConnectorFrontClearance,
						this.Position.Y - ConnectorSideClearance,
						ConnectorFrontClearance,
						ConnectorSideClearance * 2 + this.Connection.Length
					),
					RoomConnectionSide.Right => new Rectangle(
						this.Position.X,
						this.Position.Y - ConnectorSideClearance,
						ConnectorFrontClearance,
						ConnectorSideClearance * 2 + this.Connection.Length
					),
					RoomConnectionSide.Top => new Rectangle(
						this.Position.X - ConnectorSideClearance,
						this.Position.Y - ConnectorFrontClearance,
						ConnectorSideClearance * 2 + this.Connection.Length,
						ConnectorFrontClearance
					),
					RoomConnectionSide.Bottom => new Rectangle(
						this.Position.X - ConnectorSideClearance,
						this.Position.Y,
						ConnectorSideClearance * 2 + this.Connection.Length,
						ConnectorFrontClearance
					),
					_ => throw new Exception("invalid room connection side"),
				};
			}
		}

		private class RoomList {
			public List<PlacedRoom> Rooms = [];

			public Rectangle GetBounds() {

				int left = 0;
				int right = 0;
				int top = 0;
				int bottom = 0;

				foreach (var room in this.Rooms) {
					if (room.Position.X < left) {
						left = room.Position.X;
					}
					if (room.Position.Y < top) {
						top = room.Position.Y;
					}
					if (room.Position.X + room.Room.Width > right) {
						right = room.Position.X + room.Room.Width;
					}
					if (room.Position.Y + room.Room.Height > bottom) {
						bottom = room.Position.Y + room.Room.Height;
					}
				}

				return new Rectangle(left, top, right - left, bottom - top);
			}

			public void Offset(int x, int y) {
				foreach (var room in this.Rooms) {
					room.Position += new Point(x, y);
				}
			}
		}

		// TODO: Come up with a better name than QueueFrame? (it made more sense when it was a stack).
		private class QueueFrame(PlacedRoom room) {
			public PlacedRoom Room = room;
		}

		public GenerateRoomsPass() : base("Generate Rooms", 1.0) { }

		private static Point PositionRoomByConnection(Room room, RoomConnection connection, Point connPosition) {
			return connection.Side switch {
				RoomConnectionSide.Left => new Point(connPosition.X, connPosition.Y - connection.Offset),
				RoomConnectionSide.Right => new Point(connPosition.X - room.Width, connPosition.Y - connection.Offset),
				RoomConnectionSide.Top => new Point(connPosition.X - connection.Offset, connPosition.Y),
				RoomConnectionSide.Bottom => new Point(connPosition.X - connection.Offset, connPosition.Y - room.Height),
				_ => throw new Exception("invalid room connection side"),
			};
		}

		private static List<PlacedRoom> GetValidRoomList(RoomList roomList, PlacedConnection coveredConnection) {

			List<PlacedRoom> validRooms = [];

			foreach (var placingRoom in Room.Rooms) {
				foreach (var placingRoomConnection in placingRoom.Connections) {
					if (coveredConnection.Connection.Length == placingRoomConnection.Length && coveredConnection.Connection.Side == placingRoomConnection.Side.Opposite()) {

						var roomPos = PositionRoomByConnection(placingRoom, placingRoomConnection, coveredConnection.Position);
						var newPlacedRoom = new PlacedRoom(placingRoom, roomPos, placingRoomConnection);

						bool intersects = false;

						foreach (var exisingRoom in roomList.Rooms) {
							if (newPlacedRoom.Intersects(exisingRoom)) {
								intersects = true;
								break;
							}
						}

						if (!intersects) {
							validRooms.Add(newPlacedRoom);
						}

					}
				}
			}

			return validRooms;
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {

			progress.Message = "Generating Rooms";
			var rand = Terraria.WorldGen.genRand;

			var queue = new Queue<QueueFrame>();

			var starterRoomIndex = rand.Next(0, Room.Rooms.Count);
			var starterRoom = new PlacedRoom(Room.Rooms[starterRoomIndex], new Point(0, 0)) {
				IsSpawnRoom = true
			};

			var roomList = new RoomList();
			roomList.Rooms.Add(starterRoom);

			var starterQueueFrame = new QueueFrame(starterRoom);

			queue.Enqueue(starterQueueFrame);

			while (queue.Count > 0) {

				var queueFrame = queue.Peek();

				if (queueFrame.Room.ExposedConnections.Count > 0 && queue.Count < 20) {

					var connection = queueFrame.Room.ExposedConnections[0];
					queueFrame.Room.ExposedConnections.RemoveAt(0);

					var validRooms = GetValidRoomList(roomList, connection);

					if (validRooms.Count > 0) {

						validRooms.Sort((roomA, roomB) => roomA.Room.Connections.Count.CompareTo(roomB.Room.Connections.Count));

						// weighted random function explanation: last number has weight 1, next last number 3, then 5, 7, 9, etc until most weight is the len of the rooms * 2 + 1
						// sorted by amount of connections
						// will probably change later
						// ask @lunispang for explanation if confused
						int roomCount = validRooms.Count;
						int chosenIndex = (int)Math.Sqrt(rand.Next(0, roomCount * roomCount));
						if (roomList.Rooms.Count > 20) {
							chosenIndex = roomCount - chosenIndex - 1; // reverse priority to have higher chance of selecting room with few connections
						}

						var room = validRooms[chosenIndex];

						roomList.Rooms.Add(room);
						queue.Enqueue(new QueueFrame(room));

					} else {
						// no valid rooms were found, resulting in open connection
						// TODO: Replace with backtracking?
						queueFrame.Room.OpenConnections.Add(connection);
					}

				} else {
					queue.Dequeue();
				}
			}

			// Set world surface height.
			//Main.worldSurface = Main.maxTilesY * 0.17; // TODO: This is just temporary to silence some errors. // no errors anymore, will comment out for now

			Utils.GlobalPlayer.isBuilder = true;

			const int offsetFromOrigin = 100;

			var roomListBounds = roomList.GetBounds();
			roomList.Offset(offsetFromOrigin - roomListBounds.Left, offsetFromOrigin - roomListBounds.Top);

			// Fill open connections
			foreach (var room in roomList.Rooms) {
				StructureHelper.Generator.Generate(room.Room.Tag, new Terraria.DataStructures.Point16(room.Position));

				if (room.IsSpawnRoom) {
					Main.spawnTileX = room.Position.X + 2;
					Main.spawnTileY = room.Position.Y + 3;
				}

				foreach (var conn in room.OpenConnections) {

					Point position = conn.Position;
					switch (conn.Connection.Side) {
						case RoomConnectionSide.Right:
							position.X--;
							break;
						case RoomConnectionSide.Bottom:
							position.Y--;
							break;
						default: break;
					}
					for (int i = 0; i < conn.Connection.Length; i++) {
						if (Terraria.WorldGen.InWorld(position.X, position.Y)) {
							Terraria.WorldGen.PlaceTile(position.X, position.Y, TileID.Obsidian);
							Terraria.WorldGen.KillWall(position.X, position.Y, false);
						}
						switch (conn.Connection.Side) {
							case RoomConnectionSide.Top: case RoomConnectionSide.Bottom: { position.X++; break; }
							case RoomConnectionSide.Left: case RoomConnectionSide.Right: { position.Y++; break; }
						}
					}

				}
			}

			// generate the blocks around rooms to fill in gaps
			int depth = 50;
			//source, dest, c_depth
			Queue<(Point, Point, int)> tiles = new();

			foreach (var room in roomList.Rooms) {
				var roomPos = new Point(room.Position.X, room.Position.Y);

				//iterate through top&bottom side tiles
				for (int i = 0; i < room.Room.Width; i++) {
					//top
					if (!Terraria.WorldGen.TileEmpty(roomPos.X + i, roomPos.Y) &! room.Room.IsSurface) {
						tiles.Enqueue((new Point(roomPos.X + i, roomPos.Y), new Point(roomPos.X + i, roomPos.Y - 1), depth));
					}
					//bottom
					if (!Terraria.WorldGen.TileEmpty(roomPos.X + i, roomPos.Y + room.Room.Height - 1)) {
						tiles.Enqueue((new Point(roomPos.X + i, roomPos.Y + room.Room.Height - 1), new Point(roomPos.X + i, roomPos.Y + room.Room.Height), depth));
					}
				}
				//iterate through left&right side tiles
				if(!room.Room.IsSurface){
					for (int i = 0; i < room.Room.Height; i++) {
						//left
						if (!Terraria.WorldGen.TileEmpty(roomPos.X, roomPos.Y + i)) {
							tiles.Enqueue((new Point(roomPos.X, roomPos.Y + i), new Point(roomPos.X, roomPos.Y + i), depth));
						}
						//right
						if (!Terraria.WorldGen.TileEmpty(roomPos.X + room.Room.Width - 1, roomPos.Y + i)) {
							tiles.Enqueue((new Point(roomPos.X + room.Room.Width - 1, roomPos.Y + i), new Point(roomPos.X + room.Room.Width, roomPos.Y + i), depth));
						}
					}
				}
			}
			Point[] directions = [new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)];
			while (tiles.Count > 0) {
				var tile = tiles.Dequeue();
				// TODO: generate tiles and push new possible tiles if current depth is positive
				var (source, dest, d) = tile;
				if (d > 0 && Terraria.WorldGen.InWorld(dest.X, dest.Y) && Terraria.WorldGen.TileEmpty(dest.X, dest.Y)) {
					var tileType = Terraria.WorldGen.TileType(source.X, source.Y);
					// TODO: Not sure why this is necessary; it seems that TileEmpty can return false even when the tile is inactive.
					if (tileType == -1) {
						continue;
					}
					Terraria.WorldGen.PlaceTile(dest.X, dest.Y, tileType);
					foreach (var direction in directions) {
						Point newTile = dest + direction;
						tiles.Enqueue((dest, newTile, d - 1));
					}
				}
			}

			Utils.GlobalPlayer.isBuilder = false;

		}

	}
}
