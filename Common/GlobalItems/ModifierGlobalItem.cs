using Stubble.Core.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// Data structure for the new modifier system - includes name, tooltip, and tooltip color
    /// </summary>
    public class ModifierData
    {
        public string name;
        public string tooltip;
        public Color tooltipColor;

        // Constructor with white default tooltip color
        public ModifierData(string name, string tooltip)
        {
            this.name = name;
            this.tooltip = tooltip;
            tooltipColor = Color.White;
        }

        // Constructor with specified tooltip color
        public ModifierData(string name, string tooltip, Color tooltipColor)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.tooltipColor = tooltipColor;
        }
    }

    /// <summary>
    /// The underlying system for the modifier system - includes modifier enum, dictionary holding modifier data, and relevant get function
    /// </summary>
    public class ModifierSystem : ModSystem
    {
        public enum Modifier
        {
            Burning,
            Electrified,
            ExplodeOnHit
        }

        public static Dictionary<Modifier, ModifierData> ModifierInfo;

        public override void Load()
        {
            ModifierInfo = new Dictionary<Modifier, ModifierData>();

            ModifierInfo.Add(Modifier.Burning, new ModifierData("Burning", "Burn your target on hit.", Color.Red));
            ModifierInfo.Add(Modifier.Electrified, new ModifierData("Electrified", "Electrocute your target on hit.", Color.Yellow));
            ModifierInfo.Add(Modifier.ExplodeOnHit, new ModifierData("Exploding", "Explode your target on hit.", Color.Yellow));
        }

        /// <summary>
        /// Retrieves the modifier data for the given modifier enum
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static ModifierData GetModifierData(Modifier modifier)
        {
            return ModifierInfo[modifier];
        }
    }

    /// <summary>
    /// Adds modifiers 
    /// </summary>
    public class ModifierGlobalItem : GlobalItem
    {
        public List<ModifierSystem.Modifier> itemModifiers = new List<ModifierSystem.Modifier>();

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0;
        }

        public override void PostReforge(Item item)
        {
            //itemModifiers.Add(ModifierSystem.Modifier.ExplodeOnHit);
            //itemModifiers.Add(ModifierSystem.Modifier.Burning);
            itemModifiers.Add(ModifierSystem.Modifier.Electrified);
        }

        /// <summary>
        /// Adds the given list of modifiers to this item
        /// </summary>
        /// <param name="modifiers"></param>
        public void AddModifiers(List<ModifierSystem.Modifier> modifiers)
        {
            foreach (ModifierSystem.Modifier modifier in modifiers)
            {
                itemModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// Adds the given modifier to this item
        /// </summary>
        /// <param name="modifier"></param>
        public void AddModifier(ModifierSystem.Modifier modifier)
        {
            itemModifiers.Add(modifier);
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            // Send the number of modifiers - this is necessary so we know how many bytes to read on receive
            writer.Write(itemModifiers.Count);

            // Convert list of modifiers to list of integers (for writing primitives)
            List<int> integerList = itemModifiers.Select(x => Convert.ToInt32(x)).ToList();

            // Send enum data for each modifier, as an integer
            for (int i = 0; i < integerList.Count; i++)
            {
                writer.Write(integerList[i]);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            //Receive the number of modifiers
            int itemCount = reader.ReadInt32();

            List<int> integerList = new List<int>();

            // Retrieve enum data for each modifier, as an integer
            for (int i = 0; i < itemCount; i++)
            {
                integerList.Add(reader.ReadInt32());
            }
            
            // Convert integer list back to list of modifiers
            List<ModifierSystem.Modifier> enumList = integerList.Select(x => (ModifierSystem.Modifier)Enum.Parse(typeof(ModifierSystem.Modifier), x.ToString())).ToList();
            itemModifiers = enumList;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            // Convert and Send list of modifiers to list of integers (for writing primitives)
            List<int> integerList = itemModifiers.Select(x => Convert.ToInt32(x)).ToList();
            tag.Add("modifiers", integerList);
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            itemModifiers = new List<ModifierSystem.Modifier>();

            // Retrieve and convert integer list back to list of modifiers
            List<ModifierSystem.Modifier> enumList = tag.GetList<int>("modifiers").Select(x => (ModifierSystem.Modifier)Enum.Parse(typeof(ModifierSystem.Modifier), x.ToString())).ToList();
            itemModifiers = enumList;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // If the item has modifiers, display them along with their corresponding data
            if (itemModifiers.Count >= 1)
            {
                foreach (ModifierSystem.Modifier modifier in itemModifiers)
                {
                    ModifierData data = ModifierSystem.GetModifierData(modifier);

                    TooltipLine tooltip = new TooltipLine(Mod, "Modifer", data.name + ": " + data.tooltip);
                    tooltip.OverrideColor = data.tooltipColor;

                    tooltips.Add(tooltip);
                }

            }
        }
    }
}
