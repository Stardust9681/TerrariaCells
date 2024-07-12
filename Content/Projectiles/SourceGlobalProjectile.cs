using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ModTesting.Content.Items
{
    public class SourceGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public Item itemSource;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem entitySource)
            {
                /// FIX ISSUE WHEN NPCS SHOOT SO THE ITEM TYPE ISNT A KEY
                itemSource = entitySource.Item;
            }
        }
    }
}
