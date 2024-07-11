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
    const int INVENTORY_SLOT_COUNT = 6;

    EntitySource_OverfullInventory entitySource_OverfullInventory;
    Player player;
    InventoryUiConfiguration config;

    public override void OnWorldLoad()
    {
        player = Main.player[Main.myPlayer];
        entitySource_OverfullInventory = new(player, "TerraCells limit of 6 inventory slots");
        config = (InventoryUiConfiguration)Mod.GetConfig("InventoryUiConfiguration");
        if (config == null)
        {
            Logging.PublicLogger.Error("No config file found!");
            return;
        }
    }

    public override void PreUpdatePlayers()
    {
        if (config != null)
        {
            if (config.EnableInventoryLock)
            {
                if (IsInventoryFull()) { }
                for (int i = 6; i < 50; i++)
                {
                    player.inventory[i].TurnToAir();
                }
            }
        }
    }

    private bool IsInventoryFull()
    {
        for (int i = 1; i < INVENTORY_SLOT_COUNT; i++)
        {
            if (player.inventory[i] == new Item())
            {
                return false;
            }
        }
        return true;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        PostUpdatePlayers();

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
