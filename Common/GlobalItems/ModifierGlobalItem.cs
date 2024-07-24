using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// Data structure for the new modifier system - includes name, tooltip, and tooltip color
    /// </summary>
    public class ModifierData
    {
        public string name = "No Name";
        public string tooltip = "No Tooltip";
        public Color tooltipColor = Color.White;
        public float effectChance = 1f;

        // Constructor with white default tooltip color
        public ModifierData(string name, string tooltip)
        {
            this.name = name;
            this.tooltip = tooltip;
        }

        // Constructor with specified tooltip color
        public ModifierData(string name, string tooltip, Color tooltipColor)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.tooltipColor = tooltipColor;
        }

        // Constructor with specified tooltip color + effect chance
        public ModifierData(string name, string tooltip, Color tooltipColor, float effectChance)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.tooltipColor = tooltipColor;
            this.effectChance = effectChance;
        }


    }

    /// <summary>
    /// The underlying system for the modifier system - includes modifier enum, dictionary holding modifier data, and relevant get function
    /// </summary>
    public class ModifierSystem : ModSystem
    {
        /// <summary>
        /// Enum for storing all modifier internal names
        /// </summary>
        public enum Modifier
        {
            // CREATE INTERNAL NAME FOR MODIFIER HERE
            BurnOnHit,
            Electrified,
            ExplodeOnHit
        }

        /// <summary>
        /// Dictionary associating the initialized modifier data with each modifier enum
        /// </summary>
        public static Dictionary<Modifier, ModifierData> ModifierInfo;

        public override void SetStaticDefaults()
        {

            ModifierInfo = new Dictionary<Modifier, ModifierData>();

            // DEFINE ALL MODIFIER DATA HERE + ASSOCIATE WITH INTERNAL NAME (step 3: profit)

            ModifierInfo.Add(Modifier.BurnOnHit, new ModifierData(Mod.GetLocalization("BurnOnHit.Name").Value, Mod.GetLocalization("BurnOnHit.Description").Value, Color.Red, 0.5f));
            ModifierInfo.Add(Modifier.Electrified, new ModifierData(Mod.GetLocalization("Electrified.Name").Value, Mod.GetLocalization("Electrified.Description").Value, Color.Yellow));
            ModifierInfo.Add(Modifier.ExplodeOnHit, new ModifierData(Mod.GetLocalization("ExplodeOnHit.Name").Value, Mod.GetLocalization("ExplodeOnHit.Description").Value, Color.Orange));
        }

        /// <summary>
        /// Retrieves the modifier data for the given modifier enum
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static ModifierData GetModifierData(Modifier modifier)
        {
            if (ModifierInfo.ContainsKey(modifier))
            {
                return ModifierInfo[modifier];
            }
            else
            {
                return null;
            }

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

        /// <summary>
        /// Removes the given modifier from this item
        /// </summary>
        /// <param name="modifier"></param>
        public void RemoveModifier(ModifierSystem.Modifier modifier)
        {
            itemModifiers.Remove(modifier);
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
