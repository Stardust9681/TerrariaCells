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
		#region Timer and Phase
		//Packing Timer and Phase data into one variable
		//Timer goes up to 18 minutes (ushort.MaxValue ticks), surely we don't need more than that
		//There's more than enough space for phases
		//This frees up at least one slot in the ai[] array for other use

		public static void Timer(this NPC npc, int value)
			=> npc.ai[0] = Pack((ushort)value, Unpack(npc.ai[0]).Item2);
		public static ushort Timer(this NPC npc)
			=> Unpack(npc.ai[0]).Item1;
		public static void DoTimer(this NPC npc, int inc = 1)
			=> npc.Timer(npc.Timer() + inc);

		public static void Phase(this NPC npc, int value)
			=> npc.ai[0] = value; //Roughly equivalent to 'Pack(0, (short)value);' which resets Timer upon phase change
		public static ushort Phase(this NPC npc)
			=> Unpack(npc.ai[0]).Item2;

		internal static int Pack(ushort a, ushort b) => ((ushort)a << 16) | (ushort)b;
		internal static int Pack(Terraria.DataStructures.Point16 point) => ((ushort)point.X << 16) | (ushort)point.Y;
		internal static (ushort, ushort) Unpack(float value) => Unpack((int)value);
		internal static (ushort, ushort) Unpack(int value) => ((ushort)((value >> 16) & ushort.MaxValue), (ushort)(value & ushort.MaxValue));
		#endregion

		///<returns>True if NPC "can see" a given position. False otherwise.</returns>
		public static bool LineOfSight(this NPC npc, Vector2 toPosition)
			=> Collision.CanHitLine(npc.position + new Vector2(npc.width * 0.5f, npc.height * 0.25f), 4, 4, toPosition, 4, 4);
		
		///<returns>True if NPC is determined to be on ground. False otherwise.</returns>
		public static bool Grounded(this NPC npc)
			=> npc.collideY && npc.oldVelocity.Y > npc.velocity.Y && npc.oldVelocity.Y > 0;

		//Relocated and refactored from Fighters.cs
		///<returns>False if NPC has no target. True if NPC direction is towards target</returns>
		public static bool IsFacingTarget(this NPC npc)
		{
			if (!npc.HasValidTarget) return false;
			if (npc.SupportsNPCTargets && npc.HasNPCTarget) return npc.direction == MathF.Sign(Main.npc[npc.TranslatedTargetIndex].position.X - npc.position.X);
			if (npc.HasPlayerTarget) return npc.direction == MathF.Sign(Main.player[npc.target].position.X - npc.position.X);
			return false;
		}
		///<returns>True if NPC direction is towards target. False otherwise</returns>
		public static bool IsFacingTarget(this NPC npc, Player target)
			=> npc.direction == MathF.Sign(target.position.X - npc.position.X);
	}
}
