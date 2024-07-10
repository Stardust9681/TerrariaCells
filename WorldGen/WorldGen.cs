using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TerrariaCells.WorldGen {
    class WorldGen : ModSystem {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            // TODO: Disable previous tasks.
            tasks.Add(new GenerateRoomsPass());
        }
    }

    class GenerateRoomsPass : GenPass {
        
        private struct RoomRect(int x, int y, int index) {
            public int x = x;
            public int y = y;
            public int width = Room.Rooms[index].Width;
            public int height = Room.Rooms[index].Height;
            public int roomIndex = index;
        }

        private struct Connection {
            public int x;
            public int y;
            public int length;
            public RoomConnectionSide side;
        }

        private struct RoomGenState {
            public int x;
            public int y;
            public int roomIndex;
            public List<Connection> connections;
        }

        public GenerateRoomsPass() : base("Generate Rooms", 1.0) {}

        private static void PushRoomToStack(Stack<RoomGenState> stack, int x, int y, int index, int? entranceConnIndex = null) {

            var room = Room.Rooms[index];

            List<Connection> connections = [];
            int connIndex = 0;
            foreach (var connection in room.Connections) {

                if (entranceConnIndex.HasValue && entranceConnIndex.Value == connIndex) {
                    continue;
                }

                int connX;
                int connY;

                switch (connection.side) {
                    case RoomConnectionSide.Left:
                        connX = x;
                        connY = y + connection.offset;
                        break;
                    case RoomConnectionSide.Right:
                        connX = x + room.Width;
                        connY = y + connection.offset;
                        break;
                    case RoomConnectionSide.Top:
                        connX = x + connection.offset;
                        connY = y;
                        break;
                    case RoomConnectionSide.Bottom:
                        connX = x + connection.offset;
                        connY = y + room.Height;
                        break;
                    default:
                        throw new Exception("invalid room connection side");
                }

                connections.Add(new Connection {
                    x = connX,
                    y = connY,
                    length = connection.length,
                    side = connection.side,
                });
            }

            stack.Push(new RoomGenState {
                x = x,
                y = y,
                roomIndex = index,
                connections = connections
            });
        }

        private static void PositionRoomByConnection(Room room, RoomConnection connection, int connX, int connY, out int x, out int y) {
            switch (connection.side) {
                case RoomConnectionSide.Left:
                    x = connX;
                    y = connY - connection.offset;
                    break;
                case RoomConnectionSide.Right:
                    x = connX - room.Width;
                    y = connY - connection.offset;
                    break;
                case RoomConnectionSide.Top:
                    x = connX - connection.offset;
                    y = connY;
                    break;
                case RoomConnectionSide.Bottom:
                    x = connX - connection.offset;
                    y = connY - room.Height;
                    break;
                default:
                    throw new Exception("invalid room connection side");
            }
        }

        private static bool IsRoomPositionValid(List<RoomRect> roomRects, int x, int y, int width, int height) {
            // TODO: Also ensure that connections of other rooms are not blocked.
            foreach (var room in roomRects) {
                bool separateX = (x + width <= room.x) || (x >= room.x + room.width);
                bool separateY = (y + height <= room.y) || (y >= room.y + room.height);

                if (!separateX && !separateY) {
                    return false;
                }
            }

            return true;
        }

        private struct ValidRoomPosition {
            public int x;
            public int y;
            public int roomIndex;
            public int connectionIndex;
        }

        private static List<ValidRoomPosition> GetValidRoomPositionList(List<RoomRect> roomRects, Connection connection) {

            List<ValidRoomPosition> validRooms = [];

            int roomIndex = 0;
            foreach (var room in Room.Rooms) {
                int connectionIndex = 0;
                foreach (var otherConnection in room.Connections) {
                    if (connection.length == otherConnection.length && connection.side == otherConnection.side.Opposite()) {

                        PositionRoomByConnection(room, otherConnection, connection.x, connection.y, out int x, out int y);

                        if (IsRoomPositionValid(roomRects, x, y, room.Width, room.Height)) {
                            validRooms.Add(new ValidRoomPosition {
                                x = x,
                                y = y,
                                roomIndex = roomIndex,
                                connectionIndex = connectionIndex
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

            var logger = ModContent.GetInstance<TerrariaCells>().Logger;

            progress.Message = "Generating Rooms";

            var rand = Terraria.WorldGen.genRand;

            List<RoomRect> rooms = [];

            var genStates = new Stack<RoomGenState>();

            // Push the starting room to the stack.
            var x = Main.maxTilesX / 2;
            var y = Main.maxTilesY / 2;
            var starterRoomIndex = rand.Next(0, Room.RoomNames.Length);
            PushRoomToStack(genStates, x, y, starterRoomIndex);
            rooms.Add(new RoomRect { x = x, y = y, roomIndex = starterRoomIndex });

            while (genStates.Count > 0) {

                var state = genStates.Peek();

                if (state.connections.Count > 0 && genStates.Count < 20) { // arbitrary maximum depth for now

                    var connection = state.connections[^1];
                    state.connections.RemoveAt(state.connections.Count - 1);

                    var validRoomPositions = GetValidRoomPositionList(rooms, connection);

                    if (validRoomPositions.Count > 0) {

                        var chosenIndex = rand.Next(0, validRoomPositions.Count);
                        var roomPos = validRoomPositions[chosenIndex];

                        PushRoomToStack(genStates, roomPos.x, roomPos.y, roomPos.roomIndex);
                        rooms.Add(new RoomRect(roomPos.x, roomPos.y, roomPos.roomIndex));

                    }
                    
                } else {
                    genStates.Pop();
                }
                
            }

            foreach (var roomRect in rooms) {

                var room = Room.Rooms[roomRect.roomIndex];

                StructureHelper.Generator.Generate(room.Tag, new Terraria.DataStructures.Point16(roomRect.x, roomRect.y));
            }
        }
    }
}
