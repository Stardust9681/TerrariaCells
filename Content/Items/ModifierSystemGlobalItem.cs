using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
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

    public class ModifierSystemGlobalItem : GlobalItem
    {
        public List<ModifierData> modifiers = new List<ModifierData>();

        public Dictionary<int, ModifierData> ModifierDict;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0;
        }

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            ModifierSystemGlobalItem myClone = (ModifierSystemGlobalItem)base.Clone(item, itemClone);

            myClone.modifiers = modifiers;

            return myClone;
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            modifiers.Add(new ModifierData("Test Name", "Test Description"));
            modifiers.Add(new ModifierData("Test Name2", "Test Description2"));
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (modifiers.Count >= 1)
            {
                Mod.Logger.Debug(item.Name + " has modifiers");

                for (int i = 0; i < modifiers.Count; i++)
                {
                    tooltips.Add(new TooltipLine(Mod, "Modifer", "Modifier: " + modifiers[i].ToString()));
                }

                /// NEXT STEPS: ADD AN ACTUAL MODIFIER CLASS TO STORE MODIFIER INFO
                /// THEN, DETERMINE HOW I WANT TO CAUSE EFFECTS BASED ON APPLIED MODIFIERS

            }
            else
            {
                Mod.Logger.Debug(item.Name + " has no modifiers");
            }
        }
    }
}
