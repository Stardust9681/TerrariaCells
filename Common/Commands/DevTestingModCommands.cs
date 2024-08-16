﻿using System;
using System.Text;
using Terraria;
using Terraria.Chat;
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
}
