using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrariaCells.Common.Utilities
{
	public static class NumberHelpers
	{
		//Time

		///<returns> <paramref name="sec"/>*60 as a measure of internal ticks </returns>
		///<remarks> To hopefully avoid N*60 repetition </remarks>
		public static int SecToFrames(this int sec) => sec * 60;

		//Distance

		/// <returns> <paramref name="tileCount"/>*16 as a measure of tile distance </returns>
		/// <remarks> To hopefully avoid N*16 repetition </remarks>
		public static float ToTileDist(float tileCount) => tileCount * 16f;
		///<inheritdoc cref="ToTileDist(float)"/>
		public static float ToTileDist(int tileCount) => tileCount * 16f;
		///<returns> <paramref name="val"/> squared </returns>
		///<remarks> To hopefully avoid N*N repetition, or worse </remarks>
		public static float Squared(this float val) => val * val;
		///<inheritdoc cref="Squared(float)"/>
		public static float Squared(this int val) => val * val;
		///<returns> The signed direction to move from <paramref name="from"/> to <paramref name="to"/> </returns>
		///<remarks> To hopefully make measures of direction from A to B more clear </remarks>
		public static float DirectionFromTo(float from, float to) => to.CompareTo(from); //Help name this?
		///<returns> The absolute difference between A and B </returns>
		///<remarks> To hopefully make distance comparisons more obviously that </remarks>
		public static float Distance(float a, float b) => MathF.Abs(a - b);

		//Random
		public static int NextDirection(this Terraria.Utilities.UnifiedRandom random) => random.Next(2) * 2 - 1;
	}
}
