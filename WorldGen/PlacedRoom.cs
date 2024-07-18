using System.Collections.Generic;
using System.Linq;

namespace TerrariaCells.WorldGen;

internal class PlacedRoom
{
    public readonly Room Room;
    public Point Position;
    public readonly List<PlacedConnection> ExposedConnections;
    public bool IsSpawnRoom = false;
    public readonly List<PlacedConnection> OpenConnections = [];

    public PlacedRoom(Room room, Point position, RoomConnection coveredConnection = null)
    {
        this.Room = room;
        this.Position = position;

        this.ExposedConnections = (from conn in room.Connections
            where conn != coveredConnection
            select new PlacedConnection(this, conn)).ToList();
    }

    public List<Rectangle> GetRectangles()
    {
        var roomRect = new Rectangle(this.Position.X, this.Position.Y, this.Room.Width, this.Room.Height);

        if (this.Room.IsSurface)
        {
            roomRect.Y = -(2 << 16);
            roomRect.Height += (2 << 16) + this.Position.Y;
        }

        List<Rectangle> rects = [roomRect];

        foreach (var conn in this.ExposedConnections)
        {
            var clearanceRect = conn.GetClearanceRect();

            // TODO: This is a hack which extends the clearance of surface connections to (effectively) the top
            //       of the world, like what is done for the room rectangles.
            if (this.Room.IsSurface && conn.Connection.Length == 1)
            {
                clearanceRect.Height = clearanceRect.Y + (2 << 16);
                clearanceRect.Y = -(2 << 16);
            }

            rects.Add(clearanceRect);
        }

        return rects;
    }

    public bool Intersects(PlacedRoom other)
    {
        var thisRects = this.GetRectangles();
        var otherRects = other.GetRectangles();

        return (from rectA in thisRects from rectB in otherRects where rectA.Intersects(rectB) select new { }).Any();
    }
}