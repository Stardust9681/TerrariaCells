using Terraria.IO;
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
        Utils.GlobalPlayer.isBuilder = false;
    }

    public void GenerateRooms() { }
}

enum LevelGenerationMode {
    Default,
    
}
