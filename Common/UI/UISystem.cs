using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaCells.Common.UI
{
    public class UISystem : ModSystem
    {
        internal UserInterface ReloadInterface;
        internal ReloaderUI reloaderUI;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                ReloadInterface = new UserInterface();

                reloaderUI = new ReloaderUI();
                reloaderUI.Activate(); // Activate calls Initialize() on the UIState if not initialized and calls OnActivate, then calls Activate on every child element.
            }
        }
        public override void Unload()
        {
            base.Unload();
        }
        internal void ShowReloadUI()
        {
            ReloadInterface?.SetState(reloaderUI);
        }

        internal void HideReloadUI()
        {
            ReloadInterface?.SetState(null);
        }
        private GameTime _lastUpdateUiGameTime;
        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (ReloadInterface?.CurrentState != null)
            {
                ReloadInterface.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "TerrariaCells: ReloadUI",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && ReloadInterface?.CurrentState != null)
                        {
                            ReloadInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
