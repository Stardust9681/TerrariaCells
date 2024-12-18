using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.WorldGen;

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
    public TagCompound Tag { get; private set; }
    public int Width { get; }
    public int Height { get; }
    public bool IsSurface { get; }

    public Room(string path, Mod mod)
    {
        using (var stream = mod.GetFileStream(path))
        {
            this.Tag = TagIO.FromStream(stream);
        }

        if (this.Tag == null)
        {
            throw new Exception($"unable to load structure file ${path}");
        }

        var data = this.Tag.GetList<TagCompound>("TileData");
        this.Width = this.Tag.GetInt("Width") + 1;
        this.Height = this.Tag.GetInt("Height") + 1;
        this.IsSurface = this.Tag.GetBool("Surface");

        // Check for connections at the top.
        int x = 0;
        while (x < Width)
        {
            if (IsConnector(data, Width, Height, x, 0, mod))
            {
                ClearTile(data, Width, Height, x, 0);

                int end = x + 1;
                while (end < Width)
                {
                    if (!IsConnector(data, Width, Height, end, 0, mod))
                    {
                        break;
                    }
                    ClearTile(data, Width, Height, end, 0);
                    end++;
                }

                var connection = new RoomConnection(this, RoomConnectionSide.Top, x, end - x);
                this.Connections.Add(connection);

                x = end;
            }
            else
            {
                x++;
            }
        }

        // Check for connections at the bottom.
        x = 0;
        while (x < Width)
        {
            if (IsConnector(data, Width, Height, x, Height - 1, mod))
            {
                ClearTile(data, Width, Height, x, Height - 1);

                int end = x + 1;
                while (end < Width)
                {
                    if (!IsConnector(data, Width, Height, end, Height - 1, mod))
                    {
                        break;
                    }
                    ClearTile(data, Width, Height, end, Height - 1);
                    end++;
                }

                var connection = new RoomConnection(this, RoomConnectionSide.Bottom, x, end - x);
                this.Connections.Add(connection);

                x = end;
            }
            else
            {
                x++;
            }
        }

        // Check for connections at the left.
        int y = 0;
        while (y < Height)
        {
            if (IsConnector(data, Width, Height, 0, y, mod))
            {
                ClearTile(data, Width, Height, 0, y);

                int end = y + 1;
                while (end < Height)
                {
                    if (!IsConnector(data, Width, Height, 0, end, mod))
                    {
                        break;
                    }
                    ClearTile(data, Width, Height, 0, end);
                    end++;
                }

                var connection = new RoomConnection(this, RoomConnectionSide.Left, y, end - y);
                this.Connections.Add(connection);

                y = end;
            }
            else
            {
                y++;
            }
        }

        // Check for connections at the right.
        y = 0;
        while (y < Height)
        {
            if (IsConnector(data, Width, Height, Width - 1, y, mod))
            {
                ClearTile(data, Width, Height, Width - 1, y);

                int end = y + 1;
                while (end < Height)
                {
                    if (!IsConnector(data, Width, Height, Width - 1, end, mod))
                    {
                        break;
                    }
                    ClearTile(data, Width, Height, Width - 1, end);
                    end++;
                }

                var connection = new RoomConnection(this, RoomConnectionSide.Right, y, end - y);
                this.Connections.Add(connection);

                y = end;
            }
            else
            {
                y++;
            }
        }
    }

    /// <summary>
    /// Finds all of the connectors along the side of a room.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="vertical"></param>
    /// <param name="roomLength"></param>
    /// <param name="offset"></param>
    public static List<RoomConnection> GetConnectorsForSide(
        IList<TagCompound> data,
        bool vertical,
        int roomLength,
        int offset = 0
    ) { 
        List<RoomConnection> list = new();

        x = 0;
        while (x < Width)
        {
            if (IsConnector(data, Width, Height, x, Height - 1, mod))
            {
                ClearTile(data, Width, Height, x, Height - 1);

                int end = x + 1;
                while (end < Width)
                {
                    if (!IsConnector(data, Width, Height, end, Height - 1, mod))
                    {
                        break;
                    }
                    ClearTile(data, Width, Height, end, Height - 1);
                    end++;
                }

                var connection = new RoomConnection(this, RoomConnectionSide.Bottom, x, end - x);
                this.Connections.Add(connection);

                x = end;
            }
            else
            {
                x++;
            }
        }

        return list
    }

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
