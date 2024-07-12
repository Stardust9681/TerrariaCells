using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    //ModPlayer for buff/debuff related stuff
    //include all fields for doing more DoT damage/gaining DoT resist
    //include special effects of buffs that need more code than what ModBuff.Update can do
    public class BuffNPC : GlobalNPC
    {
        //list indexed by player whoami of timers
        //any index with a value greater than 0 means that player's debuff multipliers will proc on this npc
        //every number here is decremented every frame
        public int[] PlayerTags;
        public override void SetDefaults(NPC entity)
        {
            PlayerTags = new int[Main.maxPlayers];
            base.SetDefaults(entity);
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            //if the projectile is owned by a player, tag the npc with that player for 3 seconds
            if (projectile.owner > -1) PlayerTags[projectile.owner] = 180;
            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            //when hit by melee tag npc with player for 6 seconds
            if (player.active) PlayerTags[player.whoAmI] = 360;
            base.OnHitByItem(npc, player, item, hit, damageDone);
        }
        public override bool PreAI(NPC npc)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (PlayerTags[i] > 0) PlayerTags[i]--;
            }
            return base.PreAI(npc);
        }

        //debuffs of x type deal more/less damage to the npc
        //num less than 1 = take more damage
        //less than 0 = heal? lol.
        public float GlobalDebuffResist = 1;
        public float FireDebuffResist = 1;
        public float IceDebuffResist = 1;
        public float EvilDebuffResist = 1;
        public float HolyDebuffResist = 1;
        public float BloodDebuffResist = 1;
        public float PoisonDebuffResist = 1;
        public float NaturalDebuffResist = 1;
        public float ElectricDebuffResist = 1;
    }
    
}
