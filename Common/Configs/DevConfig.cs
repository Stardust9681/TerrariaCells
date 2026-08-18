using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common.Configs
{
	public class DevConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		[Newtonsoft.Json.JsonIgnore]
		private static DevConfig _instance;
		[Newtonsoft.Json.JsonIgnore]
		public static DevConfig Instance => Terraria.ModLoader.ModContent.GetInstance<DevConfig>();


        [Header("GameplaySettings")]
        ///<summary>Toggle ability to build. Also allows tiles to be interacted with normally.</summary>
        [DefaultValue(false)]
        public bool BuilderMode = false;
        
        [Header("BuilderSettings")]

        /// <summary>Toggle NPC spawns (added to allow builders to build in peace)</summary>
        [DefaultValue(false)]
        public bool DisableSpawns = false;

        [DefaultValue(true)]
        public bool DisableUsingMouseItem = true;

        /// <summary>Prevents tile damage from explosives.</summary>
        [DefaultValue(true)]
        public bool PreventExplosionDamage = true;

        /// <summary>Toggles intended Pylon mechanics.</summary>
        [DefaultValue(true)]
        public bool DoPylonDiscoveries = true;

        /// <summary>When active, drops all of the players items onto the ground on death.</summary>
        [DefaultValue(true)]
        public bool DropItems = true;

        /// <summary>When active, replaces vanilla world generation with custom worldgen.</summary>
        [DefaultValue(true)]
        public bool EnableCustomWorldGen = true;

        [Header("InventorySettings")]
        /// <summary>Effectively controls whether this mod affects the interface.</summary>
        [DefaultValue(true)]
        public bool EnableInventoryChanges = true;

        /// <summary>If enabled, activate the chest limiting feature that disables the chest interface and drops items from chests instead.</summary>
        [DefaultValue(true)]
        public bool EnableChestChanges = true;

        /// <summary>Since the default inventory is used and manipulated in this mod, you can disable that behaviour here if you wish.</summary>
        [DefaultValue(true)]
        public bool EnableInventoryLock = true;

        /// <summary>
        /// Disables the interfaces that show the inventory.
        /// <para>Note that this disables the functionality of the visible inventory as well.</para>
        /// </summary>
        [DefaultValue(true)]
        public bool HideVanillaInventory = true;

        [Header("DebugAndPlaytesting")]

        /// <summary>
        /// If enabled, makes it so that NPC shops sell all available items for their category instead of just a small selection.
        /// Only for testing purposes.
        /// <summary>
        [DefaultValue(false)]
		public bool PlaytesterShops = false;

        /// <summary>
        /// If true, shows the categorization of an item in it's tooltips.
        /// Can be disabled for a cleaner list.
        /// </summary>
        [DefaultValue(true)]
        public bool ListCategorizationTooltip = true;

        /// <summary>
        /// Allow the use of testing commands (like <c>/die</c> or <c>/tier {lv}</c>)
        /// </summary>
        [DefaultValue(false)]
        public bool AllowDebugCommands = false;

        [Header("PylonVisuals")]
        /// <summary>
        /// Hides inactive Pylons entirely. Overrides every other setting if enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool HideInactivePylonsEntirely = false;
        /// <summary>
        /// Whether inactive Pylons should be greyed out instead of their normal colour
        /// </summary>
        [DefaultValue(false)]
        public bool GreyInactivePylons = true;
        /// <summary>
        /// Whether inactive Pylons should be translucent instead of fully opaque
        /// </summary>
        [DefaultValue(true)]
        public bool TranslucentInactivePylons = true;
        /// <summary>
        /// Whether translucent inactive Pylons should be darkened, as if actuated. Has no effect if Translucent Inactive Pylons is not set
        /// </summary>
        [DefaultValue(true)]
        public bool DarkenTranslucentPylons = true;
        /// <summary>
        /// Whether inactive Pylons should bob up and down
        /// </summary>
        [DefaultValue(false)]
        public bool BobbingInactivePylons = false;
        /// <summary>
        /// Whether inactive Pylons should spin
        /// </summary>
        [DefaultValue(false)]
        public bool SpinInactivePylons = true;
        /// <summary>
        /// Whether inactive Pylons should have their glow
        /// </summary>
        [DefaultValue(false)]
        public bool GlowInactivePylons = false;
        /// <summary>
        /// Whether inactive Pylons should emit dust
        /// </summary>
        [DefaultValue(false)]
        public bool InactivePylonDust = false;

        [Header("AmmoUIPlacement")]
        [DefaultValue(Content.UI.Reload.AmmoPositionMode.Resource)]
        [DrawTicks]
        public Content.UI.Reload.AmmoPositionMode AmmoIndicatorType = Content.UI.Reload.AmmoPositionMode.Resource;
    }
}
