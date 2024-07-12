using Stubble.Core.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using TerrariaCells.Content;

namespace ModTesting.Content.Items
{
    public class ModifierData
    {
        public string name;
        public string description;

        public ModifierData(string name, string description) 
        {
            this.name = name;
            this.description = description;
        }
    }

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

            ModifierInfo.Add(Modifier.Burning, new ModifierData("Burning", "Burn your target on hit."));
            ModifierInfo.Add(Modifier.Electrified, new ModifierData("Electrified", "Electrocute your target on hit."));
            ModifierInfo.Add(Modifier.ExplodeOnHit, new ModifierData("Exploding", "Explode your target on hit."));
        }

        public static ModifierData GetModifierData(Modifier modifier)
        {
            return ModifierInfo[modifier];
        }
    }

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
            itemModifiers.Add(ModifierSystem.Modifier.ExplodeOnHit);
        }

        public void AddModifiers(List<ModifierSystem.Modifier> modifiers)
        {
            foreach(ModifierSystem.Modifier modifier in modifiers)
            {
                itemModifiers.Add(modifier);
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(itemModifiers.Count);

            List<int> integerList = itemModifiers.Select(x => Convert.ToInt32(x)).ToList();

            for (int i = 0; i < integerList.Count; i++)
            {
                writer.Write(integerList[i]);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            int itemCount = reader.ReadInt32();

            List<int> integerList = new List<int>();

            for (int i = 0; i < itemCount; i++)
            {
                integerList.Add(reader.ReadInt32());
            }

            List<ModifierSystem.Modifier> enumList = integerList.Select(x => (ModifierSystem.Modifier)Enum.Parse(typeof(ModifierSystem.Modifier), x.ToString())).ToList();
            itemModifiers = enumList;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            List<int> integerList = itemModifiers.Select(x => Convert.ToInt32(x)).ToList();
            tag.Add("modifiers", integerList);
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            itemModifiers = new List<ModifierSystem.Modifier>();

            List<ModifierSystem.Modifier> enumList = tag.GetList<int>("modifiers").Select(x => (ModifierSystem.Modifier)Enum.Parse(typeof(ModifierSystem.Modifier), x.ToString())).ToList();
            itemModifiers = enumList;
        }

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            ModifierGlobalItem myClone = (ModifierGlobalItem)base.Clone(item, itemClone);

            myClone.itemModifiers = itemModifiers;

            return myClone;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (itemModifiers.Count >= 1)
            {
                foreach(ModifierSystem.Modifier modifier in itemModifiers)
                {
                    ModifierData data = ModifierSystem.GetModifierData(modifier);

                    tooltips.Add(new TooltipLine(Mod, "Modifer", data.name + ": " + data.description ));
                }

            }
        }
    }
}
