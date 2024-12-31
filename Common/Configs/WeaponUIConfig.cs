using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common.Configs
{
    public class WeaponUIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [DefaultValue(false)]
        public bool DragUI;

        [DefaultValue(false)]
        public bool FadeOut;

        [DefaultValue(1.2f)]
        [Range(0f, 3f)]
        [Increment(0.1f)]
        public float Scale;
    }
}
