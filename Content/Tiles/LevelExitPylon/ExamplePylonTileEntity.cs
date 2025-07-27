using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Content.Tiles.LevelExitPylon;

public class ForestExitPylonTileEntity : ModTileEntity
{
    public string Destination = "Inn";
    public bool editing = false;

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        //The MyTile class is shown later
        return tile.HasTile && tile.TileType == ModContent.TileType<ForestExitPylon>();
    }

    public override int Hook_AfterPlacement(
        int i,
        int j,
        int type,
        int style,
        int direction,
        int alternate
    )
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
            int width = 2;
            int height = 2;
            NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

            // Sync the placement of the tile entity with other clients
            // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
            NetMessage.SendData(
                MessageID.TileEntityPlacement,
                number: i,
                number2: j,
                number3: Type
            );
            return -1;
        }

        // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
        int placedEntity = Place(i, j);
        return placedEntity;
    }

    public override void OnNetPlace()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(
                MessageID.TileEntitySharing,
                number: ID,
                number2: Position.X,
                number3: Position.Y
            );
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Destination);
    }
    public override void NetReceive(BinaryReader reader)
    {
        Destination = reader.ReadString();
    }

    public override void Update()
    {
        if (!editing)
        {
            return;
        }

        Sign sign = Main.sign[0];
        if (sign == null)
        {
            editing = false;
            return;
        }

        if (!Main.editSign)
        {
            editing = false;
            return;
        }

        // Main.npcChatText = sign.text;
        Destination = Main.npcChatText;
    }

    public override void SaveData(TagCompound tag)
    {
        if (Destination != "Inn")
        {
            tag["destination"] = Destination;
        }
    }

    public override void LoadData(TagCompound tag)
    {
        tag.TryGet("destination", out Destination);
    }
}
