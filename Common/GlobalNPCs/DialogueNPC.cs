using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace TerrariaCells.Common.GlobalNPCs
{
    public class DialogueNPC : GlobalNPC
    {
        public override void Load()
        {
            On_Main.HelpText += On_Main_HelpText;
            On_Main.DrawNPCChatButtons += On_Main_DrawNPCChatButtons;
        }
        public override void Unload()
        {
            On_Main.HelpText -= On_Main_HelpText;
            On_Main.DrawNPCChatButtons -= On_Main_DrawNPCChatButtons;
        }

        private static readonly string[] HelpText = new string[] {
            //Pylons
            "When you're ready to begin your adventure, head out past the pylon.",
            "You can use pylons to travel back to somewhere you've been in the same map. Walking near one will activate it, allowing you to teleport to it from other pylons.",
            //Items
            "You have five hotbar slots - two for weapons, two for skills, and one for healing potions.",
                //Weapons
            "You can find weapons in gold chests. Many of them will have conditions that, if fulfilled, will make them much more powerful. For instance, the Volcano will cause any oiled enemies to explode, dealing a critical hit.",
            "Melee weapons will heal some of your lost HP if you strike an enemy shortly after taking damage. They'll also stun most enemies, preventing them from attacking.",
            "You can hold right click to charge bows, significantly increasing their power. You can also reload guns with right click.",
            "Charged bow shots will have extra effects. Try them out!",
            "Magic attacks cause enemies to drop mana stars when hit. Keep an eye on mana costs - powerful weapons will consume a lot more than they create and need to be combined with something that can generate mana to be effective.",
                //Abilities
            "The third and fourth slots are for skills. These are attacks, buffs, or temporary effects with a cooldown. You can set a custom keybind to use them without swapping to them.",
                //Potions
            "The last slot is for healing potions. These are quite rare, so use them wisely!",
                //Misc
            "Additionally, you have four inventory slots that can hold any item.",
            //World
            "The world out there is filled with treasure. You can smash pots with any weapon, while chests can be opened by right clicking.",
            "Life crystals can be right clicked to raise your maximum HP by 20.",
            "Pressure plates are usually activated by walking on them. Some will need to be shot, and yellow pressure plates can only be triggered by an enemy walking on them.",
            "Most levels have multiple exits, but you might need special items or upgrades to reach some of them.",
            "If you complete an area quickly, you can open a chest with a ton of treasure at the Inn Between!",
            //Enemies
            "If an enemy is glowing red, it will hurt you when touched",
            "Enemies grow stronger the further into the world you get. Be sure to look for upgraded weapons to fight them with!",
            //Multiplayer
            "Any dead players will respawn after completing the level, as long as one player survives.",
            //End
            "That's all I can teach you. If you want me to repeat this, just ask.",
        };
        private void On_Main_HelpText(On_Main.orig_HelpText orig)
        {
            if (Main.LocalPlayer.TalkNPC?.type == NPCID.Guide)
            {
                if (Main.helpText < 0 || Main.helpText > HelpText.Length - 1) Main.helpText = 0;
                Main.npcChatText = HelpText[Main.helpText];
                Main.helpText++;

                //Skip Multiplayer-specific dialogue?
                //if (Main.helpText == HelpText.Length - 2 && Main.netMode == NetmodeID.SinglePlayer)
                    //Main.helpText = HelpText.Length - 1;
            }
            else
            {
                orig.Invoke();
            }
        }
        private void On_Main_DrawNPCChatButtons(On_Main.orig_DrawNPCChatButtons orig, int superColor, Color chatColor, int numLines, string focusText, string focusText3)
        {
            if (Main.LocalPlayer.TalkNPC?.type == NPCID.Guide)
            {
                focusText3 = string.Empty;
            }
            if (Main.LocalPlayer.talkNPC != -1)
            {
                Main.LocalPlayer.currentShoppingSettings.HappinessReport = "";
            }
            orig.Invoke(superColor, chatColor, numLines, focusText, focusText3);
        }

        public override bool? CanChat(NPC npc)
        {
            if (npc.type == NPCID.BoundGoblin)
            {
                Main.LocalPlayer.GetModPlayer<Common.ModPlayers.MetaPlayer>().Goblin = true;
            }
            return base.CanChat(npc);
        }
    }
}
