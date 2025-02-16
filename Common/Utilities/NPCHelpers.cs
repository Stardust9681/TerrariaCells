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
		//Need to change these, @ me later, I don't feel like trying to compress this rn, why couldn't they just be ints dude
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

		public static float PositionToFloat(Vector2 pos)
		{
			return (uint)((uint)pos.X * (Main.maxTilesY * 16)) + (uint)pos.Y;
		}
		public static Vector2 FloatToPosition(float input)
		{
			Vector2 res;
			res.X = (uint)(input / (Main.maxTilesY * 16));
			res.Y = (uint)(input - (res.X * (Main.maxTilesY * 16)));
			return res;
		}

		///<returns>True if NPC "can see" a given position. False otherwise.</returns>
		public static bool LineOfSight(this NPC npc, Vector2 toPosition)
			=> Collision.CanHitLine(npc.position + new Vector2(npc.width * 0.5f - 8, npc.height * 0.25f - 8), 16, 16, toPosition, 16, 16);

		///<returns>True if NPC is determined to be on ground. False otherwise.</returns>
		public static bool Grounded(this NPC npc)
			=> npc.collideY && npc.oldVelocity.Y > npc.velocity.Y && npc.oldVelocity.Y > 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target">Will be null if none found</param>
		/// <returns>True if valid target exists</returns>
		public static bool TryGetTarget(this NPC npc, out Entity target)
		{
			target = null;
			if (npc.SupportsNPCTargets && npc.HasNPCTarget) return (target = Main.npc[npc.TranslatedTargetIndex]).active;
			else if (npc.HasPlayerTarget) return (target = Main.player[npc.target]).active && !Main.player[npc.target].dead && !Main.player[npc.target].ghost;
			return npc.HasValidTarget;
		}

		//Relocated and refactored from Fighters.cs
		///<returns>False if NPC has no target. True if NPC direction is towards target</returns>
		public static bool IsFacingTarget(this NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target)) return false;
			return npc.direction == MathF.Sign(target.position.X - npc.position.X);
		}
		///<returns>True if NPC direction is towards target. False otherwise</returns>
		public static bool IsFacingTarget(this NPC npc, Entity target)
			=> npc.direction == MathF.Sign(target.position.X - npc.position.X);

		public static Vector2 FindGroundInFront(this NPC npc, int tilesToCheck = 6)
		{
			Vector2 testPos = npc.position;
			testPos.X += npc.direction * npc.width;
			//Might be able to improve this by doing less checks, depending on how tall the NPC is
			for (int i = 0; i < tilesToCheck; i++)
			{
				Vector2 collision = Collision.TileCollision(testPos, Vector2.UnitY * 16, npc.width, npc.height);
				if (collision.Y != 16)
					return testPos  + collision;
			}
			return testPos + new Vector2(0, tilesToCheck * 16);
		}

		public static bool TargetInAggroRange(this NPC npc, float range = 240, bool lineOfSight = true, bool allowDamageTrigger = true)
		{
			if (!npc.TryGetTarget(out Entity target))
				return false;
			if (lineOfSight && !npc.LineOfSight(target.position))
				return false;
			if (allowDamageTrigger && npc.life < npc.lifeMax)
				return true;
			return npc.DistanceSQ(target.position) < MathF.Pow(range, 2);
		}

		public static bool TargetInAggroRange(this NPC npc, Entity target, float range = 240, bool lineOfSight = true, bool allowDamageTrigger = true)
		{
			if (lineOfSight && !npc.LineOfSight(target.position))
				return false;
			if (allowDamageTrigger && npc.life < npc.lifeMax)
				return true;
			return npc.DistanceSQ(target.position) < MathF.Pow(range, 2);
		}
	}
}