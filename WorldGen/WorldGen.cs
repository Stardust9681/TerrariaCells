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

		const int ConnectorFrontClearance = 7;
		const int ConnectorSideClearance = 2;
		
		private class RoomRect(Point position, int roomIndex) {
			public readonly Rectangle Rect = new(
				position.X, position.Y,
				Room.Rooms[roomIndex].Width, Room.Rooms[roomIndex].Height
			);
			public readonly int RoomIndex = roomIndex;
		}

		/// <summary>
		/// Represents a connection along with an absolute position in world space.
		/// </summary>
		private readonly struct PosConnection {
			public readonly Point Position;
			public readonly RoomConnection Connection;

			public PosConnection(Point position, Room room, RoomConnection connection) {

				var offset = connection.Side switch {
					RoomConnectionSide.Left => new Point(0, connection.Offset),
					RoomConnectionSide.Right => new Point(room.Width, connection.Offset),
					RoomConnectionSide.Top => new Point(connection.Offset, 0),
					RoomConnectionSide.Bottom => new Point(connection.Offset, room.Height),
					_ => throw new Exception("invalid room connection side"),
				};

				this.Position = position + offset;
				this.Connection = connection;
			}

		}

		private struct RoomGenState {
			public int RoomIndex;
			public List<PosConnection> Connections;
		}

		private struct ValidRoomPosition {
			public Point Position;
			public int RoomIndex;
			public int ConnectionIndex;
		}

		public GenerateRoomsPass() : base("Generate Rooms", 1.0) {}

		private static void PushRoomToQueue(Queue<RoomGenState> queue, Point position, int index, int connectionIndex = -1) {

			var room = Room.Rooms[index];

			List<PosConnection> connections = [];
			int i = 0;
			foreach (var connection in room.Connections) {
				if(i!=connectionIndex) connections.Add(new PosConnection(position, room, connection));
				i++;
			}

			queue.Enqueue(new RoomGenState {
				RoomIndex = index,
				Connections = connections
			});
		}

		private static Point PositionRoomByConnection(Room room, RoomConnection connection, Point connPosition) {
			return connection.Side switch {
				RoomConnectionSide.Left => new Point(connPosition.X, connPosition.Y - connection.Offset),
				RoomConnectionSide.Right => new Point(connPosition.X - room.Width, connPosition.Y - connection.Offset),
				RoomConnectionSide.Top => new Point(connPosition.X - connection.Offset, connPosition.Y),
				RoomConnectionSide.Bottom => new Point(connPosition.X - connection.Offset, connPosition.Y - room.Height),
				_ => throw new Exception("invalid room connection side"),
			};
		}

		private static Rectangle GetConnectionClearanceRect(PosConnection connection) {
			return connection.Connection.Side switch {
				RoomConnectionSide.Left => new Rectangle(
					connection.Position.X - ConnectorFrontClearance,
					connection.Position.Y - ConnectorSideClearance,
					ConnectorFrontClearance,
					ConnectorSideClearance * 2 + connection.Connection.Length
				),
				RoomConnectionSide.Right => new Rectangle(
					connection.Position.X,
					connection.Position.Y - ConnectorSideClearance,
					ConnectorFrontClearance,
					ConnectorSideClearance * 2 + connection.Connection.Length
				),
				RoomConnectionSide.Top => new Rectangle(
					connection.Position.X - ConnectorSideClearance,
					connection.Position.Y - ConnectorFrontClearance,
					ConnectorSideClearance * 2 + connection.Connection.Length,
					ConnectorFrontClearance
				),
				RoomConnectionSide.Bottom => new Rectangle(
					connection.Position.X - ConnectorSideClearance,
					connection.Position.Y,
					ConnectorSideClearance * 2 + connection.Connection.Length,
					ConnectorFrontClearance
				),
				_ => throw new Exception("invalid room connection side"),
			};
		}

		private static bool IsRoomPositionValid(List<RoomRect> roomRects, Point placingRoomPos, Room placingRoom, PosConnection ignoreConnection, RoomConnection placingIgnoreConnection) {
			var placingRoomRect = new Rectangle(placingRoomPos.X, placingRoomPos.Y, placingRoom.Width, placingRoom.Height);

			var placingConnectionClearances = new List<Rectangle>();
			foreach (var placingConnection in placingRoom.Connections) {
				if (placingConnection != placingIgnoreConnection) {
					var posConnection = new PosConnection(placingRoomPos, placingRoom, placingConnection);
					placingConnectionClearances.Add(GetConnectionClearanceRect(posConnection));
				}
			}

			foreach (var room in roomRects) {
				if (placingRoomRect.Intersects(room.Rect)) {
					return false;
				}

				foreach (var placingConnectionRect in placingConnectionClearances) {
					if (placingConnectionRect.Intersects(room.Rect)) {
						return false;
					}
				}

				foreach (var connection in Room.Rooms[room.RoomIndex].Connections) {

					var posConnection = new PosConnection(room.Rect.Location, Room.Rooms[room.RoomIndex], connection);

					if (connection == ignoreConnection.Connection && ignoreConnection.Position == posConnection.Position) {
						continue;
					}

					var clearanceRect = GetConnectionClearanceRect(posConnection);

					if (placingRoomRect.Intersects(clearanceRect)) {
						return false;
					}

				}
			}

			return true;
		}

		private static List<ValidRoomPosition> GetValidRoomPositionList(List<RoomRect> roomRects, PosConnection connection) {

			List<ValidRoomPosition> validRooms = [];

			int roomIndex = 0;
			foreach (var room in Room.Rooms) {
				int connectionIndex = 0;
				foreach (var otherConnection in room.Connections) {
					if (connection.Connection.Length == otherConnection.Length && connection.Connection.Side == otherConnection.Side.Opposite()) {

						var roomPos = PositionRoomByConnection(room, otherConnection, connection.Position);

						if (IsRoomPositionValid(roomRects, roomPos, room, connection, otherConnection)) {
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
			List<PosConnection> openConnections = [];

			var genStates = new Queue<RoomGenState>();

			// Push the starting room to the stack.
			var x = Main.maxTilesX / 2;
			var y = Main.maxTilesY / 2;
			var starterRoomIndex = rand.Next(0, Room.RoomNames.Length);
			PushRoomToQueue(genStates, new Point(x, y), starterRoomIndex);
			rooms.Add(new RoomRect(new Point(x, y), starterRoomIndex));

			// Set spawn point.
			Main.spawnTileX = x + 2;
			Main.spawnTileY = y + 3;

			// Set world surface height.
			Main.worldSurface = Main.maxTilesY * 0.17; // TODO: This is just temporary to silence some errors.

			while (genStates.Count > 0) {

				var state = genStates.Peek();

				if (state.Connections.Count > 0 && genStates.Count < 20) { // arbitrary maximum depth for now

					var connection = state.Connections[0];
					state.Connections.RemoveAt(0);

					var validRoomPositions = GetValidRoomPositionList(rooms, connection);

					if (validRoomPositions.Count > 0) {

						var sortedRoomPositions = validRoomPositions.OrderBy(roomPos=>Room.Rooms[roomPos.RoomIndex].Connections.Count()).ToList();
						//weighted random function explanation: no
						//sorted by amount of connections
						//ask @lunispang for explanation if confused
						int roomCount = validRoomPositions.Count;
						int chosenIndex = (int)Math.Sqrt(rand.Next(0, roomCount * roomCount)); 
						if (rooms.Count > 20) chosenIndex = roomCount - chosenIndex - 1; // reverse priority to have higher chance of selecting room with few connections
						var roomPos = sortedRoomPositions[chosenIndex];

						PushRoomToQueue(genStates, roomPos.Position, roomPos.RoomIndex, roomPos.ConnectionIndex);
						rooms.Add(new RoomRect(roomPos.Position, roomPos.RoomIndex));

					} else {
						// no valid rooms were found, resulting in open connection
						openConnections.Add(connection);	
					}

				} else {
					genStates.Dequeue();
				}
				
			}

			Utils.GlobalPlayer.isBuilder = true;

			foreach (var roomRect in rooms) {

				var room = Room.Rooms[roomRect.RoomIndex];

				StructureHelper.Generator.Generate(room.Tag, new Terraria.DataStructures.Point16(roomRect.Rect.Location));
			}
			foreach (var conn in openConnections) {
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
					Terraria.WorldGen.PlaceTile(position.X, position.Y, TileID.Obsidian);
					Terraria.WorldGen.KillWall(position.X, position.Y, false);
					switch (conn.Connection.Side) {
						case RoomConnectionSide.Top: case RoomConnectionSide.Bottom: { position.X++; break; }
						case RoomConnectionSide.Left: case RoomConnectionSide.Right: { position.Y++; break; }
					}
				}
			}

			Utils.GlobalPlayer.isBuilder = false;
		}
	}
}
