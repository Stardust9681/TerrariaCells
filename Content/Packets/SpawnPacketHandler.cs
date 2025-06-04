using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Net;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells;
using TerrariaCells.Common.GlobalNPCs;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    internal class SpawnPacketHandler(TCPacketType handlerType) : PacketHandler(handlerType)
	{
        public override void HandlePacket(BinaryReader reader, int fromWho)
		{
			switch (reader.ReadByte())
			{
				// If someone requests to spawn all dead, this can only happen if everyone is dead
				case (byte)SpawnPacketType.SpawnDead:
				{
					// Spawn all players on the server, and then sync it to clients, this also checks if all players are dead
                    if (Main.netMode == NetmodeID.MultiplayerClient) return;
                    bool allDead = true;
                    foreach (Player player in Main.player) 
                    {
                        if (!player.active) continue;
                        if (!player.dead)
                        {
                            allDead = false; 
                            break;
                        }
                    }
                    if(!allDead) break;
                    foreach (Player player in Main.player)
                    {
						// Sync the spawn with clients
                        if (player.active)
						{
                            NetMessage.SendData(MessageID.PlayerSpawn, -1, -1, null, player.whoAmI);
						}
                    }
                    break;
				}
				// This force respawns all players, including living ones
				case (byte)SpawnPacketType.SpawnAll:
				{
					foreach (Player player in Main.player)
					{
						if (player.active)
						{
                            NetMessage.SendData(MessageID.PlayerSpawn, -1, -1, null, player.whoAmI);
						}
					}
					break;
				}
				// In case you want to spawn a specific player
				case (byte)SpawnPacketType.SpawnTarget:
				{
					NetMessage.SendData(MessageID.PlayerSpawn, -1, -1, null, reader.ReadByte());
					break;
				}
			}
		}
	}
	public enum SpawnPacketType : byte
	{
		SpawnDead,
		SpawnAll,
		SpawnTarget,
	}
}