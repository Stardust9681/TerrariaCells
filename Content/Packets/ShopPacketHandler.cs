using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    internal class ShopPacketHandler() : PacketHandler(TCPacketType.ShopPacket)
    {
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            switch (Main.netMode)
            {
                case NetmodeID.Server:
                    HandleServerPacket(mod, reader, fromWho);
                    break;
                case NetmodeID.MultiplayerClient:
                    HandleClientPacket(mod, reader, fromWho);
                    break;
            }
        }
        void HandleServerPacket(Mod mod, BinaryReader reader, int fromWho)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.TryGetGlobalNPC<Common.GlobalNPCs.VanillaNPCShop>(out var shopNPC))
                {
                    shopNPC.NetSendShop(npc, ModNetHandler.GetPacket(mod, TCPacketType.ShopPacket), fromWho);
                }
            }
        }
        void HandleClientPacket(Mod mod, BinaryReader reader, int fromWho)
        {
            byte npcWhoAmI = reader.ReadByte();
            NPC npc = Main.npc[npcWhoAmI];
            if (npc.TryGetGlobalNPC<Common.GlobalNPCs.VanillaNPCShop>(out var shopNPC))
                shopNPC.NetReceiveShop(npc, reader);
        }
    }
}