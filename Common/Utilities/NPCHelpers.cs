using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TerrariaCells.Common.Utilities
{
	public static class NPCHelpers
	{
		public static int Timer(this NPC npc, int? value = null)
		{
			return (int)(npc.ai[0] = value ?? (int)npc.ai[0]);
		}
		public static int Phase(this NPC npc, int? value = null)
		{
			if (value is null)
			{
				return (int)npc.ai[1];
			}
			npc.ai[1] = value.Value;
			npc.Timer(0); //Switching to new phase, reset phase timer
			return (int)npc.ai[1];
		}

		///<returns>True if NPC "can see" a given position. False otherwise.</returns>
		public static bool LineOfSight(this NPC npc, Vector2 toPosition)
		{
			return Collision.CanHitLine(npc.position + new Vector2(npc.width * 0.5f, npc.height * 0.25f), 4, 4, toPosition, 4, 4);
		}

		///<remarks> Shorthand for <c>npc.collideY</c> and <c>npc.oldVel.Y > npc.vel.Y</c> and <c>npc.oldVel.Y > 0</c> </remarks>
		///<returns>True if NPC is determined to be on ground. False otherwise.</returns>
		public static bool Grounded(this NPC npc) => npc.collideY && npc.oldVelocity.Y > npc.velocity.Y && npc.oldVelocity.Y > 0;
	}
}
