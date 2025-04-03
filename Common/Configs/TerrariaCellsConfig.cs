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
		[DrawTicks]
		public DebuffIndicators IndicatorType;

		[DefaultValue(0.8f)]
		[Increment(0.05f)]
		public float EnemyDebuffOpacity;

		[DefaultValue(-8)]
		[Range(-64, 64)]
		[Slider]
		public int EnemyDebuffOffset;

        [Header("SkillUI")]

        [DefaultValue(true)]
        public bool ShowKeybind;

        [DefaultValue(true)]
        public bool ShowCooldown;
    }
}