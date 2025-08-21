using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Common.GlobalTiles
{
    public class LarvaBreak : GlobalTile
    {
        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            //player camera effect to lock onto arena center
            
            if (type == TileID.Larva && Main.netMode != NetmodeID.Server && new Vector2(i*16, j*16).Distance(Main.LocalPlayer.Center) < 300)
            {
                //only center based on the top left corner of the larva
                
                if (Main.tile[i - 1, j].TileType != TileID.Larva && Main.tile[i, j - 2].TileType != TileID.Larva)
                {
                    Common.GlobalNPCs.NPCTypes.Hive.QueenBee.SpawnPosition = new Vector2(i + 1.5f, j + 2)*16;
                    Main.LocalPlayer.GetModPlayer<QueenBeeCamera>().NearQueenBee = true;
                }
            }
            base.NearbyEffects(i, j, type, closer);
        }

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            //so the server knows where the center of the arena is for queen bee's ai
            if (type == TileID.Larva && Main.netMode == NetmodeID.Server &&
                Main.tile[i - 1, j].TileType != TileID.Larva && Main.tile[i, j - 2].TileType != TileID.Larva)
            {
                Common.GlobalNPCs.NPCTypes.Hive.QueenBee.SpawnPosition = new Vector2(i + 2, j + 2)*16;
            }
        }
    }
}
