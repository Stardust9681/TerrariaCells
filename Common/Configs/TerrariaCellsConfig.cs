using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common.Configs
{
    public class TerrariaCellsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static TerrariaCellsConfig Instance;

		[Header("Gameplay")]

        public bool DisableZoom;

		[FlagsAttribute]
		public enum DebuffIndicators
		{
			None = 0,				//0b00
			Icon = 1,				//0b01
			Particles = 2,			//0b10
			IconAndParticles = 3,	//0b11
		}
		[DefaultValue(DebuffIndicators.IconAndParticles)]
		public DebuffIndicators IndicatorType;

        [Header("SkillUI")]

        [DefaultValue(true)]
        public bool ShowKeybind;

        [DefaultValue(true)]
        public bool ShowCooldown;
    }
}