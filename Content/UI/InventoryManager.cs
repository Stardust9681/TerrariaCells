using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaCells.Common;

namespace TerrariaCells.Content.UI;

[Autoload(Side = ModSide.Client)]
public class InventoryManager : ModSystem
{
    const int INVENTORY_SLOT_COUNT = 4;

    Player player;
    InventoryUiConfiguration config;

    public override void OnWorldLoad()
    {
        player = Main.LocalPlayer;

        config = (InventoryUiConfiguration)Mod.GetConfig("InventoryUiConfiguration");
        if (config == null)
        {
            Logging.PublicLogger.Error("No config file found!");
            return;
        }

        for (int i = INVENTORY_SLOT_COUNT; i < Main.hotbarScale.Length; i++)
            Main.hotbarScale[i] = 0;

    }

    public override void PreUpdateItems()
    {
        if (player.selectedItem < INVENTORY_SLOT_COUNT) return;

        if (player.selectedItem > INVENTORY_SLOT_COUNT - 1 + (10 - INVENTORY_SLOT_COUNT) / 2)
            player.selectedItem = INVENTORY_SLOT_COUNT - 1;
        else player.selectedItem = 0;
    }

    public override void PreUpdatePlayers()
    {
        if (config != null)
        {
            if (config.EnableInventoryLock)
            {
                if (IsInventoryFull())
                {
                    player.preventAllItemPickups = true; // TODO: Figure out why this doesn't block item pickups
                }
                else
                {
                    player.preventAllItemPickups = false;
                }
            }
        }
    }

    private bool IsInventoryFull()
    {
        for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
        {
            if (player.inventory[i].IsAir)
            {
                return false;
            }
        }
        return true;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        PreUpdatePlayers();

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (mouseTextIndex == -1)
        {
            return;
        }

        LegacyGameInterfaceLayer newLayer = new LegacyGameInterfaceLayer(
            "YourMod: A Description",
            delegate
            {
                return true;
            },
            InterfaceScaleType.UI
        );

        layers.Insert(mouseTextIndex, newLayer);
    }
}
