using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace TerrariaCells.Common.GlobalNPCs
{
    public class GuideDialogue : GlobalNPC
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
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.Guide;
        }

        private static readonly string[] HelpText = new string[] {
            "When you're ready to begin your adventure, head out past the pylon.",
            "You can use pylons to travel back to somewhere you've been in the same map. Walking near one will activate it, allowing you to teleport to it from other pylons.",
            "You have five hotbar slots - two for weapons, two for skills, and one for healing potions.",
            "You can find weapons in gold chests. Many of them will have conditions that, if fulfilled, will make them much more powerful. For instance, the Volcano will cause any oiled enemies to explode, dealing a critical hit.",
            "Melee weapons will heal some of your lost HP if you strike an enemy shortly after taking damage.",
            "You can hold right click to charge bows, significantly increasing their power. You can also reload guns with right click.",
            "Magic attacks cause enemies to drop mana stars when hit. Keep an eye on mana costs - powerful weapons will consume a lot more than they create and need to be combined with something that can generate mana to be effective.",
            "The third and fourth slots are for skills. These are attacks, buffs, or temporary effects with a cooldown. You can set a custom keybind to use them without swapping to them.",
            "The last slot is for healing potions. These are quite rare, so use them wisely!",
            "Additionally, you have four inventory slots that can hold any item.",
            "The world out there is filled with treasure. You can smash pots with any weapon, while chests can be opened by right clicking.",
            "Life crystals can be right clicked to raise your maximum HP by 20.",
            "Pressure plates are usually activated by walking on them. Some will need to be shot, and yellow pressure plates can only be triggered by an enemy walking on them.",
            "Most enemies can be walked through safely as long as they aren't in the middle of an attack.",
            "That's all I can teach you. If you want me to repeat this, just ask."
        };
        private void On_Main_HelpText(On_Main.orig_HelpText orig)
        {
            if (Main.helpText < 0 || Main.helpText > HelpText.Length - 1) Main.helpText = 0;
            Main.npcChatText = HelpText[Main.helpText];
            Main.helpText++;
        }
        private void On_Main_DrawNPCChatButtons(On_Main.orig_DrawNPCChatButtons orig, int superColor, Color chatColor, int numLines, string focusText, string focusText3)
        {
            if (Main.LocalPlayer.TalkNPC.type == NPCID.Guide)
            {
                focusText3 = string.Empty;
            }
            Main.LocalPlayer.currentShoppingSettings.HappinessReport = "";
            orig.Invoke(superColor, chatColor, numLines, focusText, focusText3);
        }
    }
}
