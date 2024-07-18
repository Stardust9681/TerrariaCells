using System;

namespace TerrariaCells.WorldGen;

internal class PlacedConnection
{
    private const int ConnectorFrontClearance = 9;
    private const int ConnectorSideClearance = 2;

    public readonly Point Position;
    public readonly PlacedRoom PlacedRoom;
    public readonly RoomConnection Connection;

    public PlacedConnection(PlacedRoom room, RoomConnection connection)
    {
        var offset = connection.Side switch
        {
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

    public Rectangle GetClearanceRect()
    {
        return this.Connection.Side switch
        {
            RoomConnectionSide.Left => new Rectangle(this.Position.X - ConnectorFrontClearance,
                this.Position.Y - ConnectorSideClearance, ConnectorFrontClearance,
                ConnectorSideClearance * 2 + this.Connection.Length),
            RoomConnectionSide.Right => new Rectangle(this.Position.X, this.Position.Y - ConnectorSideClearance,
                ConnectorFrontClearance, ConnectorSideClearance * 2 + this.Connection.Length),
            RoomConnectionSide.Top => new Rectangle(this.Position.X - ConnectorSideClearance,
                this.Position.Y - ConnectorFrontClearance, ConnectorSideClearance * 2 + this.Connection.Length,
                ConnectorFrontClearance),
            RoomConnectionSide.Bottom => new Rectangle(this.Position.X - ConnectorSideClearance, this.Position.Y,
                ConnectorSideClearance * 2 + this.Connection.Length, ConnectorFrontClearance),
            _ => throw new Exception("invalid room connection side"),
        };
    }
}