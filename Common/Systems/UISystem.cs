using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.UI;
using TerrariaCells.Common.UI.Components;
using TerrariaCells.Common.UI.Components.Windows;

namespace TerrariaCells.Common.Systems
{
    //Imported from one of my mods
    //Inspired in large by DragonLens' UI
    // -Stardust
    public class DeadCellsUISystem : ModSystem
    {
        private static List<CustomUserInterface> Interfaces;
        private static List<WindowState> Windows;
        public static void Add<T>() where T : WindowState, new()
        {
            CustomUserInterface uInterface = new CustomInterface<T>();
            T window = new T() { UserInterface = uInterface };
            uInterface.SetState(window);
            Windows.Add(window);
            Interfaces.Add(uInterface);
        }
        public static T GetWindow<T>() where T : WindowState
        {
            try
            {
                return (T)Windows.First(x => x is T);
            }
            catch (Exception x)
            {
                ModContent.GetInstance<TerrariaCells>().Logger.Error($"Window of type '{typeof(T).FullName}' did not exist", x);
                return default(T);
            }
        }
        public static void ToggleActive<T>(bool enable) where T : WindowState
        {
            if (enable)
                GetWindow<T>()?.Open();
            else
                GetWindow<T>()?.Close();
        }

        public override void Load()
        {
            if (Main.dedServ) return;

            Interfaces = new List<CustomUserInterface>();
            Windows = new List<WindowState>();

            //Opted to do manual loading instead of automatic
            Add<LimitedStorageUI>();
            ToggleActive<LimitedStorageUI>(true);
            Add<Content.UI.RewardTracker>();
            ToggleActive<Content.UI.RewardTracker>(true);
            Add<Content.UI.Reload>();
        }
        public override void Unload()
        {
            Interfaces.Clear();
            Interfaces = null;
            Windows.Clear();
            Windows = null;
        }

        public override void PostUpdatePlayers()
        {
            if (!Main.dedServ)
            {
                if (Main.mapStyle != 0)
                    Main.mapStyle = 0;
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.ingameOptionsWindow || Main.InGameUI.IsVisible) return;
            foreach (CustomUserInterface uInterface in Interfaces)
            {
                if (uInterface?.CurrentState != null && uInterface.CurrentState.Active)
                    uInterface.Update(gameTime);
            }
        }

        private static readonly HashSet<string> RemoveLayers = new HashSet<string>() {
            "Vanilla: Laser Ruler",
            "Vanilla: Ruler",
            "Vanilla: Inventory",
            "Vanilla: Info Accessories Bar",
            "Vanilla: Hotbar",
        };
        private static void HideVanillaInventoryLayers(List<GameInterfaceLayer> layers)
        {
            if (!DevConfig.Instance.HideVanillaInventory)
                return;

            layers.RemoveAll(
                delegate (GameInterfaceLayer layer) {
                    return RemoveLayers.Contains(layer.Name);
                }
            );
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            HideVanillaInventoryLayers(layers);
            for (int i = 0; i < Windows.Count; i++)
            {
                WindowState window = Windows[i];

                int insertionIndex = layers.FindIndex(x => x.Name.Equals(window.InsertionIndex));
                if (insertionIndex == -1) continue;

                layers.Insert(
                insertionIndex,
                new LegacyGameInterfaceLayer(
                    window.Name,
                    delegate {
                        if (window.Active) //Window.Draw calls PreDraw, DrawSelf (PreDrawSelf, PostDrawSelf), and DrawChildren (PreDrawChildren)
                            window.Draw(Main.spriteBatch); //Use literally any of those if you need to modify how your UI State is drawn
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}