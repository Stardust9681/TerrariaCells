using System.Collections.Generic;

namespace TerrariaCells.WorldGen;

internal class RoomList
{
    public List<PlacedRoom> Rooms = [];

    public Rectangle GetBounds()
    {
        int left = 0;
        int right = 0;
        int top = 0;
        int bottom = 0;

        foreach (var room in this.Rooms)
        {
            if (room.Position.X < left)
            {
                left = room.Position.X;
            }

            if (room.Position.Y < top)
            {
                top = room.Position.Y;
            }

            if (room.Position.X + room.Room.Width > right)
            {
                right = room.Position.X + room.Room.Width;
            }

            if (room.Position.Y + room.Room.Height > bottom)
            {
                bottom = room.Position.Y + room.Room.Height;
            }
        }

        return new Rectangle(left, top, right - left, bottom - top);
    }

    public void Offset(int x, int y)
    {
        foreach (var room in this.Rooms)
        {
            room.Position += new Point(x, y);
        }
    }
}