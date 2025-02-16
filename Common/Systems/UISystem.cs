using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.UI;

public class DeadCellsUISystem : ModSystem
{
    static readonly string[] filtered_layers =
    [
        "Vanilla: Laser Ruler",
        "Vanilla: Ruler",
        "Vanilla: Inventory",
        "Vanilla: Info Accessories Bar",
        "Vanilla: Hotbar",
    ];

    internal UserInterface limitedStorageInterface;
    internal LimitedStorageUI limitedStorageUI;
    internal UserInterface ReloadInterface;

    // internal ReloaderUI reloaderUI;

    internal GameTime _lastUpdateUiGameTime;

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        limitedStorageInterface = new UserInterface();
        limitedStorageUI = new LimitedStorageUI();
        limitedStorageUI.Activate();
        limitedStorageInterface.SetState(limitedStorageUI);

        ReloadInterface = new UserInterface();
        ReloadInterface.SetState(null);
        // reloaderUI = new ReloaderUI();
    }

    public override void Unload()
    {
        limitedStorageUI = null;
    }

    internal void ShowReloadUI()
    {
        // ReloadInterface?.SetState(reloaderUI);
    }

    internal void HideReloadUI()
    {
        ReloadInterface?.SetState(null);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (ReloadInterface?.CurrentState != null)
        {
            ReloadInterface.Update(gameTime);
        }

        if (!DevConfig.Instance.EnableInventoryChanges)
        {
            return;
        }

        if (limitedStorageInterface?.CurrentState == null)
        {
            limitedStorageInterface.Update(gameTime);
        }
    }

    public void HideVanillaInventoryLayers(List<GameInterfaceLayer> layers)
    {
        if (!DevConfig.Instance.HideVanillaInventory)
        {
            return;
        }

        layers.RemoveAll(
            delegate(GameInterfaceLayer layer)
            {
                return filtered_layers.Contains(layer.Name);
            }
        );
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // called here before the filtering of vanilla inventory layers since somethign dumb happens when filtering and idk what
        //ModContent.GetInstance<UISystem>().ModifyInterfaceLayers(layers);

        HideVanillaInventoryLayers(layers);

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(
                mouseTextIndex,
                new LegacyGameInterfaceLayer(
                    "TerraCells: ReloadUI",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && ReloadInterface?.CurrentState != null)
                        {
                            ReloadInterface?.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                )
            );

            if (!DevConfig.Instance.EnableInventoryChanges)
            {
                return;
            }

            layers.Insert(
                mouseTextIndex,
                new LegacyGameInterfaceLayer(
                    "TerraCells: InventoryUI",
                    delegate
                    {
                        LimitedStorageUI.CustomGUIHotbarDrawInner(); // draws hotbar, aka inventory closed
                        LimitedStorageUI.CustomDrawInterface_27_Inventory(); // draws inventory, aka inventory open
                        return true;
                    },
                    InterfaceScaleType.UI
                )
            );
        }
    }
}
