using System;
using System.Collections.Generic;
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
		
		private readonly struct RoomRect(Point position, int roomIndex) {
			public readonly Rectangle Rect = new(
				position.X, position.Y,
				Room.Rooms[roomIndex].Width, Room.Rooms[roomIndex].Height
			);
			public readonly int RoomIndex = roomIndex;
		}

		private struct Connection {
			public Point Position;
			public int Length;
			public RoomConnectionSide Side;
		}

		private struct RoomGenState {
			public int RoomIndex;
			public List<Connection> Connections;
		}

		private struct ValidRoomPosition {
			public Point Position;
			public int RoomIndex;
			public int ConnectionIndex;
		}

		public GenerateRoomsPass() : base("Generate Rooms", 1.0) {}

		private static void PushRoomToStack(Stack<RoomGenState> stack, Point position, int index) {

			var room = Room.Rooms[index];

			List<Connection> connections = [];
			foreach (var connection in room.Connections) {

				var offset = connection.side switch {
					RoomConnectionSide.Left => new Point(0, connection.offset),
					RoomConnectionSide.Right => new Point(room.Width, connection.offset),
					RoomConnectionSide.Top => new Point(connection.offset, 0),
					RoomConnectionSide.Bottom => new Point(connection.offset, room.Height),
					_ => throw new Exception("invalid room connection side"),
				};

				connections.Add(new Connection {
					Position = position + offset,
					Length = connection.length,
					Side = connection.side,
				});
			}

			stack.Push(new RoomGenState {
				RoomIndex = index,
				Connections = connections
			});
		}

		private static Point PositionRoomByConnection(Room room, RoomConnection connection, Point connPosition) {
			return connection.side switch {
				RoomConnectionSide.Left => new Point(connPosition.X, connPosition.Y - connection.offset),
				RoomConnectionSide.Right => new Point(connPosition.X - room.Width, connPosition.Y - connection.offset),
				RoomConnectionSide.Top => new Point(connPosition.X - connection.offset, connPosition.Y),
				RoomConnectionSide.Bottom => new Point(connPosition.X - connection.offset, connPosition.Y - room.Height),
				_ => throw new Exception("invalid room connection side"),
			};
		}

		private static bool IsRoomPositionValid(List<RoomRect> roomRects, int x, int y, int width, int height) {
			var roomRect = new Rectangle(x, y, width, height);

			// TODO: Also ensure that connections of other rooms are not blocked.
			foreach (var room in roomRects) {
				if (roomRect.Intersects(room.Rect)) {
					return false;
				}
			}

			return true;
		}

		private static List<ValidRoomPosition> GetValidRoomPositionList(List<RoomRect> roomRects, Connection connection) {

			List<ValidRoomPosition> validRooms = [];

			int roomIndex = 0;
			foreach (var room in Room.Rooms) {
				int connectionIndex = 0;
				foreach (var otherConnection in room.Connections) {
					if (connection.Length == otherConnection.length && connection.Side == otherConnection.side.Opposite()) {

						var roomPos = PositionRoomByConnection(room, otherConnection, connection.Position);

						if (IsRoomPositionValid(roomRects, roomPos.X, roomPos.Y, room.Width, room.Height)) {
							validRooms.Add(new ValidRoomPosition {
								Position = roomPos,
								RoomIndex = roomIndex,
								ConnectionIndex = connectionIndex
							});
						}

					}
					connectionIndex++;
				}
				roomIndex++;
			}

			return validRooms;
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {

			progress.Message = "Generating Rooms";

			var rand = Terraria.WorldGen.genRand;

			List<RoomRect> rooms = [];

			var genStates = new Stack<RoomGenState>();

			// Push the starting room to the stack.
			var x = Main.maxTilesX / 2;
			var y = Main.maxTilesY / 2;
			var starterRoomIndex = rand.Next(0, Room.RoomNames.Length);
			PushRoomToStack(genStates, new Point(x, y), starterRoomIndex);
			rooms.Add(new RoomRect(new Point(x, y), starterRoomIndex));

			// Set spawn point.
			Main.spawnTileX = x + 2;
			Main.spawnTileY = y + 3;

			// Set world surface height.
			Main.worldSurface = Main.maxTilesY * 0.17; // TODO: This is just temporary to silence some errors.

			while (genStates.Count > 0) {

				var state = genStates.Peek();

				if (state.Connections.Count > 0 && genStates.Count < 20) { // arbitrary maximum depth for now

					var connection = state.Connections[^1];
					state.Connections.RemoveAt(state.Connections.Count - 1);

					var validRoomPositions = GetValidRoomPositionList(rooms, connection);

					if (validRoomPositions.Count > 0) {

						var chosenIndex = rand.Next(0, validRoomPositions.Count);
						var roomPos = validRoomPositions[chosenIndex];

						PushRoomToStack(genStates, roomPos.Position, roomPos.RoomIndex);
						rooms.Add(new RoomRect(roomPos.Position, roomPos.RoomIndex));

					}
					
				} else {
					genStates.Pop();
				}
				
			}

			foreach (var roomRect in rooms) {

				var room = Room.Rooms[roomRect.RoomIndex];

				StructureHelper.Generator.Generate(room.Tag, new Terraria.DataStructures.Point16(roomRect.Rect.Location));
			}
		}
	}
}
