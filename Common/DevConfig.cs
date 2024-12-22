using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common
{
	public class DevConfig : ModConfig
	{
		//[Newtonsoft.Json.JsonIgnore]
		private static DevConfig _instance;
		//[Newtonsoft.Json.JsonIgnore]
		public static DevConfig Instance => _instance ??= Terraria.ModLoader.ModContent.GetInstance<DevConfig>();

		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("BuilderConfig")]

		[DefaultValue(false)]
		public bool BuilderMode;

		[DefaultValue(true)]
		public bool PreventExplosionDamage;

		[DefaultValue(true)]
		public bool DoPylonDiscoveries;
	}
}
