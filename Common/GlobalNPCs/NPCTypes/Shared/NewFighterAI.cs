using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters
    {
        //general updates
        public void Update(NPC npc)
        {
            //get a target. return false if theres no target
            npc.TargetClosest();
            //allow jumping again if on the floor
            if (npc.collideY && WorldGen.SolidTile2(Main.tile[npc.Bottom.ToTileCoordinates()]))
            {
                npc.ai[0] = 0;
            }
            ShouldWalk = true;
        }

        //but heres the walker
        public void Walk(NPC npc, float maxSpeed, float accel)
        {
            //make sure npc is real
            if (npc == null || !npc.active) return;
            Player target = null;
            if (npc.HasValidTarget)
            {
                target = Main.player[npc.target];
            }
            //npc will continue in the direction its facing if theres no target
            int direction = npc.direction;
            if (target != null) direction = target.Center.X > npc.Center.X ? 1 : -1;
            //accelerate in the direction of the target
            //accelerate faster if moving the wrong way

            if ( direction == 1 && npc.velocity.X < maxSpeed)
            {
                npc.velocity.X += accel;
                if (npc.velocity.X < 0) npc.velocity.X += accel * 2;
            }
            else if (npc.velocity.X > -maxSpeed)
            {
                npc.velocity.X -= accel;
                if (npc.velocity.X > 0) npc.velocity.X -= accel * 2;
            }
            //get the tile in front of the enemies feet
            Vector2 pos = npc.BottomRight + new Vector2(1, -1);
            if (npc.direction == -1) pos = npc.BottomLeft + new Vector2(-1, -1);
            Tile infront = Main.tile[pos.ToTileCoordinates()];
            //move up a tile if theres only one tile in front of the enemies feet
            if (infront != null && WorldGen.SolidOrSlopedTile(infront) && npc.collideX && npc.velocity.Y == 0)
            {
                bool enoughSpace = true;
                for (int i = 1; i < npc.height / 16 + 1; i++)
                {
                    if (WorldGen.SolidOrSlopedTile(Main.tile[pos.ToTileCoordinates() + new Point(0, -i)])){
                        enoughSpace = false;
                    }
                }
                if (enoughSpace)
                {
                    //account for not being perfectly aligned and for half blocks
                    npc.position.Y = pos.ToTileCoordinates().ToWorldCoordinates().Y - npc.height - 8;
                    if (infront.IsHalfBlock)
                        npc.position.Y += 8;
                    //dont lose momentum
                    npc.velocity = npc.oldVelocity;
                }
            }
            //jump if hugging a wall
            if (npc.collideX) {
                //check for tile preventing moving
                bool anyTileBlocking = false;
                for (int i = 1; i < npc.height / 16 + 1; i++)
                {
                    if (WorldGen.SolidOrSlopedTile(Main.tile[pos.ToTileCoordinates() + new Point(0, -i)]))
                    {
                        anyTileBlocking = true;
                    }
                }
                //jump after 30 ticks of hugging a wall
                if (anyTileBlocking)
                {
                    npc.ai[1]++;
                    if (npc.ai[1] >= 5)
                    {
                        Jump(npc, JumpSpeed);
                    }
                }
            }
            //jump if target is high up for >2 seconds
            else if (target != null && target.Bottom.Y < npc.Top.Y)
            {
                if (npc.ai[1] < 120) npc.ai[1]++;
                if (npc.ai[1] >= 120)
                {
                    
                    Jump(npc, JumpSpeed);
                }
            }
            else
            {
                npc.ai[1] = 0;
            }


        }
        //but heres the jumper
        public void Jump(NPC npc, float jumpSpeed)
        {
            if (npc.collideY && npc.ai[0] == 0)
            {
                JustJumped = true;
                npc.ai[1] = 0;
                npc.ai[0] = 1;
                npc.velocity.Y = -jumpSpeed;
                
            }
        }
    }
}
