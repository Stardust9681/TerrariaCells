using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common
{
    public class Detours : ModSystem
    {
        public override void Load()
        {
            Terraria.On_Main.DoDraw_UpdateCameraPosition += On_Main_DoDraw_UpdateCameraPosition;
            On_Player.PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool += On_Player_PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool;
        }
        //fix EXTRMELEY strange bug with phantom phoenix
        private void On_Player_PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool(On_Player.orig_PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool orig, Player self, Item sItem, ref int projToShoot, ref float speed, ref bool canShoot, ref int totalDamage, ref float KnockBack, out int usedAmmoItemId, bool dontConsume)
        {
            if (sItem.type == ItemID.DD2PhoenixBow)
            {
                projToShoot = ProjectileID.FireArrow;
                usedAmmoItemId = ItemID.FlamingArrow;
                return;
            }
            orig(self, sItem, ref projToShoot, ref speed, ref canShoot, ref totalDamage, ref KnockBack, out usedAmmoItemId, dontConsume);
        }

        //kill scope effect
        private void On_Main_DoDraw_UpdateCameraPosition(Terraria.On_Main.orig_DoDraw_UpdateCameraPosition orig)
        {
            int originalType = Main.player[Main.myPlayer].inventory[Main.player[Main.myPlayer].selectedItem].type;
            Main.player[Main.myPlayer].inventory[Main.player[Main.myPlayer].selectedItem].type = 0;
            orig();
            Main.player[Main.myPlayer].inventory[Main.player[Main.myPlayer].selectedItem].type = originalType;
        }
    }
}
