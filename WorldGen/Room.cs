using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.WorldGen
{
    internal enum RoomConnectionSide
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    internal class RoomConnection { }

    internal class Room
    {
        private static bool IsConnector(
            IList<TagCompound> data,
            int width,
            int height,
            int x,
            int y,
            Mod mod
        )
        {
            if (x < 0 || x >= width)
            {
                throw new IndexOutOfRangeException("x is out of range");
            }
            if (y < 0 || y >= height)
            {
                throw new IndexOutOfRangeException("y is out of range");
            }

            int index = x * height + y;
            var tileTag = data[index];
            return (
                    int.TryParse(tileTag.GetString("Tile"), out int tileId)
                    && tileId == TileID.TeamBlockRed
                )
                || (
                    tileTag.GetString("Tile")
                    == mod.Name
                        + " "
                        + ModContent.GetModTile(ModContent.TileType<RoomConnectorTile>()).Name
                );
        }

        private static void ClearTile(IList<TagCompound> data, int width, int height, int x, int y)
        {
            if (x < 0 || x >= width)
            {
                throw new IndexOutOfRangeException("x is out of range");
            }
            if (y < 0 || y >= height)
            {
                throw new IndexOutOfRangeException("y is out of range");
            }

            int index = x * height + y;
            var tileTag = data[index];

            tileTag.Set("Tile", "0", true);

            var wallWireData = tileTag.GetInt("WallWireData");
            wallWireData &= ~0b1;
            tileTag.Set("WallWireData", wallWireData, true);
        }

        public static void CheckForConnections()
        {
            // Check for connections at the bottom.
            // x = 0;
            // while (x < width)
            // {
            //     if (IsConnector(data, width, height, x, height - 1, mod))
            //     {
            //         ClearTile(data, width, height, x, height - 1);

            //         int end = x + 1;
            //         while (end < width)
            //         {
            //             if (!IsConnector(data, width, height, end, height - 1, mod))
            //             {
            //                 break;
            //             }
            //             ClearTile(data, width, height, end, height - 1);
            //             end++;
            //         }

            //         var connection = new RoomConnection(
            //             this,
            //             RoomConnectionSide.Bottom,
            //             x,
            //             end - x
            //         );
            //         this.Connections.Add(connection);

            //         x = end;
            //     }
            //     else
            //     {
            //         x++;
            //     }
            // }

            // // Check for connections at the left.
            // int y = 0;
            // while (y < height)
            // {
            //     if (IsConnector(data, width, height, 0, y, mod))
            //     {
            //         ClearTile(data, width, height, 0, y);

            //         int end = y + 1;
            //         while (end < height)
            //         {
            //             if (!IsConnector(data, width, height, 0, end, mod))
            //             {
            //                 break;
            //             }
            //             ClearTile(data, width, height, 0, end);
            //             end++;
            //         }

            //         var connection = new RoomConnection(this, RoomConnectionSide.Left, y, end - y);
            //         this.Connections.Add(connection);

            //         y = end;
            //     }
            //     else
            //     {
            //         y++;
            //     }
            // }

            // // Check for connections at the right.
            // y = 0;
            // while (y < height)
            // {
            //     if (IsConnector(data, width, height, width - 1, y, mod))
            //     {
            //         ClearTile(data, width, height, width - 1, y);

            //         int end = y + 1;
            //         while (end < height)
            //         {
            //             if (!IsConnector(data, width, height, width - 1, end, mod))
            //             {
            //                 break;
            //             }
            //             ClearTile(data, width, height, width - 1, end);
            //             end++;
            //         }

            //         var connection = new RoomConnection(this, RoomConnectionSide.Right, y, end - y);
            //         this.Connections.Add(connection);

            //         y = end;
            //     }
            //     else
            //     {
            //         y++;
            //     }
            // }
        }
    }
}
