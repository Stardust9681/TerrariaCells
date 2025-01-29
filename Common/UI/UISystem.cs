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
        public override void Load()
        {
            if (!Main.dedServ)
            {
                
            }
        }
        public override void Unload()
        {
            base.Unload();
        }
        
        internal GameTime _lastUpdateUiGameTime;
        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            
        }

        // removed override since CustomInterface.ModifyInterfaceLayers now effectively manages calling this.
        public new void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (mouseTextIndex != -1)
            {
                
            }
        }
    }
}
