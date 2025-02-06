using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common.Configs
{
    public class TerrariaCellsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static TerrariaCellsConfig Instance;

        public bool DisableZoom;

        [Header("SkillUI")]

        [DefaultValue(true)]
        public bool ShowKeybind;

        [DefaultValue(true)]
        public bool ShowCooldown;
    }
}