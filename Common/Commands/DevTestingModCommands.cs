using System;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.Commands
{
    /// <summary>
    /// Command for modifying weapons tier, for testing
    /// </summary>
    public class DamageCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "damage";

        public override string Usage => "/damage <value>" +
            "\ndamage - damage value to give held weapon, or \"reset\" to reset damage";
        public override string Description => "change or set weapons tier";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!Common.Configs.DevConfig.Instance.AllowDebugCommands)
            {
                caller.Reply("Debug Commands are disabled");
                return;
            }
            if (args.Length < 1)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Too few arguments!"), Color.Yellow);
                return;
            }
            if (args[0] == "reset")
            {
                caller.Player.HeldItem.damage = caller.Player.HeldItem.OriginalDamage;
                return;
            }
            try
            {
                int dmg = int.Parse(args[0]);
                caller.Player.HeldItem.damage = dmg;
            }
            catch (Exception e)
            {
                Main.NewText(e);
            }
        }
    }

    /// <summary>
    /// Command for applying modifiers to items in your inventory, for testing purposes
    /// </summary>
    public class ModifierCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "modifier";

        public override string Usage
            => "/modifier <'add' or 'remove'> <slot> <modifier>" +
            "\n 'add' or 'remove' — add or remove modifiers?" +
            "\n slot — inventory slot with weapon to apply modifier on" +
            "\n modifier — modifier enum name or dictionary index" +
            "\n \n '/modifier list' will provide a list of all modifiers" +
            "\n '/modifier info <modifier>' will provide additional info about a specific modifier";

        public override string Description
            => "Command to change modifiers on an item or receive information about modifiers";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!Common.Configs.DevConfig.Instance.AllowDebugCommands)
            {
                caller.Reply("Debug Commands are disabled");
                return;
            }
            // Checking input Arguments
            if (args.Length == 0)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("At least one argument was expected"), Color.Yellow);
                return;
            }

            // Run different variations of the modifier command based on the first argument
            switch (args[0].ToLower())
            {
                // Info about a specific modifier
                case "info":
                    {
                        if (args.Length < 2)
                        {
                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("No modifier was provided for argument 2"), Color.Yellow);
                            return;
                        }

                        string[] str = Enum.GetNames(typeof(ModifierSystem.Modifier));
                        ModifierData data;

                        ModifierSystem.Modifier modifier = (ModifierSystem.Modifier)Enum.Parse(typeof(ModifierSystem.Modifier), args[1]);

                        data = ModifierSystem.GetModifierData(modifier);

                        if (data == null)
                        {
                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Invalid modifier was provided, use '/modifier list' to see a list of all modifiers"), Color.Yellow);
                            return;
                        }

                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Enum.GetName(typeof(ModifierSystem.Modifier), modifier) + "\n  Name: " + data.name + "\n  Tooltip: " + data.tooltip), Color.White);
                        return;
                    }

                // Listing all possible modifiers
                case "list":
                    {
                        StringBuilder modifierList = new StringBuilder("Modifiers: \n");

                        foreach (string modifier in Enum.GetNames(typeof(ModifierSystem.Modifier)))
                        {
                            var value = (int)Enum.Parse(typeof(ModifierSystem.Modifier), modifier);

                            modifierList.AppendLine(modifier);
                        }

                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(modifierList.ToString()), Color.White);
                        return;
                    }

                // Adding or removing modifiers
                case "add":
                case "remove":

                    if (args.Length < 2)
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("No inventory slot was provided for argument 2"), Color.Yellow);
                        return;
                    }

                    if (int.TryParse(args[1], out int slot))
                    {
                        Item inventoryItem = caller.Player.inventory[slot - 1];

                        if (inventoryItem.TryGetGlobalItem(out ModifierGlobalItem modifierItem))
                        {
                            if (args.Length < 3)
                            {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("No Modifier was provided, use '/modifier list' to see a list of all modifiers"), Color.Yellow);
                                return;
                            }

                            if (Enum.TryParse(args[2], out ModifierSystem.Modifier modifier))
                            {

                                ModifierData data = ModifierSystem.GetModifierData(modifier);

                                if (data == null)
                                {
                                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Invalid modifier dictionary index, consider using a modifier enum from '/modifier list' instead"), Color.Yellow);
                                    return;
                                }

                                if (args[0].ToLower() == "add")
                                {
                                    modifierItem.AddModifier(modifier);

                                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(data.name + " modifier was added to " + inventoryItem.Name), Color.Green);
                                }
                                else if (args[0].ToLower() == "remove")
                                {
                                    modifierItem.RemoveModifier(modifier);

                                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(data.name + " modifier was removed from " + inventoryItem.Name), Color.Red);
                                }
                                else
                                {
                                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("'set' and 'add' are the ony acceptable parameters for the first argument"), Color.Yellow);
                                    return;
                                }
                            }
                            else
                            {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Invalid modifier was provided, use '/modifier list' to see a list of all modifiers"), Color.Yellow);
                            }

                        }
                        else
                        {
                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Item in slot provided does not have a modifier"), Color.Yellow);
                        }

                    }
                    break;
                default:
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("'list', 'info', 'add', and 'remove' are the ony acceptable parameters for the first argument"), Color.Yellow);
                    return;
            }


        }


    }

    /// <summary>
    /// Command to change or set the tier of items in your inventory, for testing purposes
    /// </summary>
    public class TierCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "tier";


        public override string Usage
            => "/tier <'set' or 'add'> <slot> <amount>" +
            "\n 'set' or 'add' — set the current tier or add tier levels (adding negative numbers lowers tier)" +
            "\n slot — inventory slot with item" +
            "\n amount — amount to add or set tier to";

        public override string Description
            => "Command used to add or set the tier of an item in your inventory";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!Common.Configs.DevConfig.Instance.AllowDebugCommands)
            {
                caller.Reply("Debug Commands are disabled");
                return;
            }
            // Checking input Arguments
            if (args.Length == 0)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("At least one argument was expected"), Color.Yellow);
                return;
            }
            else if (args.Length < 3)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Values must be provided for all three arguments"), Color.Yellow);
                return;
            }

            // Check if the amount argument is an integer
            if (int.TryParse(args[1], out int slot) && int.TryParse(args[2], out int amount))
            {
                Item inventoryItem = caller.Player.inventory[slot - 1];

                if (inventoryItem.TryGetGlobalItem(out TierSystemGlobalItem tierItem))
                {
                    if (args[0].ToLower() == "set")
                    {
                        tierItem.SetLevel(inventoryItem, amount);
                    }
                    else if (args[0].ToLower() == "add")
                    {
                        tierItem.AddLevels(inventoryItem, amount);
                    }
                    else
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("'set' and 'add' are the ony acceptable parameters for the first argument"), Color.Yellow);
                        return;
                    }
                }
                else
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Item in slot provided does not have a tier"), Color.Yellow);
                }

            }
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Non-integer values provided for slot and/or amount"), Color.Yellow);
                return;
            }

        }

    }

    /// <summary>
    /// Command to kill, hurt or get type or position of npcs
    /// </summary>
    public class NPCCommand : ModifierCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "npc";

        public override string Usage
    => "/npc <'kill', 'type' or 'ai'> <index 1> <index 2> <index 3> ... (0 or more indeces)" +
            "\n/npc <hurt> <damage> <index 1> <index 2> <index 3> ... (0 or more indeces)" +
            "\n kill — kill npcs at indeces in Main.npc (all active npcs if no index is provided)" +
            "\n hurt - hurt npcs at indeces in Main.npc (all active npcs if no index is provided)" +
            "\n type - get npc types at indeces in Main.npc (all active npcs if no index is provided)" +
            "\n ai - get the 4 entries in npc.ai at indeces in Main.npc (all active npcs if no index is provided)";

        public override string Description => "Command used to kill an npc at a specific index in Main.npc";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!Common.Configs.DevConfig.Instance.AllowDebugCommands)
            {
                caller.Reply("Debug Commands are disabled");
                return;
            }
            // Checking input Arguments
            if (args.Length < 1)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("At least one argument was expected"), Color.Yellow);
                return;
            }

            switch (args[0])
            {
                case "kill":
                    #region kill npcs

                    for (int i = 1; i < args.Length; i++)
                    {
                        if (!int.TryParse(args[i], out int index))
                        {
                            ChatHelper.BroadcastChatMessage(
                                       NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is not an integer (has to be one)"),
                                       Color.Yellow);
                            continue;
                        }
                        if (index < 0 || index >= Main.maxNPCs)
                        {
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is out of bounds of the Main.npc array (from 0 to 199 allowed)"),
                                Color.Yellow);
                            continue;
                        }

                        Main.npc[index].StrikeInstantKill();
                    }

                    if (args.Length == 1)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            Main.npc[i].StrikeInstantKill();
                        }
                    }

                    #endregion
                    return;
                case "hurt":
                    #region hurt npcs

                    if (!int.TryParse(args[1], out int damage))
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("First argument after hurt must be an integer denoting damage"), Color.Yellow);
                    }

                    for (int i = 2; i < args.Length; i++)
                    {
                        if (!int.TryParse(args[i], out int index))
                        {
                            ChatHelper.BroadcastChatMessage(
                                       NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is not an integer (has to be one)"),
                                       Color.Yellow);
                            continue;
                        }
                        if (index < 0 || index >= Main.maxNPCs)
                        {
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is out of bounds of the Main.npc array (from 0 to 199 allowed)"),
                                Color.Yellow);
                            continue;
                        }

                        Main.npc[index].life -= damage;
                    }

                    if (args.Length == 2)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            Main.npc[i].life -= damage;
                        }
                    }

                    #endregion
                    return;
                case "type":
                    #region get npc types

                    for (int i = 1; i < args.Length; i++)
                    {
                        if (!int.TryParse(args[i], out int index))
                        {
                            ChatHelper.BroadcastChatMessage(
                                       NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is not an integer (has to be one)"),
                                       Color.Yellow);
                            continue;
                        }
                        if (index < 0 || index >= Main.maxNPCs)
                        {
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is out of bounds of the Main.npc array (from 0 to 199 allowed)"),
                                Color.Yellow);
                            continue;
                        }

                        if (!Main.npc[index].active)
                        {
                            continue;
                        }

                        ChatHelper.BroadcastChatMessage(
                            NetworkText.FromLiteral("Type of npc at index " + index + " is " + NPCID.Search.GetName(Main.npc[index].type)),
                            Color.White);
                    }

                    if (args.Length == 1)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (!Main.npc[i].active)
                            {
                                continue;
                            }

                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral("Type of npc at index " + i + " is " + NPCID.Search.GetName(Main.npc[i].type)),
                                Color.White);
                        }
                    }

                    #endregion
                    return;
                case "ai":
                    #region get npc ai

                    for (int i = 1; i < args.Length; i++)
                    {
                        if (!int.TryParse(args[i], out int index))
                        {
                            ChatHelper.BroadcastChatMessage(
                                       NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is not an integer (has to be one)"),
                                       Color.Yellow);
                            continue;
                        }
                        if (index < 0 || index >= Main.maxNPCs)
                        {
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is out of bounds of the Main.npc array (from 0 to 199 allowed)"),
                                Color.Yellow);
                            continue;
                        }

                        if (!Main.npc[index].active)
                        {
                            continue;
                        }

                        NPC npc = Main.npc[index];
                        ChatHelper.BroadcastChatMessage(
                            NetworkText.FromLiteral("AI of npc at index " + index + " is this: " + npc.ai[0] + ", " + npc.ai[1] + ", " + npc.ai[2] + ", " + npc.ai[3]),
                            Color.White);
                    }

                    if (args.Length == 1)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (!Main.npc[i].active)
                            {
                                continue;
                            }

                            NPC npc = Main.npc[i];
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral("AI of npc at index " + i + " is this: " + npc.ai[0] + ", " + npc.ai[1] + ", " + npc.ai[2] + ", " + npc.ai[3]),
                                Color.White);
                        }
                    }

                    #endregion
                    return;
                case "pos":
                    #region get npc position

                    for (int i = 1; i < args.Length; i++)
                    {
                        if (!int.TryParse(args[i], out int index))
                        {
                            ChatHelper.BroadcastChatMessage(
                                       NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is not an integer (has to be one)"),
                                       Color.Yellow);
                            continue;
                        }
                        if (index < 0 || index >= Main.maxNPCs)
                        {
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral(i + 1 + (i != 0 ? i != 1 ? i != 2 ? "th" : "rd" : "nd" : "st") + " index is out of bounds of the Main.npc array (from 0 to 199 allowed)"),
                                Color.Yellow);
                            continue;
                        }

                        if (!Main.npc[index].active)
                        {
                            continue;
                        }

                        NPC npc = Main.npc[index];
                        ChatHelper.BroadcastChatMessage(
                            NetworkText.FromLiteral("Position of npc at index " + index + " is " + npc.Center),
                            Color.White);
                    }

                    if (args.Length == 1)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (!Main.npc[i].active)
                            {
                                continue;
                            }

                            NPC npc = Main.npc[i];
                            ChatHelper.BroadcastChatMessage(
                                NetworkText.FromLiteral("Position of npc at index " + i + " is " + npc.Center),
                                Color.White);
                        }
                    }

                    #endregion
                    return;
                default:
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("First argument must be 'kill', 'hurt', 'type', 'ai' or 'pos'"), Color.Yellow);
                    return;
            }
        }
    }
}
