using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Systems;

namespace TerrariaCells.Common.UI
{
    public class UIPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            DeadCellsUISystem.ToggleActive<Content.UI.RewardTracker>(true);
        }

        public override void PostUpdate()
        {
            //toggle UI so that it only shows if the player is holding a weapon that uses it. Only set state if it needs to.
            // DeadCellsUISystem uiSystem = ModContent.GetInstance<DeadCellsUISystem>();
            // if (WeaponHoldoutify.Guns.Contains(Player.HeldItem.type))
            // {
            //     if (uiSystem.ReloadInterface?.CurrentState == null)
            //     {
            // 		uiSystem.ShowReloadUI();
            //     }

            // }
            // else if (uiSystem.ReloadInterface?.CurrentState == uiSystem.reloaderUI && uiSystem.reloaderUI != null)
            // {
            // 	uiSystem.HideReloadUI();
            // }
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            PlayerDrawLayers.CaptureTheGem.Hide();
        }
    }
}
