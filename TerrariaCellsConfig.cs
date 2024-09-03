using Terraria.ModLoader.Config;

namespace TerrariaCells
{
    public class TerrariaCellsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static TerrariaCellsConfig Instance;

        public bool DisableZoom;
    }
}