using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.Systems;

public class ClickedHeartsTracker : ModSystem
{
    internal List<(int, int)> collectedHearts = [];
    private List<(int, int)> resettingHearts = [];

    public override void OnWorldLoad()
    {
        Reset();
    }

    public override void PostUpdateWorld()
    {
        foreach (var (i, j) in resettingHearts)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileType != TileID.Heart)
            {
                continue;
            }
            tile.IsActuated = false;
            tile = Main.tile[i + 1, j];
            tile.IsActuated = false;
            tile = Main.tile[i, j + 1];
            tile.IsActuated = false;
            tile = Main.tile[i + 1, j + 1];
            tile.IsActuated = false;
        }
        resettingHearts.Clear();
    }

    public override void OnWorldUnload()
    {
        Reset();
    }

    public void Reset()
    {
        foreach ((int i, int j) in collectedHearts)
        {
            // the hearts are not reset here and now, since they cannot be unactuated at some times
            resettingHearts.Add((i, j));
        }
        collectedHearts.Clear();
        Main.LocalPlayer.GetModPlayer<LifeModPlayer>().extraHealth = 0;
    }

    public void ClickedHeart(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX == 18)
        {
            i -= 1;
        }
        if (tile.TileFrameY == 18)
        {
            j -= 1;
        }
        tile = Main.tile[i, j];

        (int, int) coords = (i, j);
        if (!collectedHearts.Contains(coords))
        {
            collectedHearts.Add(coords);
            Main.player[Main.myPlayer].GetModPlayer<LifeModPlayer>().IncreasePlayerHealth(20);
            tile = Main.tile[i, j];
            tile.IsActuated = true;
            tile = Main.tile[i + 1, j];
            tile.IsActuated = true;
            tile = Main.tile[i, j + 1];
            tile.IsActuated = true;
            tile = Main.tile[i + 1, j + 1];
            tile.IsActuated = true;

            // SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_4"));
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Shatter"));

            if (Main.netMode == 1)
            {
                var packet = ModNetHandler.GetPacket(Mod, TCPacketType.HeartPacket);
                packet.Write((byte)Content.Packets.HeartPacketHandler.HeartPacketType.ClientUse);
                packet.Write((ushort)i);
                packet.Write((ushort)j);
                packet.Send();
            }
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((ushort)collectedHearts.Count);
        for (int i = 0; i < collectedHearts.Count; i++)
        {
            writer.Write((ushort)collectedHearts[i].Item1);
            writer.Write((ushort)collectedHearts[i].Item2);
        }
    }
    public override void NetReceive(BinaryReader reader)
    {
        int count = (int)reader.ReadUInt16();
        List<(int, int)> collected = new List<(int, int)>();
        for (int i = 0; i < count; i++)
        {
            collected.Add(((int)reader.ReadUInt16(), (int)reader.ReadUInt16()));
            //Prefer Framing.GetTileSafely(..) but I'm tired and running on caffeine so /whatever/
            (int x, int y) = collected[i];
            Tile tile = Main.tile[x, y];
            tile.IsActuated = true;
            tile = Main.tile[x + 1, y];
            tile.IsActuated = true;
            tile = Main.tile[x, y + 1];
            tile.IsActuated = true;
            tile = Main.tile[x + 1, y + 1];
            tile.IsActuated = true;
        }
        collectedHearts = collected;
    }

    public override void PreUpdateWorld()
    {
        base.PreUpdateWorld();
    }
}
