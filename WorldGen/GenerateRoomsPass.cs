using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace TerrariaCells.WorldGen;

class GenerateRoomsPass : GenPass
{
    public GenerateRoomsPass()
        : base("Generate Rooms", 1.0) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating Rooms";
        var rand = Terraria.WorldGen.genRand;

        progress.Message = "Fill in holes";

        Point[] directions = [new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)];

        Utils.GlobalPlayer.isBuilder = false;
    }
}
