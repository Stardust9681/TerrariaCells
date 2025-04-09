using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaCells.Common.Utilities
{
    public static class NumberHelpers
    {
		public static int SecToFrames(this int sec) => sec * 60;

		public static float TicksToSec(this int ticks) => ticks / 60f;
    }
}
