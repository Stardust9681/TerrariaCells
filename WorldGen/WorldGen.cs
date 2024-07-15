using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.IO;
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
		}

		private class StackFrame(RoomList roomList, PlacedRoom room) {
			public RoomList RoomList = roomList;
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

			var roomLists = new List<RoomList>();
			var stack = new Stack<StackFrame>();

			var starterRoomList = new RoomList();
			roomLists.Add(starterRoomList);
			var starterRoomIndex = rand.Next(0, Room.Rooms.Count);
			var starterRoom = new PlacedRoom(Room.Rooms[starterRoomIndex], new Point(0, 0)) {
				IsSpawnRoom = true
			};
			starterRoomList.Rooms.Add(starterRoom);

			var starterStackFrame = new StackFrame(starterRoomList, starterRoom);

			stack.Push(starterStackFrame);

			while (stack.Count > 0) {

				var stackFrame = stack.Peek();

				if (stackFrame.Room.ExposedConnections.Count > 0 && stack.Count < 20) {

					var connection = stackFrame.Room.ExposedConnections[^1];
					stackFrame.Room.ExposedConnections.RemoveAt(stackFrame.Room.ExposedConnections.Count - 1);

					var validRooms = GetValidRoomList(stackFrame.RoomList, connection);

					if (validRooms.Count > 0) {

						validRooms.Sort((roomA, roomB) => roomA.Room.Connections.Count.CompareTo(roomB.Room.Connections.Count));

						// weighted random function explanation: last number has weight 1, next last number 3, then 5, 7, 9, etc until most weight is the len of the rooms * 2 + 1
						// sorted by amount of connections
						// will probably change later
						// ask @lunispang for explanation if confused
						int roomCount = validRooms.Count;
						int chosenIndex = (int)Math.Sqrt(rand.Next(0, roomCount * roomCount));

						var room = validRooms[chosenIndex];

						stackFrame.RoomList.Rooms.Add(room);
						stack.Push(new StackFrame(stackFrame.RoomList, room));

					}

				} else {
					stack.Pop();
				}
			}

			// Set world surface height.
			Main.worldSurface = Main.maxTilesY * 0.17; // TODO: This is just temporary to silence some errors.

			foreach (var roomList in roomLists) {

				// TODO: Right now this just places all room lists in the middle of the world.
				//       We'll need to find a way to allow for multiple room lists to be laid out (fun).

				int x = Main.maxTilesX / 2;
				int y = Main.maxTilesY / 2;

				foreach (var room in roomList.Rooms) {
					StructureHelper.Generator.Generate(room.Room.Tag, new Terraria.DataStructures.Point16(room.Position + new Point(x, y)));

					if (room.IsSpawnRoom) {
						Main.spawnTileX = x + 2;
						Main.spawnTileY = y + 3;
					}

					// TODO: This is a hack to make PlaceFiller work.
					room.Position += new Point(x, y);
				}


			}



			PlaceFiller(roomLists[0]);

		}

		private class FillColumnRange(int left, int right, int height) {
			public int Left = left;
			public int Right = right;
			public int Height = height;
		}

		private static void PlaceFiller(RoomList roomList) {

			// TODO: The contents of this loop can (and probably should) be moved into its own function.
			foreach (var room in roomList.Rooms) {

				var columnY = room.Position.Y + room.Room.Height;

				var columnRanges = new List<FillColumnRange> {
					new(room.Position.X, room.Position.X + room.Room.Width, 2 << 16)
				};

				foreach (var roomBelow in roomList.Rooms) {

					var addColumnRanges = new List<FillColumnRange>();

					foreach (var columnRange in columnRanges) {

						var roomLeft = roomBelow.Position.X;
						var roomRight = roomBelow.Position.X + roomBelow.Room.Width;

						var inVerticalRange = roomBelow.Position.Y + roomBelow.Room.Height > columnY && roomBelow.Position.Y < columnY + columnRange.Height;

						if (inVerticalRange && roomLeft <= columnRange.Right && roomRight >= columnRange.Left) {
							// Full or partial overlap; shorten overlapping range.

							var oldHeight = columnRange.Height;

							columnRange.Height = roomBelow.Position.Y - columnY;

							// Partial overlap on the right.
							if (roomLeft > columnRange.Left) {
								addColumnRanges.Add(new FillColumnRange(columnRange.Left, roomLeft, oldHeight));
								columnRange.Left = roomLeft;
							}

							// Partial overlap on the left.
							if (roomRight < columnRange.Right) {
								addColumnRanges.Add(new FillColumnRange(roomRight, columnRange.Right, oldHeight));
								columnRange.Right = roomRight;
							}
						}

					}

					columnRanges.AddRange(addColumnRanges);
				}

				foreach (var columnRange in columnRanges) {

					for (int y = columnY; y < Math.Min(columnY + columnRange.Height, Main.maxTilesY - 1); y++) {
						for (int x = columnRange.Left; x < columnRange.Right; x++) {
							Terraria.WorldGen.PlaceTile(x, y, 0);
						}
					}

				}

			}

		}
	}
}
